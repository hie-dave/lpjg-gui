# lpjguess

Python wrapper for LPJ-Guess experiment runner (via pythonnet + .NET 9)

## Install

- Prerequisites: .NET 9 runtime

```bash
pip install lpjguess
```

## Example Usage

```python
from lpjguess import *

run_settings = RunSettings.Local(
    "/path/to/guess/executable",
    "/path/to/output/directory",
    "nc",   # input module
    4,      # cpu count
    "job_name")

simulations = [
    simulation("nindiv_max_0_sla_26", [           # Run all .ins files with:
        TopLevelParameter("nindiv_max", "0"),     # - nindiv_max = 0
        BlockParameter("sla", "pft", "MRS", "26") # - SLA of "MRS" pft set to 26
    ]),
    simulation("nindiv_max_1_sla_39", [           # Run all .ins files with:
        TopLevelParameter("nindiv_max", "1"),     # - nindiv_max = 1
        BlockParameter("sla", "pft", "MRS", "39") # - SLA of "MRS" pft set to 39
    ]),
]

ins = ["/path/to/file1.ins", "/path/to/file2.ins"]
pfts = ["MRS"]

result = run_simulations(run_settings,
                            simulations,
                            ins,
                            pfts,
                            ConsoleProgressReporter(), # Write progres messages to stdout
                            ConsoleOutputHelper())     # Propagate subprocess output to stdout

print(f"Total jobs: {result.TotalJobs}")
print(f"Successful jobs: {result.SuccessfulJobs}")
print(f"Failed jobs: {result.FailedJobs}")
print(f"Error: {result.Error}")
```
