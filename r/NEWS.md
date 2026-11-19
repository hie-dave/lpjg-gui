# LPJ-GUESS R Runner Changelog

## [Unreleased]

## [lpjguessRunner 1.0.1] - 2026-11-18

- Add support for overriding model process environment variables. If needed, use
  `run_settings_local(..., environment_overrides = list(PATH = "/my/path"))`.

## [lpjguessRunner 1.0.0] - 2026-11-16

### Updates

- Support for file blocks in instruction files

## [lpjguessRunner 0.9.0] - 2026-10-27

### Fixes

- Parameter changes now override the last occurrence of that parameter
