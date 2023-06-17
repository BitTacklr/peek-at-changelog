namespace TryOutPeekAtChangelog

open Markdig
open PeekAtChangelog
open Xunit

module ChangelogReleaseNotesSeekerTests =
    let pipeline = MarkdownPipelineBuilder().EnableTrackTrivia().Build()

    [<Fact>]
    let ``Seek returns notes of known version`` () =
        let document =
            Markdown.Parse(
                """# Changelog

## Release 1

Changes in release 1
""",
                pipeline
            )

        let result = ChangelogReleaseNotesSeeker.seek document "release 1"

        Assert.Equal(
            Some
                """
Changes in release 1
""",
            result
        )

    [<Fact>]
    let ``Seek returns notes of unreleased`` () =

        let document =
            Markdown.Parse(
                """# Changelog

## Unreleased

Working on release 1
""",
                pipeline
            )

        let result = ChangelogReleaseNotesSeeker.seek document "unreleased"

        Assert.Equal(
            Some
                """
Working on release 1
""",
            result
        )

    [<Fact>]
    let ``Seek returns notes of known linked version`` () =
        let document =
            Markdown.Parse(
                """# Changelog

## [Release 2]

Changes in release 2

## [Release 1]

Changes in release 1

[Release 2]: http://example.com
[Release 1]: http://example.com""",
                pipeline
            )

        let result = ChangelogReleaseNotesSeeker.seek document "release 2"

        Assert.Equal(
            Some
                """
Changes in release 2

""",
            result
        )

    [<Fact>]
    let ``Seek does not return notes of known link-look-alike version`` () =
        let document =
            Markdown.Parse(
                """# Changelog

## [Release 2]

Changes in release 2

## [Release 1]

Changes in release 1

[Release 1]: http://example.com""",
                pipeline
            )

        let result = ChangelogReleaseNotesSeeker.seek document "release 2"

        Assert.Equal(
            None,
            result
        )

    [<Fact>]
    let ``Seek returns notes of known linked version followed by link reference definitions`` () =
        let document =
            Markdown.Parse(
                """# Changelog

## [Release 1]

Changes in release 1

[Release 1]: http://example.com""",
                pipeline
            )

        let result = ChangelogReleaseNotesSeeker.seek document "release 1"

        Assert.Equal(
            Some
                """
Changes in release 1

""",
            result
        )

    [<Fact>]
    let ``Seek returns nothing for unknown version `` () =
        let document =
            Markdown.Parse(
                """# Changelog

## Release 1

Changes in release 1
""",
                pipeline
            )

        let result = ChangelogReleaseNotesSeeker.seek document "release 2"

        Assert.Equal(None, result)
