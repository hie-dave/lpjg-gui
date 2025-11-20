# lpjguess/__init__.py

from ._loader import ensure_loaded
ensure_loaded() # TODO: lazy loading?

from System import Array, Object
from typing import Optional

# Import selected .NET symbols
from LpjGuess.Runner import ExperimentRunner, ExperimentResult
from LpjGuess.Runner.Models import RunSettings, RunnerConfiguration, IProgressReporter, IOutputHelper, ConsoleProgressReporter, ConsoleOutputHelper
from LpjGuess.Core.Models.Factorial import Simulation
from LpjGuess.Core.Models.Factorial.Factors import TopLevelParameter, BlockParameter
from LpjGuess.Core.Interfaces.Factorial import IFactor, ISimulation

__all__ = [
    # .NET types
    "ExperimentRunner", "ExperimentResult",
    "RunSettings", "RunnerConfiguration",
    "IProgressReporter", "IOutputHelper",
    "Simulation", "IFactor", "ISimulation",
    "TopLevelParameter", "BlockParameter",
    "ConsoleProgressReporter", "ConsoleOutputHelper",
    # Python helpers
    "simulation", "runner_config", "run_simulations",
    # Python base classes for extensibility
    "CustomOutputHelper", "CustomProgressReporter",
]

#
# Create a simulation object.
#
# @param name: Name of the simulation.
# @param factors: List of changes to be applied to the base instruction files.
# @return Simulation object.
#
def simulation(name: str, factors: list[IFactor]) -> Simulation:
    return Simulation(name, Array[IFactor](factors))

#
# Create a RunnerConfiguration object.
#
# @param run_settings Run settings (construct with RunSettings())
# @param simulations List of simulations.
# @param ins List of instruction files.
# @param pfts List of PFTs.
# @return RunnerConfiguration object.
#
def runner_config(run_settings: RunSettings,
                  simulations: list[ISimulation],
                  ins: list[str],
                  pfts: list[str]) -> RunnerConfiguration:
    return RunnerConfiguration(run_settings,
                               Array[ISimulation](simulations),
                               Array[str](ins),
                               Array[str](pfts))

def run_simulations(run_settings: RunSettings,
                    simulations: list[ISimulation],
                    ins: list[str],
                    pfts: list[str],
                    progress_reporter: Optional[IProgressReporter] = None,
                    output_helper: Optional[IOutputHelper] = None) -> ExperimentResult:
    cfg = runner_config(run_settings, simulations, ins, pfts)
    return ExperimentRunner().Run(cfg, progress_reporter, output_helper)

class CustomOutputHelper(Object, IOutputHelper):
    """
    Base class for implementing custom output handling in Python.

    Notes:
    - Methods may be called from background threads; keep handlers fast and
      thread-safe.
    - Output can be frequent; consider buffering or filtering.
    """
    __namespace__ = "LpjGuess.Runner.Python"

    def __init__(self):
        Object.__init__(self)

    def ReportOutput(self, jobName: str, output: str) -> None:
        """Handle a stdout line for a job."""
        raise NotImplementedError("ReportOutput must be overridden.")

    def ReportError(self, jobName: str, output: str) -> None:
        """Handle a stderr line for a job."""
        raise NotImplementedError("ReportError must be overridden.")

class CustomProgressReporter(Object, IProgressReporter):
    """
    Base class for implementing custom progress reporting in Python.

    Parameters:
    - percent: current overall progress as float in [0, 100].
    - elapsed: System.TimeSpan (total wall-clock time since start).
    - ncomplete: int (number of completed jobs).
    - njob: int (total number of jobs).
    """
    __namespace__ = "LpjGuess.Runner.Python"

    def __init__(self):
        Object.__init__(self)

    def ReportProgress(self,
                       percent: float,
                       elapsed,  # System.TimeSpan
                       ncomplete: int,
                       njob: int) -> None:
        """Handle a progress update."""
        raise NotImplementedError("ReportProgress must be overridden.")
