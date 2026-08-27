.scalar_string <- function(value, argument) {
    if (length(value) != 1L || is.na(value)) {
        stop(argument, " must be a single non-missing value", call. = FALSE)
    }
    if (is.logical(value)) return(tolower(as.character(value)))
    if (is.numeric(value)) {
        return(format(value, trim = TRUE, scientific = FALSE,
                      decimal.mark = "."))
    }
    as.character(value)
}

#' Create LPJ-GUESS runner settings
#'
#' `run_settings()` constructs the execution settings passed to the simulation
#' runner. Use `run_settings_local()` for the common case of running simulations
#' directly on the current machine.
#'
#' @param guess_path Path to the LPJ-GUESS executable.
#' @param output_directory Directory where generated simulations and LPJ-GUESS
#'   output should be written.
#' @param input_module LPJ-GUESS input module name, for example `"nc"`.
#' @param cpu_count Number of worker processes or jobs to use.
#' @param job_name Name used for generated jobs and scheduler submissions.
#' @param run_local Logical flag. If `TRUE`, run jobs on the local machine. If
#'   `FALSE`, submit jobs through the configured PBS settings.
#' @param dry_run Logical flag. If `TRUE`, prepare the experiment without
#'   executing LPJ-GUESS jobs.
#' @param walltime PBS walltime request, formatted as `HH:MM:SS`.
#' @param memory PBS memory request in gigabytes.
#' @param queue PBS queue name.
#' @param project PBS project or account name.
#' @param email_notifications Logical flag controlling PBS email notifications.
#' @param email_address Email address used for scheduler notifications.
#' @param full_factorial Logical flag. If `TRUE`, generate the full factorial
#'   combination of supplied parameter changes. If `FALSE`, pair parameter
#'   changes by position where supported by the runner.
#' @param use_cpu_affinity Logical flag controlling whether local worker
#'   processes are pinned to CPU cores.
#'
#' @return A named list suitable for the `settings` argument of
#'   [run_simulations()] or [run_simulations_async()].
#'
#' @examples
#' settings <- run_settings_local(
#'   guess_path = "/opt/lpj-guess/lpjguess",
#'   output_directory = tempfile("lpjguess-output-"),
#'   cpu_count = 2
#' )
#'
#' pbs_settings <- run_settings(
#'   guess_path = "/apps/lpj-guess/lpjguess",
#'   output_directory = "/scratch/me/lpjguess-runs",
#'   run_local = FALSE,
#'   queue = "normal",
#'   project = "my-project",
#'   walltime = "04:00:00",
#'   memory = 8
#' )
#'
#' @export
run_settings <- function(guess_path, output_directory, input_module = "nc",
                         cpu_count = 1L, job_name = "lpjguess",
                         run_local = TRUE, dry_run = FALSE,
                         walltime = "01:00:00", memory = 1L,
                         queue = "local", project = "local",
                         email_notifications = FALSE, email_address = "",
                         full_factorial = TRUE, use_cpu_affinity = TRUE) {
    list(
        dry_run = isTRUE(dry_run), run_local = isTRUE(run_local),
        output_directory = normalizePath(output_directory, mustWork = FALSE),
        guess_path = normalizePath(guess_path, mustWork = FALSE),
        input_module = input_module, cpu_count = as.integer(cpu_count),
        walltime = walltime, memory = as.integer(memory), queue = queue,
        project = project, email_notifications = isTRUE(email_notifications),
        email_address = email_address, job_name = job_name,
        full_factorial = isTRUE(full_factorial),
        use_cpu_affinity = isTRUE(use_cpu_affinity)
    )
}

#' @rdname run_settings
#' @export
run_settings_local <- function(guess_path, output_directory,
                               input_module = "nc", cpu_count = 1L,
                               job_name = "lpjguess",
                               use_cpu_affinity = TRUE) {
    run_settings(guess_path, output_directory, input_module, cpu_count,
                 job_name, use_cpu_affinity = use_cpu_affinity)
}

#' Describe a top-level LPJ-GUESS parameter change
#'
#' Parameter changes are used inside [simulation()] definitions. A top-level
#' parameter targets a setting in the instruction file outside named blocks -
#' e.g. `npatch`, `nyear_spinup`, etc.
#'
#' @param name Parameter name in the LPJ-GUESS instruction file.
#' @param value Replacement value.
#'
#' @return A named list describing the parameter change.
#'
#' @examples
#' top_level_parameter("ifcalcsla", TRUE)
#' top_level_parameter("nyear_spinup", 500)
#'
#' @export
top_level_parameter <- function(name, value) {
    list(type = "top_level", name = name,
         value = .scalar_string(value, "value"))
}

#' Describe a block-scoped LPJ-GUESS parameter change
#'
#' Block parameters target settings inside a named LPJ-GUESS instruction-file
#' block, such as a PFT or or group block.
#'
#' @param block_type Type of block to edit, for example `"pft"`.
#' @param block_name Name of the specific block to edit, e.g. `"TeBE"`.
#' @param name Parameter name inside the block, e.g. `"sla"`.
#' @param value Replacement value.
#'
#' @return A named list describing the parameter change.
#'
#' @examples
#' block_parameter("pft", "TeBE", "sla", 26)
#' block_parameter("pft", "TeBE", "include", TRUE)
#'
#' @export
block_parameter <- function(block_type, block_name, name, value) {
    list(type = "block", block_type = block_type, block_name = block_name,
         name = name, value = .scalar_string(value, "value"))
}

#' Define a named LPJ-GUESS simulation
#'
#' A simulation is a named collection of parameter changes applied to each input
#' instruction file. Pass one or more values created by
#' [top_level_parameter()] or [block_parameter()], either as separate arguments
#' or as a single list.
#'
#' To run the returned simulation, pass it to [run_simulations()] or
#' [run_simulations_async()].
#'
#' @param name Simulation name. The runner uses this in generated output paths
#'   and result metadata.
#' @param ... Parameter changes, or a single list of parameter changes.
#'
#' @return A named list suitable for the `simulations` argument of
#'   [run_simulations()] or [run_simulations_async()].
#'
#' @examples
#' sim1 <- simulation(
#'   "high-sla",
#'   block_parameter("pft", "TeBE", "sla", 39),
#'   top_level_parameter("ifcalcsla", FALSE)
#' )
#'
#' changes <- list(
#'   block_parameter("pft", "TeBE", "sla", 26),
#'   block_parameter("pft", "TeBE", "nindiv_max", 1200)
#' )
#' sim2 <- simulation("baseline", changes)
#'
#' @export
simulation <- function(name, ...) {
    factors <- list(...)
    if (length(factors) == 1L && is.list(factors[[1L]]) &&
        is.null(factors[[1L]]$type)) factors <- factors[[1L]]
    list(name = name, factors = factors)
}
