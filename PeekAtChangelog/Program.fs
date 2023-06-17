﻿open System
open System.IO
open Argu
open Markdig
open PeekAtChangelog

type Arguments =
    | [<MainCommand; Mandatory>] Changelog of path: string
    | [<Unique>] Version of version: string

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Changelog _ -> "specify the path of the changelog"
            | Version _ -> "specify the version"

try
    let parser =
        ArgumentParser.Create<Arguments>(programName = "dotnet peek-at-changelog")

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

        let pipeline = MarkdownPipelineBuilder().EnableTrackTrivia().Build()
        let markdown = File.ReadAllText(changelog)
        let document = Markdown.Parse(markdown, pipeline)

        match
            ChangelogReleaseNotesSeeker.seek document version
            |> Option.orElse (ChangelogReleaseNotesSeeker.seek document "unreleased")
        with
        | Some release_notes -> printfn $"%s{release_notes}"
        | None -> failwithf $"Could not find release notes for version '%s{version}' in '%s{changelog}'"

    exit 0

with error ->
    printfn $"%s{error.Message}"
    exit 1
