# LPJ-Guess GUI

[![CI](https://github.com/hie-dave/lpjg-gui/actions/workflows/ci.yml/badge.svg)](https://github.com/hie-dave/lpjg-gui/actions/workflows/ci.yml)

A simple cross-platform LPJ-Guess graphical frontend and simulation runner.

## Requirements

- Gtk4
- libadwaita
- .NET 9 SDK
- Working local build of LPJ-Guess (technically optional, but required in order
  to do anything useful)

Note that Gtk4 and libadwaita are not required for using the runner in
standalone mode - they are only required for the GUI. Platform-specific
installation instructions for these two packages are provided below.

### Windows

- Install [MSYS2](https://www.msys2.org/)
- Run `pacman -S mingw-w64-clang-x86_64-libadwaita`

### MacOS

Install dependencies with homebrew:

```bash
brew install dotnet-sdk libadwaita adwaita-icon-theme
```

### Linux

Install the following packages:

- Gtk4
- libadwaita
- .NET 9 SDK

## Build

Prerequisites:

- .NET 9 SDK
- Python 3.11 (if building the Python wheel)
- R (if building the R package)

The solution may be built in an IDE of choice, or using the provided Makefile
or .NET CLI:

```bash
# Clone the repository with submodules
git clone --recurse-submodules git@github.com:hie-dave/lpjg-gui.git

make
# dotnet build src/LpjGuess.sln
```

Unit tests may be run using the provided Makefile or .NET CLI:

```bash
make check
# dotnet test src/LpjGuess.sln
```

## Run

### GUI

The GUI may be run via an IDE of choice, or via the provided Makefile or .NET
CLI:

```bash
make run
# dotnet run --project src/LpjGuess.Frontend
```

The GUI requires no CLI arguments, but some optional arguments may be passed to
control the logging. Run with `--help` to view the available CLI options.

### Simulation Runner

The simulation runner provides a way to run a set of LPJ-Guess instructions
multiple times by varying parameters in the instruction files. Generated
simulations can be run locally, or submitted to PBS for execution.

The simulation runner may be run via the command-line interface or through its
Python and R wrappers. For ordinary interactive CLI use, options are read from
a TOML file:

```bash
dotnet run --project src/LpjGuess.Runner.CLI -- <myconfig.toml>
```

An [example .toml file](example.toml) containing all available options is
present in the repository.

### Python Bindings

The python bindings to the runner tool are currently the best way to run large
numbers of simulations with non-trivial parameter combinations. The bindings
may be built locally, or installed from PyPI:

```bash
pip install lpjguess-runner
```

For more detailed information, consult the
[PyPI page](https://pypi.org/project/lpjguess-runner/).

### R Bindings

The R wrapper exposes the same experiment concepts as the Python wrapper,
including named simulations, top-level and block parameter changes, output
cleanup policies, progress and model-output callbacks, complete results,
asynchronous polling, and cancellation. It communicates with the runner over a
versioned machine interface. Building it from this source checkout requires the
.NET 9 SDK; an R package with a prebuilt runner would require only the matching
.NET 9 runtime.

Build and install it from a source checkout with:

```bash
make stage-r
make docs-r
R CMD INSTALL r
```

Tagged R package releases are attached to GitHub releases. To install the
latest published R package, open the
[latest release page](https://github.com/hie-dave/lpjg-gui/releases/latest) and
follow the R installation instructions there.

For installation details, complete examples, local and PBS settings, parameter
grid construction, output policies, callbacks, and asynchronous use, see the
[R wrapper documentation](r/README.md).

## Screenshots

![Instruction Files](img/ins-files.png)
![Tabular Output](img/tabular-output.png)
![Graphs](img/graphs.png)
![Graphs Light](img/graphs-blinding.png)
![Runners](img/runners.png)
