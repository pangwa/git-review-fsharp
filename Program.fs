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

let MIN_GIT_VERSION = (2, 10, 0)

let VERBOSE = false

let runCmdIO cmd args =
    let arg_str =
        args
        |> Seq.map (fun a -> $"\"{a}\"")
        |> String.concat " "
    let full_cmd = cmd  + arg_str
    let cmd = Command.Run(cmd, args |> Seq.toArray)
    cmd.StandardInput.PipeFromAsync(Console.OpenStandardInput()) |> ignore
    cmd.StandardOutput.PipeToAsync(Console.OpenStandardOutput()) |> ignore
    cmd.StandardError.PipeToAsync(Console.OpenStandardError()) |> ignore
    let r = cmd.Result
    if r.Success = false then
        AnsiConsole.MarkupLine($"""[red]Command: {full_cmd} failed with code: {r.ExitCode}[/]""")

let runCmdGetOutput cmd args =
    let arg_str =
        args
        |> Seq.map (fun a -> $"\"{a}\"")
        |> String.concat " "
    let full_cmd = cmd + " " + arg_str
    printfn "cmd: %s" full_cmd
    let cmd = Command.Run(cmd, args |> Seq.toArray)
    let outputLines = new System.Collections.Generic.List<string>();
    cmd.StandardOutput.PipeToAsync(outputLines) |> ignore
    cmd.StandardError.PipeToAsync(outputLines) |> ignore
    cmd.Task.Wait()
    outputLines |> String.concat "\n"


let parse_review_number (review: string) =
    let parts = review.Split(",")
    if parts.Length < 2 then
        parts[0], None
    else
        parts[0], Some(parts[1])

let gitCredentials url =
    AnsiConsole.MarkupLine($"fill credential for [underline blue]{url}[/]")
    runCmdIO "git" [ "credential"; "fill" ]

let run_http<'a> url =
    let run_unsafe u =
      http {
      config_ignoreCertIssues
      GET u
      } |> Request.send
        |> Response.deserializeJson<'a>
    Result.protect run_unsafe url

let get_git_version () =
    let output = runCmdGetOutput "git" [ "version" ]
    printfn "output: %s" output

    if output.Contains("git version") then
        let ver = output.Split(" ")[2]

        let ver_array =
            ver.Split(".")[0..2] |> Array.map int |> (flip Array.append) [| 0; 0; 0 |]

        Ok((ver_array[0], ver_array[1], ver_array[2]))
    else
        Error("can't determine git version")

FsHttp.GlobalConfig.Json.defaultJsonSerializerOptions <-
    let options = JsonSerializerOptions()
    options.Converters.Add(JsonFSharpConverter())
    options

let git_directories () =
    // Determine (absolute git work directory path, .git subdirectory path).
    let work_tree = runCmdGetOutput "git" [ "rev-parse"; "--show-toplevel"; "--git-dir";]
    work_tree.Split("\n")

let git_config_and_value section option defaultVal asBool =
    let cmdPrefix = ["config"; "--get"; ]
    let cmdPostfix = [section + "." + option]
    let cmdPre = if asBool then cmdPrefix @ ["--bool"] else cmdPrefix
    let cmd = cmdPre @ cmdPostfix

    let output = (runCmdGetOutput "git" (cmd |> Seq.cast)).Trim()
    if output = "" then
        defaultVal
    else
        output

let git_string_config section  option defaultVal =
    git_config_and_value section option defaultVal false

let git_boolean_config section option defaultVal =
    let defaultStr = if defaultVal then "true" else "false"
    git_config_and_value section option defaultStr true |> Boolean.Parse


// type Post = { id: int }

let main () =
    AnsiConsole.MarkupLine("[underline red]Hello[/] git-gerrit!")
    // gitCredentials "https://test.com" |> System.Console.WriteLine
    // run_http<Post> "https://jsonplaceholder.typicode.com/posts/1"
    // |> fun o -> printfn "obj %A" o
    let ver = get_git_version () |> Result.get
    if ver < MIN_GIT_VERSION then
        failwith "local git version is too old"
    let name = git_string_config "user" "name" "unknown"
    let test = git_boolean_config "commit" "gpgsign2" false
    printfn "name: %s, %b" name test

main ()
