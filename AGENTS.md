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

## Comment and Documentation Language

- Write all code comments in English.
- Write all XML documentation, Markdown documentation, README content, and other developer-facing documentation in English.
- Do not use Russian or mixed-language comments or documentation.
- Keep terminology consistent across code, comments, and documentation.
- Use clear English names for types, members, variables, files, modules, and public APIs.

## Comment Quality

- Prefer self-explanatory code over excessive comments.
- Add comments only when they explain intent, constraints, assumptions, tradeoffs, or non-obvious behavior.
- Do not add comments that only restate what the code already makes obvious.
- Keep comments concise, accurate, and aligned with the current implementation.
- Update or remove comments when the code changes so documentation never becomes misleading.

## Architecture and Design

- Keep the architecture clean, modular, and easy to maintain.
- Follow SOLID principles in design and implementation.
- Give each class, service, and module a single, well-defined responsibility.
- Prefer composition over inheritance unless inheritance is clearly justified.
- Minimize coupling and keep related behavior cohesive.
- Separate domain logic from UI, infrastructure, persistence, and framework-specific concerns.
- Depend on abstractions at system boundaries when this improves testability, extensibility, or clarity.
- Keep public APIs explicit, stable, and easy to understand.
- Eliminate duplicated logic through extraction or refactoring instead of copying behavior.
- Avoid god objects, hidden side effects, and unclear ownership of responsibilities.

---
apply: always
---

Behavioral guidelines to reduce common LLM coding mistakes. Merge with project-specific instructions as needed.

**Tradeoff:** These guidelines bias toward caution over speed. For trivial tasks, use judgment.

## 1. Think Before Coding

**Don't assume. Don't hide confusion. Surface tradeoffs.**

Before implementing:
- State your assumptions explicitly. If uncertain, ask.
- If multiple interpretations exist, present them - don't pick silently.
- If a simpler approach exists, say so. Push back when warranted.
- If something is unclear, stop. Name what's confusing. Ask.

## 2. Simplicity First

**Minimum code that solves the problem. Nothing speculative.**

- No features beyond what was asked.
- No abstractions for single-use code.
- No "flexibility" or "configurability" that wasn't requested.
- No error handling for impossible scenarios.
- If you write 200 lines and it could be 50, rewrite it.

Ask yourself: "Would a senior engineer say this is overcomplicated?" If yes, simplify.

## 3. Surgical Changes

**Touch only what you must. Clean up only your own mess.**

When editing existing code:
- Don't "improve" adjacent code, comments, or formatting.
- Don't refactor things that aren't broken.
- Match existing style, even if you'd do it differently.
- If you notice unrelated dead code, mention it - don't delete it.

When your changes create orphans:
- Remove imports/variables/functions that YOUR changes made unused.
- Don't remove pre-existing dead code unless asked.

The test: Every changed line should trace directly to the user's request.

## 4. Goal-Driven Execution

**Define success criteria. Loop until verified.**

Transform tasks into verifiable goals:
- "Add validation" -> "Write tests for invalid inputs, then make them pass"
- "Fix the bug" -> "Write a test that reproduces it, then make it pass"
- "Refactor X" -> "Ensure tests pass before and after"

For multi-step tasks, state a brief plan:

```text
1. [Step] -> verify: [check]
2. [Step] -> verify: [check]
3. [Step] -> verify: [check]
```

Strong success criteria let you loop independently. Weak criteria ("make it work") require constant clarification.

---

**These guidelines are working if:** fewer unnecessary changes in diffs, fewer rewrites due to overcomplication, and clarifying questions come before implementation rather than after mistakes.

## Version Management

Package and dependency versions are centralized in `src/Directory.Build.props`:

```xml
<ProductVersion>1.1.0-dev.4</ProductVersion>
<TargetFramework>net10.0</TargetFramework>
<AsvCommonVersion>3.6.0-dev.25</AsvCommonVersion>
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
