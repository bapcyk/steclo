namespace Steclo.Common

exception WrongFormat

type HashString = HashString of string
with override __.ToString () = (function | HashString s -> s) __

module Functions =
    begin
        let (|HexString|_|) (str : string) =
            let hexDigits = ['0'..'9'] @ ['a'..'f']
            let lstr = str.ToLower ()
            if Seq.forall (fun c -> List.contains c hexDigits) lstr = true then Some lstr
            else None

        let ParseHash (str : string) =
            match str with
            | HexString s -> Some <| HashString str
            | _ -> None
        
        let UnsafeParseHash (str : string) =
            match ParseHash (str) with
            | Some h -> h
            | None -> raise WrongFormat

    end