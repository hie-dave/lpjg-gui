# lpjguess-runner

[![CI](https://github.com/hie-dave/lpjg-gui/actions/workflows/ci.yml/badge.svg)](https://github.com/hie-dave/lpjg-gui/actions/workflows/ci.yml)

Python wrapper for LPJ-Guess experiment runner (via pythonnet + .NET 9)

## Build

Prerequisites:
- .NET 9 SDK
- Python 3.11 (if building wheel)

```bash
# Clone the repository with submodules
git clone --recurse-submodules git@github.com:hie-dave/lpjg-gui.git
make
```

## Install

- Prerequisites: .NET 9 runtime

### From PyPI

```bash
pip install lpjguess-runner
```

### From Source

```bash
make install                 # install globally
make install-venv VENV=.venv # install into a local virtual environment
make install-user            # install for the current user
```

## Example Usage

```python
from lpjguess_runner import *

run_settings = RunSettings.Local(
    "/path/to/guess/executable",
    "/path/to/output/directory",
    "nc",   # input module
    4,      # cpu count
    "job_name")

simulations = [
    simulation("nindiv_max_0_sla_26", [           # Run all .ins files with:
        TopLevelParameter("nindiv_max", "0"),     # - nindiv_max = 0
        BlockParameter("pft", "MRS", "sla", "26") # - SLA of "MRS" pft set to 26
    ]),
    simulation("nindiv_max_1_sla_39", [           # Run all .ins files with:
        TopLevelParameter("nindiv_max", "1"),     # - nindiv_max = 1
        BlockParameter("pft", "MRS", "sla", "39") # - SLA of "MRS" pft set to 39
    ]),
]

ins = ["/path/to/file1.ins", "/path/to/file2.ins"]
pfts = ["MRS"]

result = run_simulations(run_settings,
                            simulations,
                            ins,
                            pfts,
                            ConsoleProgressReporter(), # Write progress messages to stdout
                            ConsoleOutputHelper())     # Propagate subprocess output to stdout

print(f"Total jobs: {result.TotalJobs}")
print(f"Successful jobs: {result.SuccessfulJobs}")
print(f"Failed jobs: {result.FailedJobs}")
print(f"Error: {result.Error}")
```
