# lpjguess/_loader.py

from __future__ import annotations
import os, sys, platform
from pathlib import Path

# --- Crucial: Set PYTHONNET_RUNTIME to coreclr BEFORE pythonnet import ---
# See: GH issue pythonnet:pythonnet#2595
from pythonnet import load
load("coreclr")
# os.environ["PYTHONNET_RUNTIME"] = "coreclr"

import clr
from System.Runtime.Loader import AssemblyLoadContext

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
        raise FileNotFoundError(f"RID folder not found: {path}")
    return path

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

    if not core.exists() or not runner.exists():
        raise FileNotFoundError(f"Missing assemblies in {rid_dir}")

    # Load assemblies by absolute path in CoreCLR
    # Register assemblies by name with pythonnet so namespaces are importable
    AssemblyLoadContext.Default.LoadFromAssemblyPath(str(runner))
    clr.AddReference("LpjGuess.Runner")

    AssemblyLoadContext.Default.LoadFromAssemblyPath(str(core))
    clr.AddReference("LpjGuess.Core")

    _LOADED = True
