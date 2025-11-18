# lpjguess/__init__.py

from ._loader import ensure_loaded
ensure_loaded() # TODO: lazy loading?

from System import Array
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
