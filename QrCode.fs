namespace Rhino.Scripting

open System.Collections.Generic
open Rhino
open Rhino.Geometry

open ZXing
open ZXing.QrCode
open ZXing.QrCode.Internal

//open ZXing.Common
//open ZXing.Aztec
//open ZXing.Rendering
//open ZXing.Datamatrix
//open ZXing.BarcodeFormat
//open ZXing.EncodeHintType
//open ZXing.WriterException
//open ZXing.qrcode.QRCodeWriter
//open ZXing.qrcode.decoder.ErrorCorrectionLevel

/// This module provides functions to create or QR codes as Rhino Hatch or Mesh
module QrCode = 

    // http://crunchify.com/java-simple-qr-code-generator-example/

    let private qrCodeWriter = new QRCodeWriter()
    
    let private hintMap = 
        let d = Dictionary()
        //d.[EncodeHintType.ERROR_CORRECTION] <-  ErrorCorrectionLevel.L :> obj // low 7% error correction, downcast required
        d.[EncodeHintType.ERROR_CORRECTION] <-  ErrorCorrectionLevel.H :> obj // high 30% error correction, downcast required
        d

    let private BMofTxt size txt = 
        let ba = qrCodeWriter.encode(txt,BarcodeFormat.QR_CODE, size, size, hintMap)
        ba

    let private as2dArray (bm:ZXing.Common.BitMatrix) = 
        let minX,minY,dX,dY = 
            match bm.getEnclosingRectangle() with 
            |[|minX;minY;dX;dY|] -> minX,minY,dX,dY
            |_ -> failwithf "Rhino.Scripting.QRcode.as2dArray: Err in QR code getEnclosingRectangle()"

        let arr2d = Array2D.zeroCreate (dX+1) (dY+1)
        let mutable xx = 0
        let mutable yy = 0
        for y = minY to minY+dY do
            for x = minX to minX+dX do
                //printfn "%d %d ; %d %d" xx yy x y
                arr2d.[xx,yy] <- bm.[x,y]
                xx <- xx+1
            xx <- 0
            yy <- yy+1
        arr2d
    
    let private meshAddQuadFace (m:Mesh, a:Point3d, b:Point3d, c:Point3d, d:Point3d) =  
        m.Faces.AddFace(    m.Vertices.Add (a.X,a.Y,a.Z), 
                            m.Vertices.Add (b.X,b.Y,b.Z), 
                            m.Vertices.Add (c.X,c.Y,c.Z), 
                            m.Vertices.Add (d.X,d.Y,d.Z)) |>ignore 
    
    /// Creats a QRcode Mesh of size 1 unit at 0,0
    let private asRhinoMesh (ar:bool[,]) : Mesh =
        let m = new Mesh()
        let sizeX = Array2D.length1 ar 
        let sizeY = Array2D.length2 ar 
        let stepX = 1./ float sizeX 
        let stepY = 1./ float sizeY 
        
        let mutable iyy = 0 // not reverse counter
        for iy = sizeY-1 downto 0 do // invert direction of Y axis
            let y1 =     (float iyy) * stepY  //row above x   
            let y2 = (float (iyy+1)) * stepY  //bottom row x               
            for ix = 0 to sizeX-1 do
                let x1 =     (float ix) * stepX      //left y
                let x2 = (float (ix+1)) * stepX      //right y 
                if ar.[ix,iy] then 
                    meshAddQuadFace (
                                    m,
                                    Point3d(x1,y1,0.0) ,
                                    Point3d(x2,y1,0.0) ,
                                    Point3d(x2,y2,0.0) ,
                                    Point3d(x1,y2,0.0))
            iyy <- iyy + 1
        m

    /// creats Hatch of size 1 at 0,0.
    /// Will use the default 'SOLID' hatch pattern
    let private asRhinoHatch (ar:bool[,]) : Hatch [] =        
        let crvs = ResizeArray<Curve>()
        let sizeX = Array2D.length1 ar 
        let sizeY = Array2D.length2 ar 
        let stepX = 1./ float sizeX 
        let stepY = 1./ float sizeY 
        
        let mutable iyy = 0 // not reverse counter
        for iy = sizeY-1 downto 0 do // invert direction of Y axis
            let y1 =     (float iyy) * stepY  //row above x   
            let y2 = (float (iyy+1)) * stepY  //bottom row x               
            for ix = 0 to sizeX-1 do
                let x1 =     (float ix) * stepX      //left y
                let x2 = (float (ix+1)) * stepX      //right y             
                if ar.[ix,iy] then 
                    let pts = [|Point3d(x1,y1,0.0) 
                                Point3d(x2,y1,0.0) 
                                Point3d(x2,y2,0.0) 
                                Point3d(x1,y2,0.0) 
                                Point3d(x1,y1,0.0)|]
                    let pl = new PolylineCurve(pts)
                    pl |> crvs.Add                    
            iyy <- iyy + 1        
        let pat = RhinoDoc.ActiveDoc.HatchPatterns.FindName("SOLID")
        if isNull pat then failwithf "Rhino.Scripting.QRcode 'SOLID' Hatchpattern for QR code not found. Please create it first."
        let hs = Hatch.Create (crvs, pat.Index , 0.0 , 100.0, 0.01)
        if isNull hs then failwithf "Rhino.Scripting.QRcode Hatchpattern for QR code is null"
        hs       

    //---------------------------------------------------------------------------------------------------------------
    //----------------------------------------- public functions: ---------------------------------------------------
    //---------------------------------------------------------------------------------------------------------------
        
    /// Creates a QR code as a Mesh made from Quads.  Located at World.Origin, size 1.0 unit.
    /// ErrorCorrectionLevel.H = high 30% 
    let asMesh txt : Mesh = 
        txt
        |> BMofTxt 50
        |> as2dArray
        |> asRhinoMesh

    /// Creates a QR code as Hatches.  Located at World.Origin, total size 1.0 unit.
    /// Will use the default 'SOLID' hatch pattern
    /// ErrorCorrectionLevel.H = high 30% 
    let asHatches(txt) : Hatch [] =   
        txt
        |> BMofTxt 50
        |> as2dArray
        |> asRhinoHatch
