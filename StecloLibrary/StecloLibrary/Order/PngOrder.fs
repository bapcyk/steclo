namespace Steclo.Order.Png

open System.IO
open System.Text
open System.Security.Cryptography
open Steclo.Codec.Png
open System.Windows.Media.Imaging

type OutOrderOpts =
    {
        Group : list<int>
        Dir : string
    }

type InOrderOpts =
    {
        Dir : string
    }

type OutputOrder =
    class
        val opts : OutOrderOpts
        val Id : string

        new (opts : OutOrderOpts) =
            let di = new DirectoryInfo (opts.Dir)
            let files = seq { for f in di.GetFiles () -> f.Name }
            let filesStr = String.concat "|" files
            let md5 = MD5.Create ()
            let hashBytes = md5.ComputeHash (Encoding.UTF8.GetBytes filesStr)
            let id = seq { for b in hashBytes -> b.ToString ("x2") } |> String.concat ""
            {
                opts = opts
                Id = id
            }
            then md5.Dispose ()

    end

module Functions =
    begin
        let MetaKey = "/tEXt/Scc"
        let WriteMeta_ (path : string, num : int, from : int, hash : string) =
            let data = sprintf "%d %d %s" num from hash
            use stm = new FileStream (path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite)
            let dec = new PngBitmapDecoder (stm, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.None)
            let frm = dec.Frames.[0]
            let wr = frm.CreateInPlaceBitmapMetadataWriter ()
            let mutable res = false
            if wr.TrySave () then
                wr.SetQuery (MetaKey, data.ToCharArray()) // Steclo collection
                res <- true
            stm.Close ()
            res

        let WriteMeta (path : string, num : int, from : int, hash : string) =
            let fip = new FileInfo (path)
            let data = sprintf "%d %d %s" num from hash
            use stm = new FileStream (path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite)
            let dec = new PngBitmapDecoder (stm, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad)
            let frm = dec.Frames.[0]
            let _met = frm.Metadata
            if _met = null then
                stm.Close ()
                false
            else
                let met = _met.Clone () :?> BitmapMetadata
                stm.Close ()
                fip.Delete ()
                met.SetQuery (MetaKey, data)
                let enc = new PngBitmapEncoder ()
                enc.Frames.Add (BitmapFrame.Create (frm, frm.Thumbnail, met, frm.ColorContexts))
                use stm' = File.Open (path, FileMode.Create, FileAccess.ReadWrite)
                enc.Save (stm')
                stm'.Close ()
                true

        let ReadMeta (path : string) =
            use stm = new FileStream (path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite)
            let dec = new PngBitmapDecoder (stm, BitmapCreateOptions.None, BitmapCacheOption.Default)
            let met = dec.Frames.[0].Metadata :?> BitmapMetadata
            if met = null then None
            else
                let data = met.GetQuery (MetaKey)
                let res =
                    match data with
                    | null -> None
                    | obj -> Some <| obj.ToString ()
                stm.Close ()
                res

    end