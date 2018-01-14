namespace Steclo.Codec.Png

open System.Drawing
open System.IO
open System
open System.Drawing.Imaging
open System.Runtime.InteropServices

exception EOF

type CodecMode =
    | Encode
    | Decode

type CodecOpts =
    {
        InputImg : string
        OutDir : string
        Mode : CodecMode
    }

type WRBitmap =
    class
        val Width : int
        val Height : int
        val Bits : array<uint32>
        val BitsHandle : GCHandle
        val Bitmap : Bitmap
        val Length : int64
        val mutable Disposed : bool

        new (sizeLikeFile : string) =
            let img : Image = Image.FromFile (sizeLikeFile)
            new WRBitmap (img.Width, img.Height)
            then img.Dispose ()
            
        new (width : int, height : int) =
            let bits = Array.zeroCreate (width * height)
            let bitsHandle = GCHandle.Alloc (bits, GCHandleType.Pinned)
            let bitmap = new Bitmap (width, height, width * 4, PixelFormat.Format32bppArgb, bitsHandle.AddrOfPinnedObject ())
            {
                Width = width
                Height = height
                Bits = bits
                BitsHandle = bitsHandle
                Bitmap = bitmap
                Length = width * height |> int64
                Disposed = false
            }
        
        member inline __.SetPixel (x : int, y : int, color : Color) =
            let i = x + (y * __.Width)
            let c = color.ToArgb ()
            __.Bits.[i] <- uint32 c

        member inline __.GetPixel (x : int, y : int) =
            let i = x + (y * __.Width)
            let c = __.Bits.[i]
            Color.FromArgb (int c)

        member __.Save (path : string) =
            __.Bitmap.Save (path, ImageFormat.Png)

        interface IDisposable with
            member __.Dispose () =
                if __.Disposed <> true then
                    __.Bitmap.Dispose ()
                    __.BitsHandle.Free ()
                    __.Disposed <- true

    end

type Encoder =
    class
        val opts : CodecOpts
        val Img : WRBitmap

        new (opts : CodecOpts) =
            {
                opts = opts
                Img = new WRBitmap (opts.InputImg)
            }

        member inline __.EncodeByte (c : Color) (bt : byte) =
            let a : byte = (byte c.A &&& 0xFCuy) ||| (0x03uy &&& bt)
            let r : byte = (byte c.R &&& 0xFCuy) ||| (0x03uy &&& (bt >>> 2))
            let g : byte = (byte c.G &&& 0xFCuy) ||| (0x03uy &&& (bt >>> 4))
            let b : byte = (byte c.B &&& 0xFCuy) ||| (0x03uy &&& (bt >>> 6))
            Color.FromArgb (int a, int r, int g, int b)

        member __.OutImgPath
            with get () =
                let name = Path.GetFileNameWithoutExtension (__.opts.InputImg)
                let path = Path.Combine (__.opts.OutDir, name + ".png")
                Directory.CreateDirectory (__.opts.OutDir) |> ignore
                path

        member __.Encode (inStream : Stream) =
            let mutable b : int = 0
            use inImg = new Bitmap (__.opts.InputImg)
            for y = 0 to __.Img.Height - 1 do
                for x = 0 to __.Img.Width  - 1 do
                    b <- inStream.ReadByte () // TODO check -1, gen random after it
                    let c = inImg.GetPixel (x, y)
                    let c' = __.EncodeByte c (byte b) // truncate int to byte
                    __.Img.SetPixel (x, y, c')
            __.Img.Save (__.OutImgPath)
            inImg.Dispose ()

        member __.Encode () =
            let inStream = Console.OpenStandardInput ()
            let outStream = Console.OpenStandardOutput ()
            let mutable b : int = 0
            __.Encode (inStream)
            while (b <- inStream.ReadByte (); b) <> -1 do
                outStream.WriteByte (byte b)
            inStream.Close ()
            outStream.Close ()

    end

type Decoder =
    class
        val opts : CodecOpts

        new (opts : CodecOpts) =
            {
                opts = opts
            }
        
        member inline __.DecodeByte (c : Color) =
            let mutable v : byte = 0uy
            v <- v ||| (c.A &&& 0x03uy)
            v <- v ||| ((c.R &&& 0x03uy) <<< 2)
            v <- v ||| ((c.G &&& 0x03uy) <<< 4)
            v <- v ||| ((c.B &&& 0x03uy) <<< 6)
            v

        member __.Decode () =
            let outStream = Console.OpenStandardOutput ()
            use outImg = new Bitmap (__.opts.InputImg)
            for y = 0 to outImg.Height - 1 do
                for x = 0 to outImg.Width - 1 do
                    let c = outImg.GetPixel (x, y)
                    outStream.WriteByte (__.DecodeByte c)
            outImg.Dispose ()

    end


