open Spectre.Console
open Medallion.Shell
open System
open FSharpPlus
open FsHttp
open System.Text.Json
open System.Text.Json.Serialization

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

let run_http<'a> url =
    let run_unsafe u =
      http {
      config_ignoreCertIssues
      GET u
      } |> Request.send
        |> Response.deserializeJson<'a>
    Result.protect run_unsafe url

FsHttp.GlobalConfig.Json.defaultJsonSerializerOptions <-
    let options = JsonSerializerOptions()
    options.Converters.Add(JsonFSharpConverter())
    options

type Post = { id: int }

let main () =
    AnsiConsole.MarkupLine("[underline red]Hello[/] git-gerrit!")
    // gitCredentials "https://test.com" |> System.Console.WriteLine
    run_http<Post> "https://jsonplaceholder.typicode.com/posts/1"
    |> fun o -> printfn "obj %A" o

main ()
