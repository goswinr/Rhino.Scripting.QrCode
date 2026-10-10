namespace Rhino.Scripting

open System.Collections.Generic
open Rhino
open Rhino.Geometry
open Rhino.DocObjects

open ZXing
open ZXing.QrCode
open ZXing.QrCode.Internal

/// This module provides functions to create QR codes as Rhino Meshes or Hatches.
module QrCode =

    // http://crunchify.com/java-simple-qr-code-generator-example/

    let private qrCodeWriter = new QRCodeWriter()

    let private hintsDefault =
        let d = Dictionary<EncodeHintType,obj>()
        //d.[EncodeHintType.ERROR_CORRECTION] <-  ErrorCorrectionLevel.L :> obj // low 7% error correction, downcast required
        d.[EncodeHintType.ERROR_CORRECTION] <-  ErrorCorrectionLevel.H :> obj // high 30% error correction, downcast required
        d

    /// Without this hint ZXing uses ISO-8859-1 and replaces characters like '€' or '中' with '?'.
    let private hintsUtf8 =
        let d = Dictionary<EncodeHintType,obj>(hintsDefault)
        d.[EncodeHintType.CHARACTER_SET] <- "UTF-8" :> obj // ZXing adds an ECI segment so that readers know it is UTF-8
        d

    let private BMofTxt size (txt:string) =
        // ASCII text keeps the default encoding, so that the QR code is the same as before and without an ECI segment
        let hints = if txt |> Seq.exists (fun c -> int c > 127) then hintsUtf8 else hintsDefault
        qrCodeWriter.encode(txt, BarcodeFormat.QR_CODE, size, size, hints)

    /// Returns the QR code modules without the white quiet zone around them.
    let private as2dArray (bm:ZXing.Common.BitMatrix) =
        let minX,minY,width,height =
            match bm.getEnclosingRectangle() with // returns left, top, width, height
            |[|minX;minY;width;height|] -> minX,minY,width,height
            |_ -> failwith "Rhino.Scripting.QrCode: getEnclosingRectangle() of the QR code failed."

        let arr2d = Array2D.zeroCreate width height
        for y = 0 to height-1 do
            for x = 0 to width-1 do
                arr2d.[x,y] <- bm.[minX+x, minY+y]
        arr2d

    /// Calls f with x1, y1, x2, y2: the lower left and upper right corner of each black module.
    /// The QR code is placed at 0,0 with a total size of 1.0 units.
    let private iterBlackModules (ar:bool[,]) (f: float -> float -> float -> float -> unit) =
        let sizeX = Array2D.length1 ar
        let sizeY = Array2D.length2 ar
        let stepX = 1./ float sizeX
        let stepY = 1./ float sizeY
        for iy = 0 to sizeY-1 do
            let row = sizeY-1-iy // invert direction of Y axis, row 0 of the array is the top row
            let y1 =     (float row) * stepY  // lower edge
            let y2 = (float (row+1)) * stepY  // upper edge
            for ix = 0 to sizeX-1 do
                if ar.[ix,iy] then
                    let x1 =     (float ix) * stepX  // left edge
                    let x2 = (float (ix+1)) * stepX  // right edge
                    f x1 y1 x2 y2

    /// Creates a QR code Mesh of size 1 unit at 0,0
    let private asRhinoMesh (ar:bool[,]) : Mesh =
        let m = new Mesh()
        iterBlackModules ar (fun x1 y1 x2 y2 ->
            m.Faces.AddFace(m.Vertices.Add (x1,y1,0.0),
                            m.Vertices.Add (x2,y1,0.0),
                            m.Vertices.Add (x2,y2,0.0),
                            m.Vertices.Add (x1,y2,0.0)) |> ignore )
        m.Normals.ComputeNormals() |> ignore
        m.Compact() |> ignore
        m

    /// Creates Hatches of size 1 at 0,0.
    /// Will use the 'Solid' hatch pattern of the active document, and add it if it is missing.
    let private asRhinoHatch (ar:bool[,]) : Hatch [] =
        let doc = RhinoDoc.ActiveDoc
        if isNull doc then failwith "Rhino.Scripting.QrCode: There is no active Rhino document to get the 'Solid' hatch pattern from."
        let solid = HatchPattern.Defaults.Solid
        let patternIndex =
            match doc.HatchPatterns.FindName(solid.Name) with
            | null -> doc.HatchPatterns.Add(solid) // the 'Solid' pattern might have been deleted from the document
            | pat -> pat.Index
        if patternIndex < 0 then failwith "Rhino.Scripting.QrCode: Failed to add the 'Solid' hatch pattern to the document."

        let crvs = ResizeArray<Curve>()
        iterBlackModules ar (fun x1 y1 x2 y2 ->
            let pts = [|Point3d(x1,y1,0.0)
                        Point3d(x2,y1,0.0)
                        Point3d(x2,y2,0.0)
                        Point3d(x1,y2,0.0)
                        Point3d(x1,y1,0.0)|]
            crvs.Add(new PolylineCurve(pts)) )

        // The whole QR code is only 1.0 units wide, so the tolerance must be relative to the module size.
        // A fixed tolerance of 0.01 is bigger than a module of a QR code with more than 100 modules.
        let tolerance = 0.01 / float (max (Array2D.length1 ar) (Array2D.length2 ar))
        let hs = Hatch.Create (crvs, patternIndex , 0.0 , 100.0, tolerance)
        if isNull hs then failwith "Rhino.Scripting.QrCode: Hatch.Create returned null."
        hs

    //---------------------------------------------------------------------------------------------------------------
    //----------------------------------------- public functions: ---------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------

    /// <summary>Creates a QR code as a Mesh made from Quads. Located at World Origin, size 1.0 unit.
    /// Uses ErrorCorrectionLevel.H (high, 30%).
    /// Text with non ASCII characters is encoded as UTF-8.</summary>
    /// <param name="txt">The text to encode in the QR code.</param>
    /// <returns>A Mesh with one quad face per black module of the QR code.</returns>
    let asMesh (txt:string) : Mesh =
        txt
        |> BMofTxt 50
        |> as2dArray
        |> asRhinoMesh

    /// <summary>Creates a QR code as Hatches. Located at World Origin, total size 1.0 unit.
    /// Uses the 'Solid' hatch pattern from the active Rhino document, and adds it to the document if it is missing.
    /// Uses ErrorCorrectionLevel.H (high, 30%).
    /// Text with non ASCII characters is encoded as UTF-8.</summary>
    /// <param name="txt">The text to encode in the QR code.</param>
    /// <returns>An array of Hatches covering the black modules of the QR code.</returns>
    let asHatches (txt:string) : Hatch [] =
        txt
        |> BMofTxt 50
        |> as2dArray
        |> asRhinoHatch
