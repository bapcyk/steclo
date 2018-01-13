namespace Steclo.Order.Png

open System.IO
open System.Text
open System.Security.Cryptography

type OutOrderOpts =
    {
        From : list<int>
        To : list<int>
        Dir : string
    }

type Output =
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