# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.2.0] - 2026-10-10
### Added
- First release on nuget.org
### Changed
- Target net48, net8.0 and net10.0, for Rhino 8 and later. Rhino 6 and 7 are not supported anymore
- Reference RhinoCommon 8 instead of 6
- Update ZXing.Net from 0.16.6 to 0.16.11
- NuGet package id is `Rhino.Scripting.QrCode` instead of `Rhino6.Scripting.QrCode`
### Fixed
- The QR code had an extra empty row and column, so it was smaller than 1.0 unit and shifted up by one module
- Characters outside of ISO-8859-1, like '€' or '中', were encoded as '?'. Text with non-ASCII characters is encoded as UTF-8 now
- `asHatches` used a fixed tolerance of 0.01, bigger than a module of QR codes with more than 100 modules. It is relative to the module size now
- `asHatches` adds the 'Solid' hatch pattern to the document if it is missing, instead of failing
- `asMesh` returns a Mesh with normals
- README examples

## [0.1.0] - 2021-08-01
### Added
- First version, not published on nuget.org

[0.2.0]: https://github.com/goswinr/Rhino.Scripting.QrCode/releases/tag/0.2.0
