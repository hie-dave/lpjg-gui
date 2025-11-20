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
make wheel
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
make install-venv VENV=.venv # install into a local virtual environment (default: .venv)
make install-user            # install for the current user
```

## Concepts and Workflow

This package is a thin Python wrapper around a .NET job runner for LPJ-Guess.
A typical workflow configures a run, defines which simulations to execute, and
optionally customises how progress and model output are handled.

- **Run settings**: A `RunSettings` object specifies the LPJ-Guess executable
  path, output directory, input module, CPU count, and job name.

- **Simulations**: A list of `Simulation` objects describes variations to apply
  to base instruction files (for example changing top-level parameters or PFT
  block parameters). The helper `simulation(name, factors)` creates these
  objects.

  Parameter typing note: All parameter values passed to
  `TopLevelParameter(...)` and `BlockParameter(...)` must be strings. For
  example, use `TopLevelParameter("nindiv_max", "1")` rather than
  `TopLevelParameter("nindiv_max", 1)`. When constructing simulations
  programmatically, convert values explicitly with `str(value)`.

- **Instruction files and PFTs**: A list of `.ins` files and PFT names to run.
  Each instruction file will be run once for each defined simulation.

- **Progress and output handling (optional)**: Objects can be supplied to
  receive progress updates and model stdout/stderr. For convenience, this
  package exports Python base classes that already implement the required .NET
  interfaces, so only the required methods need to be overridden:
  - `CustomProgressReporter.ReportProgress(percent, elapsed, ncomplete, njob)`
  - `CustomOutputHelper.ReportOutput(jobName, output)` / `ReportError(...)`

  See the "Customising Progress and Output" section below for more details.

- **Outputs**: Files are written under the output directory specified in
  `RunSettings`. The structure depends on the LPJ-Guess configuration and
  runner settings.

- **Result**: `run_simulations(...)` returns an `ExperimentResult` with summary
  counts (`TotalJobs`, `SuccessfulJobs`, `FailedJobs`) and `Error` if present.

Notes for Python users:

- When using custom progress/output handlers, the overriden hooks may be called
  from background threads; handlers should therefore be fast and thread-safe.
- If only console output is required, use `ConsoleProgressReporter()` and
  `ConsoleOutputHelper()` and skip writing custom classes.
- To customise behavior, subclass the provided bases rather than implementing
  .NET interfaces directly.

- Parameter values for `TopLevelParameter` and `BlockParameter` are string
  typed. Convert numeric or boolean values with `str(...)` when building
  simulation lists programmatically.

## Example Usage

```python
from lpjguess_runner import *

run_settings = RunSettings.Local(
    "/path/to/guess/executable",
    "/path/to/output/directory",
    "nc",   # input module
    4,      # cpu count
    "job_name",
    True)   # Whether to allow context switching of LPJ-Guess processes between CPUs (recommended: True)

simulations = [
    simulation("nindiv_max_0_sla_26", [               # Run all .ins files with:
        TopLevelParameter("wateruptake", "wcont"),    # - wateruptake = wcont
        BlockParameter("pft", "MRS", "sla", "26")     # - SLA of "MRS" pft set to 26
    ]),
    simulation("nindiv_max_1_sla_39", [               # Run all .ins files with:
        TopLevelParameter("wateruptake", "rootdist"), # - wateruptake = rootdist
        BlockParameter("pft", "MRS", "sla", "39")     # - SLA of "MRS" pft set to 39
    ]),
]

ins = ["/path/to/file1.ins", "/path/to/file2.ins"]
pfts = ["MRS"]

result = run_simulations(run_settings,
                         simulations,
                         ins,
                         pfts,
                         ConsoleProgressReporter(),   # Write progress messages to stdout
                         ConsoleOutputHelper())       # Propagate subprocess output to stdout

print(f"Total jobs: {result.TotalJobs}")
print(f"Successful jobs: {result.SuccessfulJobs}")
print(f"Failed jobs: {result.FailedJobs}")
print(f"Error: {result.Error}")
```

## Customising Progress and Output

The default helpers `ConsoleProgressReporter()` and `ConsoleOutputHelper()`
propagate progress and model stdout/stderr to stdout. For custom behavior,
subclass the Python base classes exported by this package:

- `CustomProgressReporter` (implements `.NET` `IProgressReporter`).
- `CustomOutputHelper` (implements `.NET` `IOutputHelper`).

These hide pythonnet interop details so only the necessary methods must be
overridden in subclasses.

Example: capture stdout/stderr and print compact progress

```python
from lpjguess_runner import *
import threading

class MyOutput(CustomOutputHelper):
    def __init__(self):
        super().__init__()
        self.stdout = {}
        self.stderr = {}
        self._lock = threading.Lock()

    def ReportOutput(self, jobName, output):
        with self._lock:
            self.stdout.setdefault(jobName, []).append(output)

    def ReportError(self, jobName, output):
        with self._lock:
            self.stderr.setdefault(jobName, []).append(output)

class MyProgress(CustomProgressReporter):
    def ReportProgress(self, percent, elapsed, ncomplete, njob):
        print(f"[{elapsed}] {ncomplete}/{njob} ({percent:.1f}%)")

out = MyOutput()
pr = MyProgress()

result = run_simulations(run_settings, simulations, ins, pfts, pr, out)
```

Notes

- Methods may be called from background threads; keep handlers fast and
  thread-safe.
- Output can be frequent; consider buffering or filtering.
- Advanced: if implementing the .NET interfaces directly instead of subclassing
  these bases, the class must inherit from `System.Object`, call
  `Object.__init__`, and set a valid `__namespace__` (for example
  `"LpjGuess.Runner.Python"`). Using the provided base classes is recommended.

## Results and Outputs

`run_simulations(...)` returns an `ExperimentResult` with at least:

- `TotalJobs`
- `SuccessfulJobs`
- `FailedJobs`
- `Error`

Files are written under the output directory provided to `RunSettings.Local`.
If jobs produce per-run subdirectories, those will appear under that directory.
