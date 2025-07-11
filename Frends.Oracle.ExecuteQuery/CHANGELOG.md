# Changelog

## [3.2.0] - 2025-03-25
### Changed
- Update packages:
  Microsoft.Extensions.DependencyInjection  5.0.1    -> 9.0.3
  Oracle.ManagedDataAccess.Core             3.21.170 -> 23.7.0
  System.ComponentModel.Annotations         4.7.0    -> 5.0.0
  coverlet.collector                        3.1.0    -> 6.0.4
  Microsoft.NET.Test.Sdk                    16.6.1   -> 17.13.0
  MSTest.TestFramework                      2.2.8    -> 3.8.3
  nunit                                     3.12.0   -> 4.3.2
  NUnit3TestAdapter                         3.17.0   -> 5.0.0

## [3.1.0] - 2025-03-25
### Fixed
- Fixed issue with converting query results to JToken.
- Updated Newtonsoft.Json to version 13.0.3.
- Updated Oracle.ManagedDataAccess.Core to version 3.21.170.

## [3.0.0] - 2025-03-24
### Added
- [Breaking] New 'ExecuteType' parameter to control how SQL command string is executed.
- Default value for new parameter is 'Auto'
- Use the default parameter if you want the Task to work as before.

## [2.0.1] - 2022-11-04
### Fixed
- Fixed issue which resulted to task not able to close connection.

## [2.0.0] - 2022-10-31
### Fixed
- [Breaking] Simplified implementation and merged ConnectionProperties and QueryProperties classes into one Input class.
- Changed the result output object to be dynamic to enable dot notation.
- Changed the implementation to resemble other database related tasks.
- Modified the task to be asynchronous.
- Changed the task use different methods when doing 'select' query and when doing other queries.
- Updated workflow for Linux based testing and enabled the tests.

## [1.0.0] - 2022-04-22
### Added
- Initial implementation.
