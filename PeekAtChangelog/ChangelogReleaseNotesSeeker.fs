namespace PeekAtChangelog

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

        writer.ToString()

    let seek (document: MarkdownDocument) (version: string) =
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
