open OptParse
open Steclo.Codec.Png
open Steclo.Order.Png


type _CliOpts =
    {
        From : list<int>
        To : list<int>
        Dir : string
        Mode : CodecMode
    }

type CliOpts = | InOpts of InOrderOpts | OutOpts of OutOrderOpts

let ParseOrder (str : string) = seq { for s in str.Split ',' -> s.Trim () |> int } |> Seq.toList

let ParseCliOpts args =
    let usage () = "[Usage]\n  %p %o"
    let appName = "png-dir"
    let defs = { From=[]; To=[]; Dir=""; Mode=Encode }
    let spec = [
        Option (descr="From order",
                callback=(fun o a -> {o with From=ParseOrder a.[0]}:_CliOpts),
                extra=1, short="-f", long="--from-order");
        Option (descr="To order",
                callback=(fun o a -> {o with To=ParseOrder a.[0]}:_CliOpts),
                extra=1, short="-t", long="--to-order");
        Option (descr="Folder",
                callback=(fun o a -> {o with Dir=a.[0]}:_CliOpts),
                required=true, extra=1, short="-i", long="--input-folder");
        Option (descr="Encode Mode",
                callback=(fun o a -> {o with Mode=Encode}:_CliOpts),
                short="-E", long="--encode")
        Option (descr="Decode Mode",
                callback=(fun o a -> {o with Mode=Decode}:_CliOpts),
                short="-D", long="--decode")
    ]
    try
        let (_, opts) = optParse spec usage appName args defs
        match opts.Mode with
        | Encode -> OutOpts { From=opts.From; To=opts.To; Dir=opts.Dir }
        | Decode -> InOpts { Dir=opts.Dir }
    with
        | SpecErr err -> eprintfn "Invalid spec: %s" err; exit 1
        | RuntimeErr err -> eprintfn "Invalid options: %s" err
                            usagePrint spec appName usage (fun _ -> exit 1)


///////////////////////////////////////////////////////////////////////////////
[<EntryPoint>]
let main argv = 
    let cliOpts = ParseCliOpts argv
    printfn "%A" cliOpts
    0 // return an integer exit code
