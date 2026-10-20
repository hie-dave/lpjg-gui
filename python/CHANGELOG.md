# LPJ-GUESS Python Runner Changelog

All notable changes to this project will be documented in this file.


The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

Where applicable, this changelog may include relevant changes to the underlying
runner itself, in addition to changes in the Python wrapper.

## [Unreleased]

### [2.0.0]

#### Breaking Changes

- Breaking change: removed the `cleanup_policy` argument from
  `run_simulations(...)`. The cleanup policy should now specified in the
  `RunSettings(...)` constructor instead

#### Updates

- Add support for python 3.14.*

## [1.0.4] - 2026-06-24

- RunSettings now requires a cleanup policy, which is a breaking change. Consult
  the documentation for details on the available policies and their effects
