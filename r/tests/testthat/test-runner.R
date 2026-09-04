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
