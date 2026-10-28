# lpjguessRunner

[![CI](https://github.com/hie-dave/lpjg-gui/actions/workflows/ci.yml/badge.svg)](https://github.com/hie-dave/lpjg-gui/actions/workflows/ci.yml)

`lpjguessRunner` runs LPJ-GUESS simulation experiments from R. You provide one
or more instruction files, define the parameter changes for each simulation,
and read completed outputs back as ordinary R tables.

GTK and libadwaita are not required for the R wrapper.

## Requirements

- R
- A working LPJ-GUESS executable
- Linux x64, Windows x64, or macOS arm64 for release packages
- .NET 9 SDK when building the runner from source

## Installation

Open the [latest release page](https://github.com/hie-dave/lpjg-gui/releases/latest)
and copy its installation block into R or RStudio. The block automatically
selects the archive for your operating system and architecture, installs the R
dependencies, and installs `lpjguessRunner` directly from GitHub into your usual
R library. No manual download or .NET installation is needed: release packages
include a self-contained runner.

In subsequent R sessions, load the package with `library(lpjguessRunner)`.

For development from a source checkout:

```bash
git clone --recurse-submodules git@github.com:hie-dave/lpjg-gui.git
cd lpjg-gui

Rscript -e 'install.packages(c("jsonlite", "processx"), repos="https://cloud.r-project.org")'
make stage-r RID=linux-x64  # choose the RID for your target machine
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
with(lai, plot(Year, Total))

logs <- read_logs(result)
```

## Documentation

- `help(package = "lpjguessRunner")`

## User Guides

- `vignette("running-simulations", package = "lpjguessRunner")`
- `vignette("reading-outputs", package = "lpjguessRunner")`
- `vignette("factorial-experiments", package = "lpjguessRunner")`
