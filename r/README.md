# lpjguessRunner

[![CI](https://github.com/hie-dave/lpjg-gui/actions/workflows/ci.yml/badge.svg)](https://github.com/hie-dave/lpjg-gui/actions/workflows/ci.yml)

R wrapper for the LPJ-GUESS experiment runner. The package starts the
framework-dependent `lpjg-experiment` host and communicates with it through its
versioned JSON/NDJSON interface.

## Requirements

- R
- .NET 9 runtime, with `dotnet` available on `PATH`, when installing a package
  that already contains the published runner
- A working LPJ-GUESS executable
- `jsonlite` and `processx` (installed automatically as R package dependencies)
- .NET 9 SDK when building or staging the runner from source

GTK and libadwaita are not required for the R wrapper.

## Installation

Tagged R package releases are attached to GitHub releases as source archives
with the published runner already included. Install a release archive directly:

```r
install.packages(
  "https://github.com/hie-dave/lpjg-gui/releases/download/r/v0.1.0/lpjguessRunner_0.1.0.tar.gz",
  repos = NULL,
  type = "source"
)

# Or:
pak::pkg_install("url::https://github.com/hie-dave/lpjg-gui/releases/download/r/v0.1.0/lpjguessRunner_0.1.0.tar.gz")
```

That installation path requires R, the R package dependencies, and the .NET 9
runtime on `PATH`, but does not require the .NET SDK.

## Build from source

Installation from a source checkout builds the runner and therefore requires
the .NET 9 SDK. Clone the repository with its submodules, publish the portable
CLI, stage it into the R package, and install:

```bash
git clone --recurse-submodules git@github.com:hie-dave/lpjg-gui.git
cd lpjg-gui

Rscript -e 'install.packages(c("jsonlite", "processx"), repos="https://cloud.r-project.org")'
make stage-r
R CMD INSTALL r
```

`stage-r` builds a portable framework-dependent runner. The corresponding
.NET 9 runtime must be installed on the target machine.

For development, build the solution and install the R source without staging:

```bash
dotnet build src/LpjGuess.sln
R CMD INSTALL r
```

When run from the repository root, the package can find the debug runner
automatically. Otherwise, select a runner DLL or executable explicitly:

```r
options(lpjguess.runner.path = "/path/to/lpjg-experiment.dll")
```

The `LPJGUESS_RUNNER` environment variable provides the same override. Lookup
order is: the R option, the environment variable, the runner bundled in the
installed package, and finally the repository debug build.

## Concepts and workflow

The package is an idiomatic R client for the same .NET experiment API used by
the CLI and Python wrapper. A typical workflow defines run settings, constructs
one or more simulations, supplies instruction files and optional PFTs, and then
runs the experiment synchronously or asynchronously.

- **Run settings:** `run_settings_local()` covers normal local execution.
  `run_settings()` exposes the complete local/PBS settings, including dry-run,
  walltime, memory, queue, project, email, factorial, and CPU-affinity options.

- **Simulations:** `simulation()` describes a named set of changes applied to
  every base instruction file. Changes are created with
  `top_level_parameter()` or `block_parameter()`.

- **Parameter values:** Numeric, logical, and character scalar values are
  accepted. The wrapper converts values to the invariant strings required by
  LPJ-GUESS; callers do not need to call `as.character()` themselves.

- **Instruction files and PFTs:** Each instruction file is run once for each
  simulation. If `pfts` is non-empty, the generated instruction file disables
  all PFTs and then enables the requested names.

- **Progress and output:** Optional R functions receive structured progress
  and LPJ-GUESS stdout/stderr events. Callbacks are dispatched while R polls
  the runner, so they execute on R's main thread.

- **Outputs:** Files are written below the output directory in the run
  settings. The precise files depend on the LPJ-GUESS instruction file and
  input module.

- **Results:** A completed run returns an `lpjguess_result` containing summary
  counts, any experiment error, and per-job names and durations.

## Example

```r
library(lpjguessRunner)

settings <- run_settings_local(
    guess_path = "/path/to/guess",
    output_directory = "/path/to/output",
    input_module = "nc",
    cpu_count = 4,
    job_name = "example",
    use_cpu_affinity = TRUE
)

simulations <- list(
    simulation(
        "wateruptake_wcont_sla_26",
        top_level_parameter("wateruptake", "wcont"),
        block_parameter("pft", "MRS", "sla", 26)
    ),
    simulation(
        "wateruptake_rootdist_sla_39",
        top_level_parameter("wateruptake", "rootdist"),
        block_parameter("pft", "MRS", "sla", 39)
    )
)

instruction_files <- c("/path/to/file1.ins", "/path/to/file2.ins")

result <- run_simulations(
    settings,
    simulations,
    instruction_files,
    pfts = "MRS",
    progress = function(event) {
        message(sprintf(
            "%.1f%%: %d/%d jobs (%.1f seconds)",
            event$percent, event$completed, event$total,
            event$elapsed_seconds
        ))
    },
    output = function(event) {
        destination <- if (event$stream == "stderr") stderr() else stdout()
        cat(sprintf("[%s] %s\n", event$job, event$text), file = destination)
    },
    existing_output_policy = "clean_managed"
)

print(result)
result$total_jobs
result$successful_jobs
result$failed_jobs
result$error
result$results
```

## Run settings

For local runs, use `run_settings_local()`:

```r
settings <- run_settings_local(
    guess_path,
    output_directory,
    input_module = "nc",
    cpu_count = 1L,
    job_name = "lpjguess",
    use_cpu_affinity = TRUE
)
```

CPU affinity pins each LPJ-GUESS process to a CPU on supported platforms and is
recommended for local parallel runs. It has no effect on macOS.

`run_settings()` also supports PBS execution. Set `run_local = FALSE` and
provide the scheduler fields as appropriate:

```r
settings <- run_settings(
    guess_path = "/cluster/path/to/guess",
    output_directory = "/cluster/path/to/output",
    input_module = "nc",
    cpu_count = 16,
    job_name = "experiment",
    run_local = FALSE,
    dry_run = FALSE,
    walltime = "12:00:00",
    memory = 32,
    queue = "normal",
    project = "project-code",
    email_notifications = TRUE,
    email_address = "user@example.org"
)
```

`walltime` uses the .NET constant `TimeSpan` format, such as `12:00:00` or
`1.12:00:00` for one day and twelve hours. `memory` is measured in GiB.

## Factors and simulations

A top-level factor changes a parameter outside a named instruction-file block:

```r
top_level_parameter("npatch", 10)
top_level_parameter("wateruptake", "rootdist")
```

A block factor identifies the block type, block name, parameter, and value:

```r
block_parameter("pft", "MRS", "sla", 26)
```

Factors may be passed individually or as a list:

```r
simulation("example",
           top_level_parameter("npatch", 10),
           block_parameter("pft", "MRS", "sla", 26))

simulation("example", list(
    top_level_parameter("npatch", 10),
    block_parameter("pft", "MRS", "sla", 26)
))
```

Simulation names are used to identify jobs and managed output directories, so
they should be unique within an experiment.

### Programmatic construction

R's `expand.grid()` is convenient for generating a parameter grid:

```r
grid <- expand.grid(
    wateruptake = c("wcont", "rootdist"),
    npatch = seq(1, 41, by = 10),
    stringsAsFactors = FALSE
)

simulations <- lapply(seq_len(nrow(grid)), function(i) {
    values <- grid[i, ]
    simulation(
        sprintf("wu_%s_npatch_%d", values$wateruptake, values$npatch),
        top_level_parameter("wateruptake", values$wateruptake),
        top_level_parameter("npatch", values$npatch)
    )
})
```

The R code constructs explicit simulations; `full_factorial` is primarily
relevant to configuration paths that generate simulations inside the .NET
runner.

## Existing output files

LPJ-GUESS outputs are stored in per-simulation directories below the configured
output directory. When rerunning an experiment, files from an earlier
configuration may otherwise remain and appear to belong to the new run.

`existing_output_policy` controls how managed outputs are handled. Its default
is `"clean_managed"`.

- `"preserve"`: leave existing outputs untouched.
- `"clean_managed"`: remove old files for simulations being regenerated.
- `"prune_stale"`: remove managed simulations from previous runs that are not
  part of the current experiment.
- `"fail"`: abort if existing output directories are found.

Policies can be combined by passing a character vector:

```r
result <- run_simulations(
    settings, simulations, instruction_files,
    existing_output_policy = c("clean_managed", "prune_stale")
)
```

Managed outputs are outputs tracked by the runner's result catalog.

## Progress and model output

`run_simulations()` accepts two optional callbacks:

- `progress(event)` receives `percent`, `elapsed_seconds`, `completed`, and
  `total`.
- `output(event)` receives `job`, `text`, and `stream`; `stream` is either
  `"stdout"` or `"stderr"`.

Callbacks run on R's main thread. They should still be reasonably fast because
the wrapper drains runner events between polling intervals. Model output can be
frequent, so consider buffering or filtering it.

If callbacks are omitted, progress and model output are retained in the run
handle's event history during asynchronous operation but are not printed.

## Asynchronous runs and cancellation

Use `run_simulations_async()` when R must remain responsive:

```r
run <- run_simulations_async(
    settings, simulations, instruction_files, pfts = "MRS"
)

while (poll_run(
    run,
    timeout = 100,
    progress = function(event) message(event$percent, "%")
)) {
    # Perform other R work between polls.
}

result <- wait_run(run)
```

`poll_run()` returns invisibly whether the process is still running. Its
`timeout` and the `poll_interval` accepted by `wait_run()` are milliseconds.
Callbacks are invoked only while `poll_run()` or `wait_run()` drains events.

Cancel a run explicitly with:

```r
cancel_run(run)
```

Interrupting synchronous `run_simulations()` also signals cancellation. The
runner propagates cancellation to active jobs and cleans up its child process
tree. If graceful cancellation is not possible, the wrapper terminates the
bridge when its handle is discarded or the R session exits.

## Results and errors

An `lpjguess_result` contains:

- `total_jobs`
- `successful_jobs`
- `failed_jobs`
- `error`, or `NULL` when no experiment error was reported
- `results`, a list of per-job `name` and `duration_seconds` records

Protocol, configuration, and runner-startup failures are raised as R errors.
Individual model failures are reflected in the completed experiment result and
its failed-job count. Files remain under the output directory according to the
selected existing-output policy.

## Runner selection and troubleshooting

Confirm that the required runtime is available with:

```bash
dotnet --list-runtimes
```

If the package cannot find the runner, set either:

```r
options(lpjguess.runner.path = "/absolute/path/to/lpjg-experiment.dll")
```

or:

```bash
export LPJGUESS_RUNNER=/absolute/path/to/lpjg-experiment.dll
```

The path may identify a framework-dependent `.dll` or a directly executable
runner host. Paths to LPJ-GUESS, instruction files, and output directories are
passed to the separate runner process and therefore must be accessible from
that process's environment.
