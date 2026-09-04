.runner_command <- function() {
    configured <- getOption("lpjguess.runner.path", Sys.getenv("LPJGUESS_RUNNER"))
    if (nzchar(configured)) {
        path <- normalizePath(configured, mustWork = TRUE)
    } else {
        candidates <- c(
            system.file("runner", "lpjg-experiment.dll",
                        package = "lpjguessRunner"),
            file.path("src", "LpjGuess.Runner.CLI", "bin", "Debug", "net9.0",
                      "lpjg-experiment.dll")
        )
        candidates <- candidates[nzchar(candidates) & file.exists(candidates)]
        if (!length(candidates)) {
            stop("Cannot find lpjg-experiment. Set option 'lpjguess.runner.path' ",
                 "or environment variable LPJGUESS_RUNNER.", call. = FALSE)
        }
        path <- normalizePath(candidates[[1L]], mustWork = TRUE)
    }
    if (grepl("\\.dll$", path, ignore.case = TRUE)) {
        list(command = "dotnet", prefix = path)
    } else {
        list(command = path, prefix = character())
    }
}

.make_request <- function(settings, simulations, instruction_files, pfts,
                          existing_output_policy) {
    if (!length(instruction_files)) {
        stop("At least one instruction file is required", call. = FALSE)
    }
    list(
        protocol_version = 1L,
        settings = settings,
        simulations = simulations,
        instruction_files = unname(as.list(normalizePath(
            instruction_files, mustWork = FALSE))),
        pfts = unname(as.list(pfts)),
        existing_output_policy = paste(existing_output_policy, collapse = ",")
    )
}

.default_progress <- function() {
    force(Sys.time())
    function(event) {
        message <- sprintf(
            "\rWorking: %.0f%% (%d of %d simulations completed)",
            event$percent, event$completed, event$total)
        cat(message)
        utils::flush.console()
    }
}

.clear_progress <- function(progress) {
    if (is.function(progress) && isTRUE(attr(progress, "lpjguess_default"))) {
        cat("\n")
        utils::flush.console()
    }
}

.normalise_progress_callback <- function(progress) {
    if (identical(progress, TRUE)) {
        callback <- .default_progress()
        attr(callback, "lpjguess_default") <- TRUE
        return(callback)
    }
    progress
}

.output_events <- function(handle) {
    Filter(function(event) identical(event$type, "output"), handle$events)
}

.dump_output_events <- function(handle) {
    events <- .output_events(handle)
    if (!length(events)) return()

    cat("LPJ-GUESS output from failed simulations:\n")
    for (event in events) {
        data <- event$data
        cat(sprintf("[%s][%s] %s\n", data$job, data$stream, data$text))
    }
    utils::flush.console()
}

#' Start LPJ-GUESS simulations asynchronously
#'
#' `run_simulations_async()` starts `lpjg-experiment` in the background and
#' returns a handle. Use [poll_run()] to consume progress and output events,
#' [wait_run()] to block until completion, or [cancel_run()] to request
#' cancellation.
#'
#' @param settings Runner settings created by [run_settings()] or
#'   [run_settings_local()].
#' @param simulations A simulation object created by [simulation()], or a list
#'   of simulation objects.
#' @param instruction_files Character vector of LPJ-GUESS instruction files.
#' @param pfts Optional character vector of PFT names to include. If provided,
#' only these PFTs will be enabled for the simulations. If empty, whichever PFTs
#' are enabled in the instruction files will be used.
#' @param existing_output_policy Policy used when generated output already
#'   exists. Common values are `"clean_managed"`, `"fail"`, and `"overwrite"`.
#'   Multiple policy flags may be supplied as a character vector.
#'
#' @return An object of class `lpjguess_run`. The handle contains process state
#'   and accumulated runner events and is intended to be passed to [poll_run()],
#'   [wait_run()], or [cancel_run()].
#'
#' @examples
#' \dontrun{
#' settings <- run_settings_local("lpjguess", "runs")
#' sims <- list(
#'   simulation("baseline"),
#'   simulation("high-sla", block_parameter("pft", "TeBE", "sla", 39))
#' )
#'
#' handle <- run_simulations_async(settings, sims, "global.ins")
#' while (poll_run(handle, timeout = 1000)) {
#'   # Update a UI, check for user cancellation, or do other work.
#' }
#' result <- wait_run(handle)
#' }
#'
#' @export
run_simulations_async <- function(settings, simulations, instruction_files,
                                  pfts = character(),
                                  existing_output_policy = "clean_managed") {
    request <- .make_request(settings, simulations, instruction_files, pfts,
                             existing_output_policy)
    request_file <- tempfile(fileext = ".json")
    jsonlite::write_json(request, request_file, auto_unbox = TRUE, null = "null")
    runner <- .runner_command()
    args <- c(runner$prefix, "run", "--request-json", request_file,
              "--events", "ndjson")
    process <- processx::process$new(runner$command, args,
                                     stdout = "|", stderr = "|",
                                     cleanup_tree = TRUE)
    handle <- new.env(parent = emptyenv())
    handle$process <- process
    handle$request_file <- request_file
    handle$events <- list()
    handle$result <- NULL
    handle$error <- NULL
    handle$stderr <- character()
    handle$request <- request
    class(handle) <- "lpjguess_run"
    reg.finalizer(handle, function(x) {
        if (x$process$is_alive()) x$process$kill_tree()
        if (file.exists(x$request_file)) unlink(x$request_file)
    }, onexit = TRUE)
    handle
}

