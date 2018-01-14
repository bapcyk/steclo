(* TODO each PNG must have meta {sequence-long; encoded-originan-name} - to restore order from Web w/o names in URL
   TODO
*)

open OptParse
open Steclo.Codec.Png


let ParseCliOpts args =
    let usage () = "Syntax:\n  %p %o"
    let appName = "png-codec"
    let defs = { InputImg=""; OutDir=""; Mode=Encode }
    let spec = [
        Option (descr="Input image",
                callback=(fun o a -> {o with InputImg=a.[0]}),
                required=true, extra=1, short="-i", long="--input-image");
        Option (descr="Output directory",
                callback=(fun o a -> {o with OutDir=a.[0]}),
                extra=1, short="-o", long="--out-dir")
        Option (descr="Encode Mode",
                callback=(fun o _ -> {o with Mode=Encode}),
                short="-E", long="--encode")
        Option (descr="Decode Mode",
                callback=(fun o _ -> {o with Mode=Decode}),
                short="-D", long="--decode")
    ]
    try
        optParse spec usage appName args defs
    with
        | SpecErr err -> eprintfn "Invalid spec: %s" err; exit 1
        | RuntimeErr err -> eprintfn "Invalid options: %s" err
                            usagePrint spec appName usage (fun _ -> exit 1)

///////////////////////////////////////////////////////////////////////////////

[<EntryPoint>]
let main argv =
    let _, cliOpts = ParseCliOpts argv
    try
        match cliOpts.Mode with
            | Encode -> 
                let enc = new Encoder (cliOpts)
                enc.Encode ()
                0
            | Decode ->
                let dec = new Decoder (cliOpts)
                dec.Decode ()
                0
    with
        | :? EOF -> eprintf "End of data!"; 1
        | _ as x -> eprintf "Error: %A" x; 1


