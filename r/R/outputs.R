.output_directory <- function(x) {
    if (inherits(x, "lpjguess_result")) {
        if (!is.null(x$output_directory) && nzchar(x$output_directory)) {
            return(normalizePath(x$output_directory, mustWork = FALSE))
        }
        stop("Run result does not contain an output directory", call. = FALSE)
    }

    if (is.list(x) && !is.null(x$output_directory)) {
        return(normalizePath(x$output_directory, mustWork = FALSE))
    }

    if (is.character(x) && length(x) == 1L && !is.na(x)) {
        return(normalizePath(x, mustWork = FALSE))
    }

    stop("Expected an output directory, run settings, or lpjguess_result",
         call. = FALSE)
}

.toml_value <- function(lines, key, required = TRUE) {
    pattern <- paste0("^\\s*", key, "\\s*=\\s*(.*)$")
    start <- grep(pattern, lines)
    if (!length(start)) {
        if (required) stop("Missing TOML key: ", key, call. = FALSE)
        return(NULL)
    }

    value <- sub(pattern, "\\1", lines[[start[[1L]]]])
    if (startsWith(trimws(value), "[") && !grepl("\\]", value, fixed = FALSE)) {
        i <- start[[1L]] + 1L
        while (i <= length(lines)) {
            value <- paste(value, lines[[i]])
            if (grepl("\\]", lines[[i]], fixed = FALSE)) break
            i <- i + 1L
        }
    }
    trimws(value)
}

.toml_string <- function(lines, key, required = TRUE) {
    value <- .toml_value(lines, key, required)
    if (is.null(value)) return(NA_character_)
    if (startsWith(value, "\"")) {
        return(jsonlite::fromJSON(value))
    }
    value
}

.toml_string_array <- function(lines, key, required = TRUE) {
    value <- .toml_value(lines, key, required)
    if (is.null(value)) return(character())
    value <- gsub(",\\s*\\]", "]", value)
    as.character(jsonlite::fromJSON(value))
}

.read_index <- function(output_directory) {
    path <- file.path(output_directory, "index.toml")
    if (!file.exists(path)) {
        stop("Cannot find simulation index: ", path, call. = FALSE)
    }
    .toml_string_array(readLines(path, warn = FALSE), "simulations")
}

.read_manifest <- function(simulation_directory) {
    path <- file.path(simulation_directory, "manifest.toml")
    if (!file.exists(path)) {
        stop("Cannot find simulation manifest: ", path, call. = FALSE)
    }

    lines <- readLines(path, warn = FALSE)
    generated <- .toml_string(lines, "generated_at_utc", required = FALSE)
    generated <- if (is.na(generated)) {
        as.POSIXct(NA)
    } else {
        as.POSIXct(generated, tz = "UTC", format = "%Y-%m-%dT%H:%M:%OS")
    }

    list(
        key = .toml_string(lines, "key"),
        simulation = .toml_string(lines, "name"),
        base_ins = .toml_string(lines, "base_ins"),
        ins_file = .toml_string(lines, "ins_file"),
        pfts = list(.toml_string_array(lines, "pfts", required = FALSE)),
        factors = list(.read_toml_factors(lines)),
        generated_at_utc = generated
    )
}

.read_toml_factors <- function(lines) {
    starts <- grep("^\\s*\\[\\[factors\\]\\]\\s*$", lines)
    if (!length(starts)) return(list())

    lapply(seq_along(starts), function(i) {
        start <- starts[[i]] + 1L
        end <- if (i == length(starts)) length(lines) else starts[[i + 1L]] - 1L
        block <- lines[start:end]

        name <- .toml_string(block, "name", required = FALSE)
        value <- .toml_string(block, "value", required = FALSE)
        block_type <- .toml_string(block, "block_type", required = FALSE)
        block_name <- .toml_string(block, "block_name", required = FALSE)

        if (is.na(name) || is.na(value)) {
            return(list(type = "dummy", name = "", value = ""))
        }
        if (!is.na(block_type) && !is.na(block_name)) {
            return(block_parameter(block_type, block_name, name, value))
        }
        top_level_parameter(name, value)
    })
}

.data_frame <- function(rows) {
    if (!length(rows)) return(data.frame())
    out <- do.call(rbind.data.frame, c(rows, stringsAsFactors = FALSE))
    rownames(out) <- NULL
    out
}

.bind_rows <- function(rows) {
    if (!length(rows)) return(data.frame())
    if (requireNamespace("data.table", quietly = TRUE)) {
        return(data.table::rbindlist(rows, fill = TRUE, use.names = TRUE,
                                     idcol = FALSE))
    }

    columns <- unique(unlist(lapply(rows, names), use.names = FALSE))
    rows <- lapply(rows, function(row) {
        missing <- setdiff(columns, names(row))
        for (column in missing) row[[column]] <- NA
        row[columns]
    })
    out <- do.call(rbind.data.frame, c(rows, stringsAsFactors = FALSE))
    rownames(out) <- NULL
    out
}