.consume_lines <- function(handle, lines, progress, output) {
    for (line in lines[nzchar(lines)]) {
        event <- tryCatch(jsonlite::fromJSON(line, simplifyVector = FALSE),
                          error = function(e) stop(
                              "Invalid response from lpjg-experiment: ", line,
                              call. = FALSE))
        handle$events[[length(handle$events) + 1L]] <- event
        if (identical(event$type, "progress") && is.function(progress)) {
            progress(event$data)
        } else if (identical(event$type, "output") && is.function(output)) {
            output(event$data)
        } else if (identical(event$type, "completed")) {
            handle$result <- event$data
            handle$result$output_directory <- handle$request$settings$output_directory
            class(handle$result) <- c("lpjguess_result", class(handle$result))
        } else if (identical(event$type, "error")) {
            handle$error <- event$message
        } else if (identical(event$type, "cancelled")) {
            handle$error <- event$message
        }
    }
}

#' Poll, wait for, or cancel an asynchronous LPJ-GUESS run
#'
#' These functions operate on handles returned by [run_simulations_async()].
#' `poll_run()` consumes any available runner events and returns whether the
#' process is still alive. `wait_run()` blocks until the run completes and
#' returns the final result. `cancel_run()` requests cancellation and returns
#' the handle invisibly.
#'
#' @param handle An `lpjguess_run` handle returned by
#'   [run_simulations_async()].
#' @param timeout Maximum time, in milliseconds, for [poll_run()] to wait for
#'   process output before returning.
#' @param progress Progress handling. Pass `TRUE` for the default one-line
#'   progress display, `NULL` to suppress progress, or a callback function
#'   invoked with the decoded progress payload.
#' @param output Optional callback function invoked for model-output events. The
#'   callback receives one argument: the decoded event payload.
#' @param poll_interval Polling interval, in milliseconds, used by
#'   [wait_run()] while the process is alive.
#'
#' @return `poll_run()` invisibly returns `TRUE` while the process is still
#'   running and `FALSE` after it exits. `wait_run()` returns an
#'   `lpjguess_result` object and raises an R error if the runner reports a
#'   failure. `cancel_run()` invisibly returns `handle`.
#'
#' @examples
#' \dontrun{
#' handle <- run_simulations_async(settings, simulations, "global.ins")
#'
#' poll_run(
#'   handle,
#'   timeout = 1000,
#'   progress = function(event) print(event)
#' )
#'
#' cancel_run(handle)
#' }
#'
#' @export
poll_run <- function(handle, timeout = 0, progress = NULL, output = NULL) {
    stopifnot(inherits(handle, "lpjguess_run"))
    handle$process$poll_io(as.integer(timeout))
    .consume_lines(handle, handle$process$read_output_lines(), progress, output)
    errors <- handle$process$read_error_lines()
    handle$stderr <- c(handle$stderr, errors)
    invisible(handle$process$is_alive())
}

