# lpjguessRunner

[![CI](https://github.com/hie-dave/lpjg-gui/actions/workflows/ci.yml/badge.svg)](https://github.com/hie-dave/lpjg-gui/actions/workflows/ci.yml)

`lpjguessRunner` runs LPJ-GUESS simulation experiments from R. You provide one
or more instruction files, define the parameter changes for each simulation,
and read completed outputs back as ordinary R tables.

GTK and libadwaita are not required for the R wrapper.

## Requirements

- R
- A working LPJ-GUESS executable
- .NET 9 runtime for published package builds
- .NET 9 SDK when building the runner from source

## Installation

Tagged R package releases are attached to GitHub releases as source archives
with the published runner included. Open the
[latest release page](https://github.com/hie-dave/lpjg-gui/releases/latest) and
follow the R installation instructions there.

For development from a source checkout:

```bash
git clone --recurse-submodules git@github.com:hie-dave/lpjg-gui.git
cd lpjg-gui

Rscript -e 'install.packages(c("jsonlite", "processx"), repos="https://cloud.r-project.org")'
make stage-r
R CMD INSTALL r
```

When run from the repository root, the package can find the debug runner
automatically. Otherwise, select a runner DLL or executable explicitly:

```r
options(lpjguess.runner.path = "/path/to/lpjg-experiment.dll")
```

The `LPJGUESS_RUNNER` environment variable provides the same override.

## Quick Start

```r
library(lpjguessRunner)

settings <- run_settings_local(
    guess_path = "/path/to/lpjguess",
    output_directory = "/path/to/output",
    input_module = "nc",
    cpu_count = 4
)

simulations <- list(
    simulation("baseline"),
    simulation(
        "high_sla",
        block_parameter("pft", "MRS", "sla", 39)
    )
)

result <- run_simulations(
    settings = settings,
    simulations = simulations,
    instruction_files = "/path/to/global.ins",
    pfts = "MRS"
)

lai <- read_output(result, "lai")
plot(lai$Year, lai$MRS)

logs <- read_logs(result)
```

By default, `run_simulations()` prints a single progress line while simulations
are running. Model stdout and stderr are kept quiet unless a run fails, in
which case the captured output is printed before the R error is raised.

## Documentation

- `help(package = "lpjguessRunner")`

## User Guides

- `vignette("running-simulations", package = "lpjguessRunner")`
- `vignette("reading-outputs", package = "lpjguessRunner")`
- `vignette("factorial-experiments", package = "lpjguessRunner")`
