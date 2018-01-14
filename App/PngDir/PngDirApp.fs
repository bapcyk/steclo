open OptParse
open Steclo.Codec.Png
open Steclo.Order.Png
open Steclo.Common


type _CliOpts =
    {
        Group : list<int>
        Dir : string
        Mode : CodecMode
    }

type CliOpts = | InOpts of InOrderOpts | OutOpts of OutOrderOpts

let ParseOrder (str : string) = seq { for s in str.Split ',' -> s.Trim () |> int } |> Seq.toList

let ParseCliOpts args =
    let usage () = "[Usage]\n  %p %o"
    let appName = "png-dir"
    let defs = { Group=[]; Dir=""; Mode=Encode }
    let spec = [
        Option (descr="Group (selector)",
                callback=(fun o a -> {o with Group=ParseOrder a.[0]}:_CliOpts),
                extra=1, short="-t", long="--to-order");
        Option (descr="Folder",
                callback=(fun o a -> {o with Dir=a.[0]}:_CliOpts),
                required=true, extra=1, short="-i", long="--input-folder");
        Option (descr="Encode Mode",
                callback=(fun o _ -> {o with Mode=Encode}:_CliOpts),
                short="-E", long="--encode")
        Option (descr="Decode Mode",
                callback=(fun o _ -> {o with Mode=Decode}:_CliOpts),
                short="-D", long="--decode")
    ]
    try
        let (_, opts) = optParse spec usage appName args defs
        match opts.Mode with
        | Encode -> OutOpts { Group=opts.Group; Dir=opts.Dir }
        | Decode -> InOpts { Dir=opts.Dir }
    with
        | SpecErr err -> eprintfn "Invalid spec: %s" err; exit 1
        | RuntimeErr err -> eprintfn "Invalid options: %s" err
                            usagePrint spec appName usage (fun _ -> exit 1)


///////////////////////////////////////////////////////////////////////////////
[<EntryPoint>]
let main argv = 
    let f = @"d:\prj\fsharp\Steclo\dat\x.png"
    let v = {Num=1; From=15; Hash=Functions.UnsafeParseHash "aabbcc"}
    let saved = Functions.WriteMeta (f, v)
    printfn "saved = %A" saved
    let meta = Functions.ReadMeta (f)
    printfn "meta = %A" meta
    let cliOpts = ParseCliOpts argv
    match cliOpts with
    | InOpts _ -> printf "Not supported yet!"; 1
    | OutOpts oo ->
        // FIXME for test
        let o = new OutputOrder (oo)
        printfn "%s ; %A" o.Id o.opts.Group
        0 // return an integer exit code