#' @rdname poll_run
#' @export
wait_run <- function(handle, progress = NULL, output = NULL,
                     poll_interval = 100) {
    stopifnot(inherits(handle, "lpjguess_run"))
    progress <- .normalise_progress_callback(progress)
    tryCatch({
        while (handle$process$is_alive()) {
            poll_run(handle, poll_interval, progress, output)
        }
        poll_run(handle, 0, progress, output)
    }, interrupt = function(e) {
        cancel_run(handle)
        stop(e)
    })
    .clear_progress(progress)
    unlink(handle$request_file)
    if (!is.null(handle$error)) {
        .dump_output_events(handle)
        stop(handle$error, call. = FALSE)
    }
    if (is.null(handle$result)) {
        details <- paste(handle$stderr, collapse = "\n")
        stop("lpjg-experiment exited without a result",
             if (nzchar(details)) paste0(":\n", details), call. = FALSE)
    }
    if (!is.null(handle$result$error) || handle$result$failed_jobs > 0L) {
        .dump_output_events(handle)
        error <- handle$result$error
        if (is.null(error) || !nzchar(error)) {
            error <- sprintf(
                "LPJ-GUESS run failed: %d of %d simulations failed.",
                handle$result$failed_jobs, handle$result$total_jobs)
        }
        stop(error, call. = FALSE)
    }
    handle$result
}

#' @rdname poll_run
#' @export
cancel_run <- function(handle) {
    stopifnot(inherits(handle, "lpjguess_run"))
    if (handle$process$is_alive()) handle$process$interrupt()
    invisible(handle)
}

#' Run LPJ-GUESS simulations synchronously
#'
#' `run_simulations()` is the simplest entry point for ordinary scripts. It
#' starts the runner, streams runner events through optional callbacks, waits
#' for completion, and returns the final result.
#'
#' @param settings Runner settings created by [run_settings()] or
#'   [run_settings_local()].
#' @param simulations A simulation object created by [simulation()], or a list
#'   of simulation objects.
#' @param instruction_files Character vector of LPJ-GUESS instruction files.
#' @param pfts Optional character vector of PFT names to include.
#' @param progress Progress handling. The default `TRUE` prints a one-line
#'   aggregate progress display. Pass `NULL` to suppress progress, or a
#'   callback function invoked with the decoded progress payload.
#' @param output Optional callback function invoked for model-output events. The
#'   callback receives one argument: the decoded event payload.
#' @param existing_output_policy Policy used when generated output already
#'   exists. Common values are `"clean_managed"`, `"fail"`, and `"overwrite"`.
#'   Multiple policy flags may be supplied as a character vector and are passed
#'   to the runner as a comma-separated value.
#'
#' @return An `lpjguess_result` object. The result contains the decoded summary
#'   returned by the runner, including total, successful, and failed job counts.
#'
#' @examples
#' \dontrun{
#' settings <- run_settings_local("lpjguess", "runs", cpu_count = 2)
#' simulations <- list(
#'   simulation("baseline"),
#'   simulation("high-sla", block_parameter("pft", "TeBE", "sla", 39))
#' )
#'
#' result <- run_simulations(
#'   settings,
#'   simulations,
#'   instruction_files = "global.ins",
#'   progress = function(event) {
#'     if (!is.null(event$message)) message(event$message)
#'   },
#'   existing_output_policy = "clean_managed"
#' )
#' print(result)
#' }
#'
#' @export
run_simulations <- function(settings, simulations, instruction_files,
                            pfts = character(), progress = TRUE, output = NULL,
                            existing_output_policy = "clean_managed") {
    handle <- run_simulations_async(
        settings, simulations, instruction_files, pfts,
        existing_output_policy)
    on.exit({
        if (handle$process$is_alive()) handle$process$kill_tree()
        if (file.exists(handle$request_file)) unlink(handle$request_file)
    }, add = TRUE)
    wait_run(handle, progress, output)
}

#' @export
print.lpjguess_result <- function(x, ...) {
    cat("LPJ-GUESS experiment result\n")
    cat("  Total jobs:     ", x$total_jobs, "\n", sep = "")
    cat("  Successful jobs:", x$successful_jobs, "\n", sep = "")
    cat("  Failed jobs:    ", x$failed_jobs, "\n", sep = "")
    invisible(x)
}

#' @export
print.lpjguess_run <- function(x, ...) {
    state <- if (x$process$is_alive()) "running" else "finished"
    cat("LPJ-GUESS run (", state, ")\n", sep = "")
    invisible(x)
}
