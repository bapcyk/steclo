// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.

open OptParse
//open Steclo.Codec.Png


//let ParseCliOpts args =
//    let usage () = "[Usage]\n  %p %o"
//    let appName = "png-codec"
//    let defs = { InputImg=""; OutDir=""; Mode=Encode }
//    let spec = [
//        Option (descr="Input image",
//                callback=(fun o a -> {o with InputImg=a.[0]}),
//                required=true, extra=1, short="-i", long="--input-image");
//        Option (descr="Output folder",
//                callback=(fun o a -> {o with OutDir=a.[0]}),
//                extra=1, short="-o", long="--out-dir")
//        Option (descr="Encode Mode",
//                callback=(fun o a -> {o with Mode=Encode}),
//                short="-E", long="--encode")
//        Option (descr="Decode Mode",
//                callback=(fun o a -> {o with Mode=Decode}),
//                short="-D", long="--decode")
//    ]
//    try
//        optParse spec usage appName args defs
//    with
//        | SpecErr err -> eprintfn "Invalid spec: %s" err; exit 1
//        | RuntimeErr err -> eprintfn "Invalid options: %s" err
//                            usagePrint spec appName usage (fun _ -> exit 1)


///////////////////////////////////////////////////////////////////////////////
[<EntryPoint>]
let main argv = 
    printfn "%A" argv
    0 // return an integer exit code
