﻿open System
open System.IO
open Argu
open Markdig
open PeekAtChangelog

type Arguments =
    | [<MainCommand; Mandatory>] Changelog of path: string
    | [<Unique>] Version of version: string
    | [<Unique>] Fallback

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Changelog _ -> "specify the path of the changelog"
            | Version _ -> "specify the version"
            | Fallback -> "falls back to unreleased version if version not found"

try
    let parser =
        ArgumentParser.Create<Arguments>(programName = "peek-at-changelog")

    let parsed =
        parser.ParseCommandLine(Environment.GetCommandLineArgs(), ignoreUnrecognized = true, raiseOnUsage = true)

    if parsed.IsUsageRequested then
        printfn $"%s{parser.PrintUsage()}"
    else
        let changelog = parsed.GetResult <@ Changelog @>

        if not (File.Exists(changelog)) then
            failwithf $"Could not find changelog at path %s{changelog}"

        let version =
            parsed.TryGetResult <@ Version @>
            |> Option.map (fun value -> value.ToLowerInvariant())
            |> Option.defaultValue "unreleased"
            
        let fallbackToUnreleased =
            parsed.TryGetResult <@ Fallback @>
            |> Option.map (fun _ -> true)
            |> Option.defaultValue false

        let pipeline = MarkdownPipelineBuilder().EnableTrackTrivia().Build()
        let markdown = File.ReadAllText(changelog)
        let document = Markdown.Parse(markdown, pipeline)

        ChangelogReleaseNotesSeeker.trySeek document version
        |> Option.orElseWith (fun () ->
            if fallbackToUnreleased && version <> "unreleased" then
                ChangelogReleaseNotesSeeker.trySeek document "unreleased"
            else
                None)
        |> Option.iter (fun release_notes -> printfn $"%s{release_notes}")

    exit 0

with error ->
    printfn $"%s{error.Message}"
    exit 1
