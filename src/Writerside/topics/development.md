# Development

## Build and test

Install the .NET 10 SDK, then run these commands from the repository root:

```bash
dotnet restore src/Asv.Hal.slnx
dotnet build src/Asv.Hal.slnx -c Release --no-restore
dotnet test src/Asv.Hal.slnx -c Release --no-build --no-restore
```

The `Code Quality` workflow (`code-quality.yml`) builds and tests pull requests,
pushes to the main and release branches, and manual runs.

## Documentation

Edit topics in `src/Writerside/topics` and register them in `src/Writerside/asv-hal.tree`.
Open `src/Writerside` in Writerside for a local preview.

The `Publish documentation` workflow (`docs.yml`) builds and checks documentation
for pull requests. Pushes to `main` publish the checked website to
[GitHub Pages](https://asv.me/asv-hal/). Manual runs build and check any selected
branch; only `main` deploys. GitHub Pages must use **GitHub Actions** as its source.

The workflow reads `ProductVersion` from `src/Directory.Build.props` and sets the
Writerside instance version before every documentation build. Changing this file
also triggers documentation validation and publication. Do not maintain a separate
hard-coded documentation version.

To reproduce the website build locally with Docker running, first set the
`version` attribute on the `asv-hal.tree` instance in a temporary copy of
`writerside.cfg` to the current `ProductVersion`, then run from that copy's
repository root:

```bash
mkdir -p artifacts
docker run --rm \
  -v "$PWD:/opt/sources" \
  -e SOURCE_DIR=/opt/sources/src \
  -e MODULE_INSTANCE=Writerside/asv-hal \
  -e OUTPUT_DIR=/opt/sources/artifacts \
  -e RUNNER=other \
  jetbrains/writerside-builder:2026.08.0328
```

C# code blocks use the Writerside language name `C#` and `noinject="true"` to retain C# output highlighting without
requiring a C# language plugin in the Writerside builder. See the
[Writerside code-block reference](https://www.jetbrains.com/help/writerside/semantic-markup-reference.html#code-block).
Compile complete
programs in a .NET 10 console project referencing the current Asv.Hal project;
compile helper types as a library or add a caller. Hardware examples require
appropriate devices and are not executed by the documentation builder.

Inspect `artifacts/report.json` for errors. The CI checker fails the workflow if
the report contains documentation errors. The website archive is
`artifacts/webHelpASV-HAL2-all.zip`.
