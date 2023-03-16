open Spectre.Console
open Medallion.Shell
open System
open FsHttp

type Colors =
    | Yellow
    | Green
    | Blue

let runGit args =
    let arg_str =
        args
        |> Seq.map (fun a -> $"\"{a}\"")
        |> String.concat " "
    let full_cmd = "git " + arg_str
    let cmd = Command.Run("git", args |> Seq.toArray)
    cmd.StandardInput.PipeFromAsync(Console.OpenStandardInput()) |> ignore
    cmd.StandardOutput.PipeToAsync(Console.OpenStandardOutput()) |> ignore
    cmd.StandardError.PipeToAsync(Console.OpenStandardError()) |> ignore
    let r = cmd.Result
    if r.Success = false then
        AnsiConsole.MarkupLine($"""[red]Command: {full_cmd} failed with code: {r.ExitCode}[/]""")

    r

let parse_review_number (review: string) =
    let parts = review.Split(",")
    if parts.Length < 2 then
        parts[0], None
    else
        parts[0], Some(parts[1])

let gitCredentials url =
    AnsiConsole.MarkupLine($"fill credential for [underline blue]{url}[/]")
    runGit [ "credential"; "fill" ]

let run_http url =
    http {
      config_ignoreCertIssues
      GET url
    } |> Request.send
    |> Response.toFormattedText // TODO: use json?


let main () =
    AnsiConsole.MarkupLine("[underline red]Hello[/] git-gerrit!")
    gitCredentials "https://test.com" |> System.Console.WriteLine

main ()
