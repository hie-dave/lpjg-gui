write_run_catalog <- function(root) {
    site_dir <- file.path(root, "site", "baseline")
    dir.create(site_dir, recursive = TRUE)
    output_dir <- file.path(site_dir, "out")
    dir.create(output_dir)

    writeLines('simulations = ["site/baseline"]', file.path(root, "index.toml"))
    writeLines(c(
        'key = "baseline"',
        'name = "baseline"',
        paste0('base_ins = "', file.path(root, "site.ins"), '"'),
        'ins_file = "generated.ins"',
        'generated_at_utc = 2024-05-01T13:15:00Z',
        'pfts = ["TeBE"]',
        '[[factors]]',
        'name = "npatch"',
        'value = "3"',
        '',
        '[[factors]]',
        'name = "sla"',
        'value = "26.5"',
        'block_type = "pft"',
        'block_name = "TeBE"'
    ), file.path(site_dir, "manifest.toml"))
    writeLines(c(
        'outputdirectory "out"',
        'file_lai "lai.out"'
    ), file.path(site_dir, "generated.ins"))
    writeLines(c(
        "Lon Lat Year TeBE Total",
        "151.25 -33.25 2000 2.5 2.5",
        "151.25 -33.75 2000 2.6 2.6"
    ), file.path(output_dir, "lai.out"))
    writeLines(c(
        "LPJ-GUESS started",
        "LPJ-GUESS finished"
    ), file.path(site_dir, "guess.log"))
    root
}

test_that("list_simulations reads runner catalog", {
    root <- write_run_catalog(tempfile("lpjguess-runs-"))

    runs <- list_simulations(root)

    expect_s3_class(runs, "lpjguess_simulations")
    expect_equal(runs$simulation, "baseline")
    expect_equal(runs$instruction_file, "site")
    expect_equal(runs$key, "baseline")
    expect_equal(runs$pfts[[1]], "TeBE")
    expect_equal(runs$factors[[1]][[1]], top_level_parameter("npatch", 3))
    expect_equal(runs$factors[[1]][[2]],
                 block_parameter("pft", "TeBE", "sla", 26.5))
    expect_true(file.exists(file.path(runs$path, runs$ins_file)))
})

test_that("list_outputs maps file types through generated instruction file", {
    root <- write_run_catalog(tempfile("lpjguess-runs-"))

    outputs <- list_outputs(root)

    expect_s3_class(outputs, "lpjguess_outputs")
    expect_equal(outputs$simulation, "baseline")
    expect_equal(outputs$instruction_file, "site")
    expect_equal(outputs$output_type, "file_lai")
    expect_equal(outputs$file, "lai.out")
    expect_true(file.exists(outputs$path))
})

test_that("list_logs discovers guess logs", {
    root <- write_run_catalog(tempfile("lpjguess-runs-"))

    logs <- list_logs(root)

    expect_s3_class(logs, "lpjguess_logs")
    expect_equal(logs$simulation, "baseline")
    expect_equal(logs$instruction_file, "site")
    expect_true(logs$exists)
    expect_true(file.exists(logs$path))
})

test_that("read_logs returns log lines with simulation ids", {
    root <- write_run_catalog(tempfile("lpjguess-runs-"))

    logs <- read_logs(root)

    expect_s3_class(logs, "data.frame")
    expect_equal(names(logs)[1:4],
                 c("simulation", "instruction_file", "line", "text"))
    expect_equal(logs$simulation, c("baseline", "baseline"))
    expect_equal(logs$instruction_file, c("site", "site"))
    expect_equal(logs$line, 1:2)
    expect_equal(logs$text, c("LPJ-GUESS started", "LPJ-GUESS finished"))
})

test_that("read_logs can return raw character vectors", {
    root <- write_run_catalog(tempfile("lpjguess-runs-"))

    logs <- read_logs(root, combine = FALSE)

    expect_type(logs, "list")
    expect_equal(logs$baseline, c("LPJ-GUESS started", "LPJ-GUESS finished"))
})

test_that("read_logs warns when selected log files are missing", {
    root <- write_run_catalog(tempfile("lpjguess-runs-"))
    unlink(file.path(root, "site", "baseline", "guess.log"))

    expect_warning(
        expect_error(read_logs(root), "No guess.log files found"),
        "No guess.log found"
    )
})

test_that("read_output returns a combined data frame with user-facing ids", {
    root <- write_run_catalog(tempfile("lpjguess-runs-"))

    lai <- read_output(root, "file_lai")

    expect_s3_class(lai, "data.frame")
    expect_equal(names(lai)[1:2], c("simulation", "instruction_file"))
    expect_equal(lai$instruction_file, c("site", "site"))
    expect_false("base_ins" %in% names(lai))
    expect_false("output_type" %in% names(lai))
    expect_equal(lai$simulation, c("baseline", "baseline"))
    expect_equal(lai$TeBE, c(2.5, 2.6))
})

