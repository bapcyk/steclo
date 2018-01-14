﻿namespace Steclo.Order.Png

open System.IO
open System.Text
open System.Security.Cryptography
open Steclo.Codec.Png
open Steclo.Common
open System.Windows.Media.Imaging


exception CorruptedMetaData

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

type MetaValue =
    {
        Num : int
        From : int
        Hash : HashString
    }

module Functions =
    begin
        let MetaKey = "/tEXt/Scc" // Steclo collection

        let WriteMeta (path : string, data : MetaValue) =
            let fip = new FileInfo (path)
            let data = sprintf "%d %d %O" data.Num data.From data.Hash // %O calls ToString
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

        let ReadRawMeta (path : string) =
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

        let ReadMeta (path : string) =
            try
                match ReadRawMeta (path) with
                | None -> None
                | Some str ->
                    match str.Split ' ' with
                    | [|num; from; hash|] ->
                        match Functions.ParseHash hash with
                        | Some hash -> Some { Num=int num; From=int from; Hash=hash }
                        | _ -> None
                    | _ -> None
            with _ -> raise CorruptedMetaData

    end