test_that("default progress writes one-line status", {
    progress <- .normalise_progress_callback(TRUE)

    output <- capture.output(
        progress(list(percent = 48, completed = 3L, total = 7L)),
        type = "output"
    )

    expect_true(is.function(progress))
    expect_true(isTRUE(attr(progress, "lpjguess_default")))
    expect_match(paste(output, collapse = ""), "Working: 48%")
    expect_match(paste(output, collapse = ""), "3 of 7 simulations completed")
})

test_that("dump output events writes captured stdout and stderr", {
    handle <- new.env(parent = emptyenv())
    handle$events <- list(
        list(
            type = "output",
            data = list(job = "sim1", stream = "stdout", text = "hello")
        ),
        list(
            type = "output",
            data = list(job = "sim1", stream = "stderr", text = "bad")
        )
    )

    output <- capture.output(.dump_output_events(handle), type = "output")

    expect_match(paste(output, collapse = "\n"), "\\[sim1\\]\\[stdout\\] hello")
    expect_match(paste(output, collapse = "\n"), "\\[sim1\\]\\[stderr\\] bad")
})

test_that("wait_run dumps captured output before reported errors", {
    process <- new.env(parent = emptyenv())
    process$is_alive <- function() FALSE
    process$poll_io <- function(timeout) NULL
    process$read_output_lines <- function() character()
    process$read_error_lines <- function() character()

    handle <- new.env(parent = emptyenv())
    handle$process <- process
    handle$request_file <- tempfile()
    handle$result <- NULL
    handle$error <- "failed"
    handle$stderr <- character()
    handle$events <- list(
        list(
            type = "output",
            data = list(job = "sim1", stream = "stderr", text = "bad")
        )
    )
    class(handle) <- "lpjguess_run"

    output <- capture.output(
        expect_error(wait_run(handle, progress = NULL), "failed"),
        type = "output"
    )

    expect_match(paste(output, collapse = "\n"), "\\[sim1\\]\\[stderr\\] bad")
})

.make_wait_handle <- function(total, batches = list(), events = list()) {
    handle <- new.env(parent = emptyenv())
    handle$request_file <- tempfile()
    file.create(handle$request_file)
    handle$request <- list(settings = list(output_directory = "runs"))
    handle$events <- events
    handle$result <- NULL
    handle$error <- NULL
    handle$stderr <- character()
    index <- 0L
    handle$process <- list(
        poll_io = function(timeout) NULL,
        read_output_lines = function() {
            index <<- index + 1L
            batch <- if (index <= length(batches)) batches[[index]] else list(
                list(type = "completed", data = list(total_jobs = total,
                     successful_jobs = total, failed_jobs = 0L)))
            vapply(batch, function(event) as.character(jsonlite::toJSON(
                event, auto_unbox = TRUE)), character(1))
        },
        read_error_lines = function() character(),
        is_alive = function() index <= length(batches),
        interrupt = function() handle$cancelled <- TRUE
    )
    class(handle) <- "lpjguess_run"
    handle
}

.progress_event <- function(percent, completed, total) {
    list(type = "progress", data = list(percent = percent,
         completed = completed, total = total))
}

test_that("wait_runs weights progress and preserves named results", {
    a <- .make_wait_handle(1, list(list(.progress_event(100, 1, 1))))
    b <- .make_wait_handle(3, list(list(.progress_event(20, 0, 3))))
    updates <- list()
    results <- wait_runs(list(a = a, b = b), poll_interval = 0,
        progress = function(event) updates[[length(updates) + 1L]] <<- event)
    expect_equal(updates[[1]]$percent, 40)
    expect_equal(updates[[1]]$completed, 1)
    expect_equal(updates[[1]]$total, 4)
    expect_equal(tail(updates, 1)[[1]]$percent, 100)
    expect_named(results, c("a", "b"))
    expect_s3_class(results$a, "lpjguess_result")
    expect_false(file.exists(a$request_file))
    expect_false(file.exists(b$request_file))
})

test_that("wait_runs includes consumed progress and waits for unknown totals", {
    a <- .make_wait_handle(2, list(list()),
                           list(.progress_event(50, 1, 2)))
    b <- .make_wait_handle(2, list(list(), list(.progress_event(0, 0, 2))))
    updates <- list()
    wait_runs(list(a, b), poll_interval = 0,
        progress = function(event) updates[[length(updates) + 1L]] <<- event)
    expect_equal(updates[[1]]$total, 4)
    expect_equal(updates[[1]]$percent, 50)
})

test_that("wait_runs prints a single overall progress format and supports silence", {
    text <- capture.output(wait_runs(list(.make_wait_handle(2),
                                         .make_wait_handle(3)), poll_interval = 0))
    expect_match(paste(text, collapse = ""),
                 "Working: 100% \\(5 of 5 simulations completed\\)")
    text <- capture.output(invisible(wait_runs(list(.make_wait_handle(1)),
                                      progress = NULL, poll_interval = 0)))
    expect_length(text, 0)
})

test_that("wait_runs drains other runs before raising failures", {
    a <- .make_wait_handle(1)
    a$error <- "failed"
    b <- .make_wait_handle(1, list(list(list(type = "output", data = list(
        job = "sim", stream = "stdout", text = "hello")))))
    output <- list()
    expect_error(wait_runs(list(a, b), progress = NULL, poll_interval = 0,
        output = function(event) output[[length(output) + 1L]] <<- event), "failed")
    expect_false(b$process$is_alive())
    expect_equal(output[[1]]$text, "hello")
    expect_false(file.exists(b$request_file))
})

test_that("wait_runs validates handles and cancels all on interrupt", {
    expect_error(wait_runs(list()), "non-empty list")
    expect_error(wait_runs(list(1)), "non-empty list")
    a <- .make_wait_handle(1)
    expect_error(wait_runs(list(a, a)), "distinct")
    b <- .make_wait_handle(1)
    a$process$poll_io <- function(timeout) {
        stop(structure(list(message = "interrupted", call = NULL),
                       class = c("interrupt", "condition")))
    }
    interrupted <- tryCatch(wait_runs(list(a, b), progress = NULL),
                            interrupt = function(e) TRUE)
    expect_true(interrupted)
    expect_true(a$cancelled)
    expect_true(b$cancelled)
})

test_that("requests serialize single simulations and lists as JSON arrays", {
    sim <- simulation("example",
        top_level_parameter("file_met_spinup", "spinup.nc"),
        top_level_parameter("file_met_forcing", "forcing.nc"))
    settings <- run_settings_local("guess", "runs")
    for (sims in list(sim, list(sim), list(example = sim),
                      list(first = sim, second = simulation("baseline")))) {
        request <- .make_request(settings, sims, "site.ins", "NFT",
                                 "clean_managed")
        json <- jsonlite::toJSON(request, auto_unbox = TRUE, null = "null")
        expect_match(as.character(json), '"simulations":\\[')
        decoded <- jsonlite::fromJSON(json, simplifyVector = FALSE)
        expect_equal(decoded$simulations[[1]], sim)
        expect_null(names(decoded$simulations))
        expect_length(decoded$simulations, if (identical(sims, sim)) 1L else length(sims))
    }
    request <- .make_request(settings, simulation("baseline"), "site.ins",
                             character(), "clean_managed")
    json <- jsonlite::toJSON(request, auto_unbox = TRUE, null = "null")
    expect_match(as.character(json), '"simulations":\\[\\{"name":"baseline","factors":\\[\\]\\}\\]')
})