test_that("read_output keeps output type only when multiple outputs are read", {
    root <- write_run_catalog(tempfile("lpjguess-runs-"))
    site_dir <- file.path(root, "site", "baseline")
    writeLines(c(
        'outputdirectory "out"',
        'file_lai "lai.out"',
        'file_npp "npp.out"'
    ), file.path(site_dir, "generated.ins"))
    writeLines(c("Lon Lat Year TeBE", "151 -33 2000 1"),
               file.path(site_dir, "out", "npp.out"))

    data <- read_output(root, c("file_lai", "file_npp"))

    expect_true("output_type" %in% names(data))
    expect_equal(sort(unique(data$output_type)), c("file_lai", "file_npp"))
})

test_that("read_output warns about requested outputs which do not exist", {
    root <- write_run_catalog(tempfile("lpjguess-runs-"))

    expect_warning(
        data <- read_output(root, c("lai", "agpp")),
        "agpp"
    )
    expect_equal(data$TeBE, c(2.5, 2.6))
})

test_that("read_output returns a data.table when data.table is available", {
    skip_if_not_installed("data.table")
    root <- write_run_catalog(tempfile("lpjguess-runs-"))

    data <- read_output(root, "lai")

    expect_s3_class(data, "data.table")
})

test_that("read_output can include all metadata columns", {
    root <- write_run_catalog(tempfile("lpjguess-runs-"))

    lai <- read_output(root, "file_lai", id_cols = "all")

    expect_true("base_ins" %in% names(lai))
    expect_true("simulation_key" %in% names(lai))
    expect_true("output_path" %in% names(lai))
})

test_that("instruction file names match runner directory naming", {
    root <- write_run_catalog(tempfile("lpjguess-runs-"))
    site_dir <- file.path(root, "My Site", "baseline")
    dir.create(file.path(site_dir, "out"), recursive = TRUE)
    writeLines('simulations = ["My Site/baseline"]',
               file.path(root, "index.toml"))
    writeLines(c(
        'key = "baseline"',
        'name = "baseline"',
        paste0('base_ins = "', file.path(root, "My Site.ins"), '"'),
        'ins_file = "generated.ins"',
        'generated_at_utc = 2024-05-01T13:15:00Z',
        'pfts = []',
        'factors = []'
    ), file.path(site_dir, "manifest.toml"))
    writeLines(c('outputdirectory "out"', 'file_lai "lai.out"'),
               file.path(site_dir, "generated.ins"))
    writeLines(c("Lon Lat Year TeBE", "151 -33 2000 1"),
               file.path(site_dir, "out", "lai.out"))

    lai <- read_output(root, "lai")

    expect_equal(lai$instruction_file, "My_Site")
})

test_that("read_output accepts filenames and conventional output stems", {
    root <- write_run_catalog(tempfile("lpjguess-runs-"))

    expect_equal(read_output(root, "lai.out")$TeBE, c(2.5, 2.6))
    expect_equal(read_output(root, "lai")$TeBE, c(2.5, 2.6))
})

test_that("read_output maps file-prefixed output types to conventional stems", {
    root <- write_run_catalog(tempfile("lpjguess-runs-"))
    site_dir <- file.path(root, "site", "baseline")
    writeLines(c(
        'outputdirectory "out"',
        'file_dave_lai "dave_lai.out"'
    ), file.path(site_dir, "generated.ins"))
    writeLines(c(
        "Lon Lat Year TeBE",
        "151.25 -33.25 2000 3.5"
    ), file.path(site_dir, "out", "dave_lai.out"))

    expect_equal(read_output(root, "dave_lai")$TeBE, 3.5)
})

test_that("read_output rejects ambiguous explicit simulation names", {
    root <- write_run_catalog(tempfile("lpjguess-runs-"))
    other_dir <- file.path(root, "site2", "baseline")
    dir.create(file.path(other_dir, "out"), recursive = TRUE)
    writeLines('simulations = ["site/baseline", "site2/baseline"]',
               file.path(root, "index.toml"))
    writeLines(c(
        'key = "baseline-2"',
        'name = "baseline"',
        paste0('base_ins = "', file.path(root, "site2.ins"), '"'),
        'ins_file = "generated.ins"',
        'generated_at_utc = 2024-05-01T13:15:00Z',
        'pfts = []',
        'factors = []'
    ), file.path(other_dir, "manifest.toml"))
    writeLines(c('outputdirectory "out"', 'file_lai "lai.out"'),
               file.path(other_dir, "generated.ins"))
    writeLines(c("Lon Lat Year TeBE", "151 -33 2000 1"),
               file.path(other_dir, "out", "lai.out"))

    expect_error(read_output(root, "file_lai", simulations = "baseline"),
                 "matches 2 completed jobs")
})
