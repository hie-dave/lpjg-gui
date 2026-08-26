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

run_settings_local <- function(guess_path, output_directory,
                               input_module = "nc", cpu_count = 1L,
                               job_name = "lpjguess",
                               use_cpu_affinity = TRUE) {
    run_settings(guess_path, output_directory, input_module, cpu_count,
                 job_name, use_cpu_affinity = use_cpu_affinity)
}

top_level_parameter <- function(name, value) {
    list(type = "top_level", name = name,
         value = .scalar_string(value, "value"))
}

block_parameter <- function(block_type, block_name, name, value) {
    list(type = "block", block_type = block_type, block_name = block_name,
         name = name, value = .scalar_string(value, "value"))
}

simulation <- function(name, ...) {
    factors <- list(...)
    if (length(factors) == 1L && is.list(factors[[1L]]) &&
        is.null(factors[[1L]]$type)) factors <- factors[[1L]]
    list(name = name, factors = factors)
}
