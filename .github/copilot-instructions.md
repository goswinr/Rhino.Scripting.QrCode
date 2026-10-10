# Rhino.Scripting.QrCode - AI Coding Guidelines

## Project Overview
Small F# library that creates QR codes as Rhino3D Meshes or Hatches, using [ZXing.Net](https://github.com/micjahn/ZXing.Net) for encoding. Targets .NET Framework 4.8 (Rhino 7) and .NET 8.0 (Rhino 8), the same as [Rhino.Scripting](https://github.com/goswinr/Rhino.Scripting). There are no tests in this project.

## Build & Release

```powershell
dotnet build  # Builds for both net48 and net8.0
```

- Version is extracted from [CHANGELOG.md](CHANGELOG.md) by `Ionide.KeepAChangelog.Tasks` — update the changelog to set the version.
- Release: commit, push to main, then `git tag 0.X.Y && git push origin 0.X.Y`. CI validates the tag matches the CHANGELOG version and publishes to NuGet.
- `FSharp.Core` is pinned at 6.0.7 — do not update.

## Architecture
All code is in [Src/QrCode.fs](Src/QrCode.fs), in the module `Rhino.Scripting.QrCode`:
- ZXing encodes the text into a `BitMatrix`, `as2dArray` crops the white quiet zone.
- `asMesh` returns one quad face per black module, `asHatches` uses the 'Solid' hatch pattern of the active document and adds it if missing.
- ASCII text uses ZXing's default encoding (ISO-8859-1), other text is encoded as UTF-8 with an ECI segment.
- `iterBlackModules` maps the module grid to the 1.0 x 1.0 square, shared by the Mesh and the Hatch functions.
- The QR code is placed at the World Origin with a total size of 1.0 x 1.0 units, Y axis pointing up.
- The public functions are module functions so that they can be called from C# and Python as static methods: `QrCode.asMesh(txt)`.

## Code Style
- XML doc comments (`///`) required on all public members — enforced by `--warnon:3390` and `FsDocsWarnOnMissingDocs`
- Error messages start with `"Rhino.Scripting.QrCode: "`
- `--warnon:1182` (unused variables) is enabled.

## Dependencies
- **ZXing.Net** — QR code encoding
- **RhinoCommon** — v7.x for net48, v8.x for net8.0 (PrivateAssets, not redistributed)
