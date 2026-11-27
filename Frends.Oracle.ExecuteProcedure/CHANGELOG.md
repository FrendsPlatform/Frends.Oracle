# Changelog

## [3.0.0] - 2026-11-23

### Changed

- **[Breaking Change] Input:** `DataReturnType` and `OutputParameters` moved from the Output tab to Input.
- **[Breaking Change] Connection:** Added the Connection tab with `ConnectionString`, moved from Input. `ClearConnectionPools`, `CloseConnection`, `ConnectionRetryAttempts`, and `ConnectionRetryDelayMs` moved from Options to Connection.
- **[Breaking Change] Result:** `Error` changed from a string to an `Error` object with `Message` and `AdditionalInfo` (the original exception).
- **[Breaking Change] Error handling:** When `ThrowErrorOnFailure` is enabled and no custom failure message is set, the task now throws the original exception instead of wrapping it in a generic exception.
- Added `ErrorMessageOnFailure` in `Options` tab, that prefixes the error message when execution fails.
- Update task to .NET 8.0

## [2.5.0] - 2026-05-26

### Added

- Added configurable retry mechanism for stale Oracle connections with `ConnectionRetryAttempts` (default: 2, range: 1-5) and `ConnectionRetryDelayMs` (default: 100ms) options.

### Fixed

- Fixed issue where cached connections would fail after Oracle server restart by adding automatic retry logic to detect and recover from stale connections.

## [2.4.0] - 2026-03-20

### Fixed

- Resolve issue with long time execution

### Added

- Add new options ( `CloseConnection`, `ClearConnectionPools`) to control connection behavior.

## [2.3.0] - 2025-12-29

### Fixed

- Improved Oracle parameter handling for NULL values and BLOBs.

## [2.2.0] - 2025-12-02

### Fixed

- Fix missing output parameter values

## [2.1.0] - 2025-03-25

### Changed

- Update packages:
  Microsoft.Extensions.DependencyInjection 6.0.0 -> 9.0.3
  Newtonsoft.Json 12.0.1 -> 13.0.3
  Oracle.ManagedDataAccess.Core 3.21.70 -> 23.7.0
  System.ComponentModel.Annotations 4.7.0 -> 5.0.0
  coverlet.collector 3.1.0 -> 6.0.4
  Microsoft.NET.Test.Sdk 16.6.1 -> 17.13.0
  MSTest.TestFramework 2.2.8 -> 3.8.3
  nunit 3.12.0 -> 4.3.2
  NUnit3TestAdapter 3.17.0 -> 5.0.0

## [2.0.0] - 2022-11-04

### Fixed

- [Breaking] change the parameter implementation to resemble other database related Tasks.
- Fixed issue that task was not able to close connection.

## [1.0.0] - 2022-10-12

### Added

- Initial implementation.