#' List completed LPJ-GUESS simulations
#'
#' Discovers simulations managed by the runner by reading `index.toml` and the
#' per-simulation `manifest.toml` files below an output directory.
#'
#' @param x Output directory, run settings object, or `lpjguess_result`.
#'
#' @return A data frame with one row per completed simulation job.
#'
#' @export
list_simulations <- function(x) {
    output_directory <- .output_directory(x)
    simulations <- .read_index(output_directory)
    rows <- lapply(simulations, function(path) {
        directory <- normalizePath(file.path(output_directory, path),
                                   mustWork = FALSE)
        manifest <- .read_manifest(directory)
        data.frame(
            simulation = manifest$simulation,
            path = directory,
            base_ins = manifest$base_ins,
            ins_file = manifest$ins_file,
            generated_at_utc = manifest$generated_at_utc,
            key = manifest$key,
            pfts = I(manifest$pfts),
            factors = I(manifest$factors),
            stringsAsFactors = FALSE
        )
    })
    out <- .data_frame(rows)
    class(out) <- c("lpjguess_simulations", class(out))
    out
}

.as_simulations <- function(x) {
    if (inherits(x, "lpjguess_simulations")) return(x)
    if (is.data.frame(x) && all(c("simulation", "path", "ins_file") %in% names(x))) {
        return(x)
    }
    list_simulations(x)
}

.filter_simulations <- function(runs, simulations = NULL, base_ins = NULL) {
    if (!is.null(base_ins)) {
        runs <- runs[runs$base_ins %in% base_ins, , drop = FALSE]
    }

    if (!is.null(simulations)) {
        simulations <- as.character(simulations)
        for (simulation in simulations) {
            matches <- runs[runs$simulation == simulation, , drop = FALSE]
            if (nrow(matches) > 1L && is.null(base_ins)) {
                stop("Simulation '", simulation, "' matches ", nrow(matches),
                     " completed jobs. Filter the simulation table first or ",
                     "provide base_ins.", call. = FALSE)
            }
        }
        runs <- runs[runs$simulation %in% simulations, , drop = FALSE]
    }

    runs
}

.ins_path <- function(row) {
    normalizePath(file.path(row$path, row$ins_file), mustWork = FALSE)
}

.ins_parameter <- function(ins_file, name) {
    if (!file.exists(ins_file)) return(NA_character_)
    lines <- readLines(ins_file, warn = FALSE)
    pattern <- paste0("^\\s*", name, "\\s+\"([^\"]+)\"")
    match <- grep(pattern, lines, value = TRUE)
    if (!length(match)) return(NA_character_)
    sub(pattern, "\\1", match[[1L]])
}

.output_directory_for_ins <- function(ins_file) {
    output <- .ins_parameter(ins_file, "outputdirectory")
    if (is.na(output)) return(dirname(ins_file))
    if (grepl("^(/|[A-Za-z]:[\\\\/])", output)) return(output)
    normalizePath(file.path(dirname(ins_file), output), mustWork = FALSE)
}

.output_mappings <- function(ins_file) {
    if (!file.exists(ins_file)) return(setNames(character(), character()))
    lines <- readLines(ins_file, warn = FALSE)
    match <- grep("^\\s*file_[A-Za-z0-9_]+\\s+\"[^\"]+\"", lines, value = TRUE)
    if (!length(match)) return(setNames(character(), character()))
    types <- sub("^\\s*(file_[A-Za-z0-9_]+)\\s+\"[^\"]+\".*$", "\\1", match)
    files <- sub("^\\s*file_[A-Za-z0-9_]+\\s+\"([^\"]+)\".*$", "\\1", match)
    stats::setNames(files, types)
}

#' List LPJ-GUESS output files
#'
#' @param x Output directory, run settings object, `lpjguess_result`, or a
#'   simulation table returned by [list_simulations()].
#' @param simulations Optional simulation name filter.
#' @param base_ins Optional base instruction-file filter, useful when the same
#'   simulation name was run for multiple instruction files.
#'
#' @return A data frame with one row per discovered `*.out` file.
#'
#' @export
list_outputs <- function(x, simulations = NULL, base_ins = NULL) {
    runs <- .filter_simulations(.as_simulations(x), simulations, base_ins)
    rows <- list()

    for (i in seq_len(nrow(runs))) {
        run <- runs[i, , drop = FALSE]
        ins <- .ins_path(run)
        output_directory <- .output_directory_for_ins(ins)
        files <- list.files(output_directory, pattern = "\\.out$",
                            full.names = TRUE)
        mappings <- .output_mappings(ins)
        reverse <- stats::setNames(names(mappings), basename(mappings))

        for (file in files) {
            name <- basename(file)
            output_type <- unname(reverse[name])
            rows[[length(rows) + 1L]] <- data.frame(
                simulation = run$simulation,
                base_ins = run$base_ins,
                ins_file = ins,
                output_type = if (is.na(output_type)) NA_character_ else output_type,
                file = name,
                path = normalizePath(file, mustWork = FALSE),
                simulation_key = run$key,
                stringsAsFactors = FALSE
            )
        }
    }

    out <- .data_frame(rows)
    class(out) <- c("lpjguess_outputs", class(out))
    out
}

