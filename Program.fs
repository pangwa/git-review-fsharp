open Spectre.Console
open Medallion.Shell
open System

type Colors =
    | Yellow
    | Green
    | Blue

let runGit cmd args =
    let cmd = Command.Run("git", args |> Seq.toArray)
    cmd.StandardInput.PipeFromAsync(Console.OpenStandardInput()) |> ignore
    cmd.StandardOutput.PipeToAsync(Console.OpenStandardOutput()) |> ignore
    cmd.StandardError.PipeToAsync(Console.OpenStandardError()) |> ignore
    cmd.Wait()

let gitCredentials url =
    AnsiConsole.MarkupLine($"fill credential for [underline blue]{url}[/]")
    runGit "git" [ "credential"; "fill" ]

let main () =
    AnsiConsole.MarkupLine("[underline red]Hello[/] git-gr!")
    gitCredentials "https://test.com" |> System.Console.WriteLine

main ()
