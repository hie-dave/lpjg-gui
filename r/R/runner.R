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
            class(handle$result) <- c("lpjguess_result", class(handle$result))
        } else if (identical(event$type, "error")) {
            handle$error <- event$message
        } else if (identical(event$type, "cancelled")) {
            handle$error <- event$message
        }
    }
}

poll_run <- function(handle, timeout = 0, progress = NULL, output = NULL) {
    stopifnot(inherits(handle, "lpjguess_run"))
    handle$process$poll_io(as.integer(timeout))
    .consume_lines(handle, handle$process$read_output_lines(), progress, output)
    errors <- handle$process$read_error_lines()
    handle$stderr <- c(handle$stderr, errors)
    invisible(handle$process$is_alive())
}

wait_run <- function(handle, progress = NULL, output = NULL,
                     poll_interval = 100) {
    stopifnot(inherits(handle, "lpjguess_run"))
    tryCatch({
        while (handle$process$is_alive()) {
            poll_run(handle, poll_interval, progress, output)
        }
        poll_run(handle, 0, progress, output)
    }, interrupt = function(e) {
        cancel_run(handle)
        stop(e)
    })
    unlink(handle$request_file)
    if (!is.null(handle$error)) stop(handle$error, call. = FALSE)
    if (is.null(handle$result)) {
        details <- paste(handle$stderr, collapse = "\n")
        stop("lpjg-experiment exited without a result",
             if (nzchar(details)) paste0(":\n", details), call. = FALSE)
    }
    handle$result
}

cancel_run <- function(handle) {
    stopifnot(inherits(handle, "lpjguess_run"))
    if (handle$process$is_alive()) handle$process$interrupt()
    invisible(handle)
}

run_simulations <- function(settings, simulations, instruction_files,
                            pfts = character(), progress = NULL, output = NULL,
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

print.lpjguess_result <- function(x, ...) {
    cat("LPJ-GUESS experiment result\n")
    cat("  Total jobs:     ", x$total_jobs, "\n", sep = "")
    cat("  Successful jobs:", x$successful_jobs, "\n", sep = "")
    cat("  Failed jobs:    ", x$failed_jobs, "\n", sep = "")
    invisible(x)
}

print.lpjguess_run <- function(x, ...) {
    state <- if (x$process$is_alive()) "running" else "finished"
    cat("LPJ-GUESS run (", state, ")\n", sep = "")
    invisible(x)
}
