# AGENTS.md

This file provides guidance to Codex when working with this repository.

## Overview

**asv-hal** is a .NET hardware abstraction library targeting `net10.0`. Source code lives under `src/`, the solution file is `src/Asv.Hal.slnx`, and shared build/version metadata lives in `src/Directory.Build.props`.

## Build Commands

```powershell
dotnet restore src/Asv.Hal.slnx
dotnet build src/Asv.Hal.slnx -c Release --no-restore
dotnet test src/Asv.Hal.slnx --no-restore
```

## Version Management

Package and dependency versions are centralized in `src/Directory.Build.props`:

```xml
<ProductVersion>1.1.0-dev.3</ProductVersion>
<TargetFramework>net10.0</TargetFramework>
<AsvCommonVersion>3.6.0-dev.23</AsvCommonVersion>
```

`Version`, `PackageVersion`, and `FileVersion` inherit from `ProductVersion`. Do not set package versions per-project unless an explicit task requires it.

Release workflows compare Git tags with `ProductVersion`; keep tags and `src/Directory.Build.props` synchronized.

### Dev Feed Dependency Updates

When asked to update `Asv.Common` or shared ASV dependencies for the dev feed:

1. Check the working tree first and keep the change scoped to version metadata unless the task explicitly asks for more.
2. Update `AsvCommonVersion` in `src/Directory.Build.props` to the requested version. This property is used for the shared ASV package family referenced by this repository, including `Asv.Common`, `Asv.IO`, and `Asv.Modeling`.
3. Increment the numeric `ProductVersion` dev suffix by one, for example `1.1.0-dev.3` -> `1.1.0-dev.4`.
4. Confirm the dev release workflow tag pattern in `.github/workflows/release-debug-version.yml`; the tag must be `v<ProductVersion>`, for example `v1.1.0-dev.4`, because the workflow compares the tag without `v` to `ProductVersion`.
5. Run focused validation:

```powershell
dotnet restore src/Asv.Hal.slnx
dotnet build src/Asv.Hal.slnx -c Release --no-restore
dotnet test src/Asv.Hal.slnx --no-restore
```

If local restore fails with `401 Unauthorized` from GitHub Packages for private ASV packages, report that validation is blocked by local NuGet credentials. The GitHub release workflow uses repository secrets for package restore and publish.

6. Commit, tag, and push:

```powershell
git add src/Directory.Build.props
git commit -m "Bump Asv.Common to <version>"
git tag -a v<ProductVersion> -m "v<ProductVersion>"
git push origin main
git push origin v<ProductVersion>
```
