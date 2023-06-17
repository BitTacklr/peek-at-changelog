namespace PeekAtChangelog

open System
open System.IO
open System.Linq
open Markdig.Renderers.Roundtrip
open Markdig.Syntax
open Markdig.Syntax.Inlines

module ChangelogReleaseNotesSeeker =
    let private print_release_notes (document: MarkdownDocument) (block: Block) =
        let release_notes =
            document
                .Descendants()
                .SkipWhile(fun (element: Block) index -> element <> block)
                .Skip(1)
                .TakeWhile(fun (element: Block) index ->
                    match element with
                    | :? HeadingBlock as heading -> heading.Level <> 2
                    | :? LinkReferenceDefinition -> false
                    | _ -> true)
                .Where(fun element -> element.Parent = document)

        use writer = new StringWriter()
        let renderer = RoundtripRenderer(writer)

        for release_note in release_notes do
            renderer.Render(release_note) |> ignore

        let rendered_release_notes = writer.ToString()
        
        use reader = new StringReader(rendered_release_notes)
        let rendered_lines =
            Seq.unfold
                (fun (reader: StringReader) ->
                    let line = reader.ReadLine()
                    Option.ofObj line
                    |> Option.map (fun line -> (line, reader))
                ) reader
            |> List.ofSeq
        let first_line_with_content =
            List.tryFindIndex (fun (line: string) -> line.Length <> 0) rendered_lines
        let last_line_with_content =
            List.tryFindIndexBack (fun (line: string) -> line.Length <> 0) rendered_lines
        let trimmed_lines =
            List.choose
                (fun (index, line) ->
                    let skip =
                        (match first_line_with_content with
                        | Some first_line -> index < first_line
                        | None -> false)
                        ||
                        (match last_line_with_content with
                        | Some last_line -> index > last_line
                        | None -> false)
                    if skip then None else Some line
                )
                (rendered_lines |> List.mapi (fun index line -> (index, line)))
                
        let trimmed_release_notes = String.Join(Environment.NewLine, trimmed_lines)
        trimmed_release_notes

    let trySeek (document: MarkdownDocument) (version: string) =
        document.Descendants<HeadingBlock>().Where(fun heading -> heading.Level = 2)
        |> Seq.choose (fun block ->
            match Option.ofObj block.Inline with
            | Some content ->
                match content.FirstChild with
                | :? LiteralInline as literal when
                    literal.Content.MatchLowercase(version)
                    ->
                    Some(print_release_notes document block)
                | :? LinkInline as link ->
                    match link.FirstChild with
                    | :? LiteralInline as literal when literal.Content.MatchLowercase(version) ->
                        Some(print_release_notes document block)
                    | _ -> None
                | _ -> None
            | None -> None)
        |> Seq.tryHead
