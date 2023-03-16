open Fli;
open Spectre.Console;

type Colors = Yellow | Green | Blue


let gitCredentials url =
  cli {
      Exec "git"
      Arguments [url;]
  } |> Command.execute


let main() =
    AnsiConsole.MarkupLine("[underline red]Hello[/] git-gr!");


main()
