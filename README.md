# Peek At Changelog

Allows one to extract the release notes from a changelog, think `CHANGELOG.md` according to the https://keepachangelog.com format, for a particular version or fallback to the `Unreleased` notes if none specified or no matching version found. How you compose the changelog is out of scope. What you do with the output of the tool is out of scope.

## Installation

Install as a [.NET tool](https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools) using

`dotnet tool install peek-at-changelog`

## Usage

```
USAGE: peek-at-changelog [--help] [--version <version>] <path>

CHANGELOG:

    <path>                specify the path of the changelog

OPTIONS:

    --version <version>   specify the version
    --help                display this list of options.
```

## Behavior

- Assumes versions start with `##` (a level 2 heading). Supports both versions with and without a link definition. However, version links without a matching link definition will not work.
- If no version specified, it defaults to `Unreleased`. If no `Unreleased` version is specified, it defaults to an empty response. If a version is not found, it also defaults to an empty response.
- Automatically trims leading and trailing empty lines.

## Run

`dotnet peek-at-changelog CHANGELOG.md` if installed as a local tool or `peek-at-changelog CHANGELOG.md` if installed as a global tool.

## Acknowledgements

There are other efforts that may suit your needs better, so do compare. No hard feelings ;-)

- https://github.com/ionide/KeepAChangelog
- https://github.com/nullean/release-notes
- https://github.com/TheAngryByrd/MiniScaffold
- https://github.com/xoofx/dotnet-releaser
- https://github.com/fsprojects/FAKE (Fake.Core.ReleaseNotes)
