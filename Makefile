SLN=src/LpjGuess.sln

# Defaults (override on CLI):
RID ?= linux-x64
TFM ?= net9.0
PUBLISH_OUT := publish/$(RID)
PKG_RIDS_DIR := python/lpjguess_runner/rids/$(RID)
DIST_DIR := python/dist
BUILD_DIR := python/build
EGG_INFO := python/lpjguess_runner.egg-info
PY_CACHE := python/lpjguess_runner/__pycache__
RIDS_ROOT := python/lpjguess_runner/rids
VENV ?= .venv


.PHONY: clean build run check coverage clean-py publish stage wheel install dev install-venv install-user clean-venv

build:
	dotnet build $(SLN)

clean: clean-py
	dotnet clean $(SLN)
	rm -rf coverage src/LpjGuess.Tests/TestResults

run:
	dotnet run --project src/LpjGuess.Frontend

check:
	dotnet test --collect:"XPlat Code Coverage" $(SLN)

# dotnet tool install -g dotnet-reportgenerator-globaltool
coverage: check
	reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coverage" -reporttypes:Html
	xdg-open coverage/index.html

# -----------------------------
# Packaging for Python (wheel)
# -----------------------------

clean-py: clean-venv
	# Remove last publish output and all staged RID payloads
	rm -rf $(PUBLISH_OUT) $(RIDS_ROOT)
	# Remove Python build artifacts
	rm -rf $(DIST_DIR) $(BUILD_DIR) $(EGG_INFO) $(PY_CACHE)
	# Also remove any stray egg-info under python/ (defensive)
	find python -maxdepth 1 -type d -name "*.egg-info" -exec rm -rf {} +

# Publish .NET Runner framework-dependent for the given RID/TFM
publish:
	dotnet publish src/LpjGuess.Runner -c Release -f $(TFM) -r $(RID) --self-contained false -o $(PUBLISH_OUT)

# Stage managed payload into the Python package rids/<rid>
stage: publish
	mkdir -p $(PKG_RIDS_DIR)
	cp -v $(PUBLISH_OUT)/* $(PKG_RIDS_DIR)/

# Build the wheel
wheel: stage
	cd python && python -m build --wheel

venv:
	python -m venv $(VENV)
	. $(VENV)/bin/activate && python -m pip install -U pip

# Remove local virtual environment (optional)
clean-venv:
	rm -rf $(VENV)

# Install the wheel into a local virtual environment
install-venv: wheel venv
	. $(VENV)/bin/activate && pip install --force-reinstall $(DIST_DIR)/*.whl

# Install the wheel for the current user (avoids system site-packages)
install-user: wheel
	pip install --user --force-reinstall $(DIST_DIR)/*.whl

# Convenience: one-shot local dev flow (no install)
dev: clean-py publish stage wheel
