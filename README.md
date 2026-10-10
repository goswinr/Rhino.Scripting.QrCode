![logo](https://raw.githubusercontent.com/goswinr/Rhino.Scripting.QrCode/main/Docs/img/logo128.png)

# Rhino.Scripting.QrCode

[![Rhino.Scripting.QrCode on nuget.org](https://img.shields.io/nuget/v/Rhino.Scripting.QrCode)](https://www.nuget.org/packages/Rhino.Scripting.QrCode/)
[![Build Status](https://github.com/goswinr/Rhino.Scripting.QrCode/actions/workflows/build.yml/badge.svg)](https://github.com/goswinr/Rhino.Scripting.QrCode/actions/workflows/build.yml)
[![Docs Build Status](https://github.com/goswinr/Rhino.Scripting.QrCode/actions/workflows/docs.yml/badge.svg)](https://github.com/goswinr/Rhino.Scripting.QrCode/actions/workflows/docs.yml)
[![license](https://img.shields.io/github/license/goswinr/Rhino.Scripting.QrCode)](LICENSE.md)
![code size](https://img.shields.io/github/languages/code-size/goswinr/Rhino.Scripting.QrCode.svg)

Rhino.Scripting.QrCode is a small library for creating QR codes as [Rhino3D](https://www.rhino3d.com/) Meshes or Hatches.<br>
It uses [ZXing.Net](https://github.com/micjahn/ZXing.Net) to encode the text.<br>
The QR code is vector geometry, not an image, so it stays sharp at any size.

It works in Rhino 8 and later, from F#, C# and Python.

The QR code is created at the World Origin with a size of 1.0 x 1.0 units, using the high error correction level (30%).<br>
Text with non-ASCII characters is encoded as UTF-8.

## Use in F#

For example in the [Fesh.Rhino](https://github.com/goswinr/Fesh.Rhino) scripting editor:

```fsharp
#r "nuget: Rhino.Scripting.QrCode"
open Rhino
open Rhino.Geometry
open Rhino.Scripting

let doc = RhinoDoc.ActiveDoc

// as a Mesh, scaled to 50 units
let mesh = QrCode.asMesh "https://www.rhino3d.com"
mesh.Transform(Transform.Scale(Point3d.Origin, 50.0)) |> ignore
doc.Objects.AddMesh(mesh) |> ignore

// as Hatches, using the 'Solid' hatch pattern of the active document
for hatch in QrCode.asHatches "https://www.rhino3d.com" do
    doc.Objects.AddHatch(hatch) |> ignore

doc.Views.Redraw()
```

## Use in C#

For example in the Rhino 8 script editor:

```csharp
#r "nuget: Rhino.Scripting.QrCode"
using Rhino;
using Rhino.Scripting;

var mesh = QrCode.asMesh("https://www.rhino3d.com");
RhinoDoc.ActiveDoc.Objects.AddMesh(mesh);
RhinoDoc.ActiveDoc.Views.Redraw();
```

## Use in Python

1) Get the files `Rhino.Scripting.QrCode.dll`, `zxing.dll` and `FSharp.Core.dll` into one folder.<br>
   Run `dotnet build` on this repository, then they are all in `bin/Release/net48`.<br>
   If you download dll files instead, you might need to [unblock](https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.utility/unblock-file) them first:

   ![unblock](https://raw.githubusercontent.com/goswinr/Rhino.Scripting.QrCode/main/Docs/img/unblock.png)

2) Reference the dll in IronPython:

```python
import clr
clr.AddReferenceToFileAndPath(r"C:\path\to\bin\Release\net48\Rhino.Scripting.QrCode.dll")
import Rhino
from Rhino.Scripting import QrCode

mesh = QrCode.asMesh("https://www.rhino3d.com")
Rhino.RhinoDoc.ActiveDoc.Objects.AddMesh(mesh)
Rhino.RhinoDoc.ActiveDoc.Views.Redraw()
```

## Full API Documentation

[goswinr.github.io/Rhino.Scripting.QrCode](https://goswinr.github.io/Rhino.Scripting.QrCode/reference/rhino-scripting-qrcode.html)

## Alternatives

There are other QR code tools for Rhino that create images instead of geometry,<br>
see [discourse.mcneel.com](https://discourse.mcneel.com/search?q=qr%20code).

## Contributing
Contributions are welcome even for small things like typos. If you have problems with this library please submit an issue.

### License
[MIT](https://github.com/goswinr/Rhino.Scripting.QrCode/blob/main/LICENSE.md)

### Changelog
see [CHANGELOG.md](https://github.com/goswinr/Rhino.Scripting.QrCode/blob/main/CHANGELOG.md)
