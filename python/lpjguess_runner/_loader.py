# lpjguess/_loader.py

from __future__ import annotations
import sys, platform
from pathlib import Path

# --- Crucial: Set PYTHONNET_RUNTIME to coreclr BEFORE pythonnet import ---
# See: GH issue pythonnet:pythonnet#2595
from pythonnet import load
load("coreclr")
# os.environ["PYTHONNET_RUNTIME"] = "coreclr"

import clr
from System import AppDomain

_TFM = "net9.0"
_LOADED = False

# Detect the RID (.NET Runtime Identifier) for the current platform.
def _detect_rid():
    system = platform.system().lower()
    machine = platform.machine().lower()

    if system.startswith("linux"):
        if machine in ("x86_64", "amd64"):
            return "linux-x64"
        elif machine in ("aarch64", "arm64"):
            return "linux-arm64"
    elif system.startswith("win"):
        return "win-x64"  # most common; extend if needed
    elif system == "darwin":
        if machine == "arm64":
            return "osx-arm64"
        elif machine in ("x86_64", "amd64"):
            return "osx-x64"
    raise RuntimeError(f"Unsupported platform: system={system}, machine={machine}")

def _rid_path() -> Path:
    # Package data: lpjguess/rids/<rid>/
    pkg_dir = Path(__file__).resolve().parent
    rid = _detect_rid()
    path = pkg_dir / "rids" / rid
    if not path.exists():
        raise FileNotFoundError(
            f"RID folder not found: {path}\n"
            f"Detected RID: {rid}\n"
            f"Package directory: {pkg_dir}\n"
            f"Available RID folders: {_available_rids(pkg_dir)}")
    return path

def _available_rids(pkg_dir: Path) -> str:
    rids_dir = pkg_dir / "rids"
    if not rids_dir.exists():
        return "<no rids directory>"
    names = sorted(path.name for path in rids_dir.iterdir() if path.is_dir())
    return ", ".join(names) if names else "<none>"

def _loaded_lpjguess_assemblies() -> str:
    names = sorted(
        assembly.GetName().Name
        for assembly in AppDomain.CurrentDomain.GetAssemblies()
        if assembly.GetName().Name.startswith("LpjGuess"))
    return ", ".join(names) if names else "<none>"

def _validate_namespace_imports() -> None:
    try:
        from LpjGuess.Runner import ExperimentRunner  # noqa: F401
        from LpjGuess.Core.Models.Factorial import Simulation  # noqa: F401
    except Exception as error:
        raise ImportError(
            "Loaded LPJ-Guess .NET assemblies, but pythonnet could not import "
            "their CLR namespaces.\n"
            f"Loaded LPJ-Guess assemblies: {_loaded_lpjguess_assemblies()}\n"
            "This usually means the assemblies were not registered with "
            "pythonnet's import hook. Try reinstalling the lpjguess-runner "
            "package so the patched loader is used.\n"
            f"Original import error: {type(error).__name__}: {error}"
        ) from error

def ensure_loaded() -> None:
    global _LOADED
    if _LOADED:
        return

    rid_dir = _rid_path()

    # Ensure dependency DLLs can be probed by user code if needed
    if str(rid_dir) not in sys.path:
        sys.path.append(str(rid_dir))

    # Load Core first, then Runner; use absolute paths via AssemblyLoadContext
    core = rid_dir / "LpjGuess.Core.dll"
    runner = rid_dir / "LpjGuess.Runner.dll"

    missing = [str(path) for path in (core, runner) if not path.exists()]
    if missing:
        available = "\n".join(f"  - {path.name}" for path in sorted(rid_dir.iterdir()))
        raise FileNotFoundError(
            "Missing required LPJ-Guess assemblies:\n"
            + "\n".join(f"  - {path}" for path in missing)
            + f"\nRID directory: {rid_dir}\nAvailable files:\n{available}")

    try:
        # Register assemblies with pythonnet by absolute path. Loading only via
        # AssemblyLoadContext makes reflection work, but can leave CLR
        # namespaces unavailable to Python imports.
        clr.AddReference(str(core))
        clr.AddReference(str(runner))
        _validate_namespace_imports()
    except Exception as error:
        raise RuntimeError(
            "Failed to load LPJ-Guess .NET assemblies for pythonnet.\n"
            f"RID directory: {rid_dir}\n"
            f"Core assembly: {core} (exists={core.exists()})\n"
            f"Runner assembly: {runner} (exists={runner.exists()})\n"
            f"Loaded LPJ-Guess assemblies: {_loaded_lpjguess_assemblies()}\n"
            f"Python: {sys.version}\n"
            f"Platform: {platform.platform()}\n"
            f"Original error: {type(error).__name__}: {error}"
        ) from error

    _LOADED = True