.read_table <- function(path) {
    if (requireNamespace("data.table", quietly = TRUE)) {
        return(data.table::fread(path, data.table = TRUE))
    }

    utils::read.table(path, header = TRUE, check.names = FALSE,
                      stringsAsFactors = FALSE)
}

.matches_output <- function(outputs, output) {
    output_stem <- tools::file_path_sans_ext(basename(output))
    output_file <- if (grepl("\\.out$", output, ignore.case = TRUE)) {
        output
    } else {
        paste0(output, ".out")
    }
    output_type_files <- ifelse(grepl("^file_", outputs$output_type),
                                paste0(sub("^file_", "", outputs$output_type),
                                       ".out"),
                                NA_character_)
    output_type_stems <- tools::file_path_sans_ext(output_type_files)
    file_stems <- tools::file_path_sans_ext(outputs$file)

    outputs$output_type == output |
        outputs$file == output |
        outputs$file == output_file |
        file_stems == output |
        file_stems == output_stem |
        output_type_files == output |
        output_type_files == output_file |
        output_type_stems == output |
        output_type_stems == output_stem |
        outputs$path == output
}

.select_outputs <- function(outputs, output) {
    output <- as.character(output)
    matched <- rep(FALSE, nrow(outputs))
    missing <- character()

    for (item in output) {
        item_matches <- .matches_output(outputs, item)
        item_matches[is.na(item_matches)] <- FALSE
        if (any(item_matches)) {
            matched <- matched | item_matches
        } else {
            missing <- c(missing, item)
        }
    }

    selected <- outputs[matched, , drop = FALSE]
    if (!nrow(selected)) {
        stop("No matching output files found for: ",
             paste(output, collapse = ", "), call. = FALSE)
    }
    if (length(missing)) {
        warning("No matching output files found for: ",
                paste(missing, collapse = ", "), call. = FALSE)
    }
    selected
}

.id_data <- function(output, n, id_cols, include_output_type) {
    if (isFALSE(id_cols)) return(NULL)
    if (identical(id_cols, "all")) {
        return(data.frame(
            simulation = rep(output$simulation, n),
            base_ins = rep(output$base_ins, n),
            ins_file = rep(output$ins_file, n),
            output_type = rep(output$output_type, n),
            output_file = rep(output$file, n),
            output_path = rep(output$path, n),
            simulation_key = rep(output$simulation_key, n),
            stringsAsFactors = FALSE
        ))
    }
    ids <- data.frame(
        simulation = rep(output$simulation, n),
        stringsAsFactors = FALSE
    )
    if (include_output_type) {
        ids$output_type <- rep(output$output_type, n)
    }
    ids
}

#' Read LPJ-GUESS output files as data frames
#'
#' Reads completed model output files discovered from the runner catalog. The
#' default return value is one combined wide data frame, with simulation
#' metadata columns prepended.
#'
#' @param x Output directory, run settings object, `lpjguess_result`,
#'   simulation table, or output table.
#' @param output Output type such as `"file_lai"`, concrete filename such as
#'   `"lai.out"`, filename stem such as `"lai"`, or full output path. For
#'   conventional output names, a stem such as `"dave_lai"` also matches
#'   output type `"file_dave_lai"`.
#' @param simulations Optional simulation name filter.
#' @param base_ins Optional base instruction-file filter.
#' @param combine If `TRUE`, return a single combined data frame. If `FALSE`,
#'   return a named list of data frames.
#' @param id_cols If `TRUE`, prepend common identifier columns. If `"all"`,
#'   prepend all available metadata columns. If `FALSE`, return raw file data.
#'
#' @return A data frame, or a list of data frames when `combine = FALSE`.
#'   If `data.table` is installed, combined results and per-file reads are
#'   returned as `data.table` objects.
#'
#' @export
read_output <- function(x, output, simulations = NULL, base_ins = NULL,
                        combine = TRUE, id_cols = TRUE) {
    if (inherits(x, "lpjguess_outputs")) {
        outputs <- x
        if (!is.null(base_ins)) {
            outputs <- outputs[outputs$base_ins %in% base_ins, , drop = FALSE]
        }
        if (!is.null(simulations)) {
            outputs <- outputs[outputs$simulation %in% simulations, , drop = FALSE]
        }
    } else {
        outputs <- list_outputs(x, simulations = simulations, base_ins = base_ins)
    }
    outputs <- .select_outputs(outputs, as.character(output))
    output_ids <- ifelse(is.na(outputs$output_type), outputs$file,
                         outputs$output_type)
    include_output_type <- length(unique(output_ids)) > 1L

    data <- lapply(seq_len(nrow(outputs)), function(i) {
        item <- outputs[i, , drop = FALSE]
        values <- .read_table(item$path)
        ids <- .id_data(item, nrow(values), id_cols, include_output_type)
        if (is.null(ids)) values else cbind(ids, values)
    })

    names(data) <- paste(outputs$simulation, outputs$file, sep = "/")
    if (!combine) return(data)

    .bind_rows(data)
}
