# OsmToolkit

.NET 8 library for reading, writing, searching, and analysing OpenStreetMap data.
Originally a group project (HiØ), continued by me. Published as a NuGet package.

## Design principles — respect these
- Interface-driven. Everything is registered via `AddOsmToolkit()` in `Setup/ServiceCollectionExtensions.cs`.
- `ILogger<T>` with `NullLogger` fallback. Use `[LoggerMessage]` partials, not string interpolation.
- Fail fast with standard .NET exceptions. Don't return null on failure.
- Async for all IO, always with `CancellationToken`.
- XML doc on all public members.

## Critical: no breaking changes
This is a published package. `IOsmFinder` is marked `[Obsolete]` but must remain.
New methods go into new, small interfaces, not into existing ones.

## Tests
MSTest. Naming: `Method_Scenario_ExpectedResult`.
Arrange/Act/Assert. One thing per test. New features must have tests.

## Commands
dotnet build
dotnet test

## Agent skills

### Issue tracker

Issues and specs live in GitHub Issues (Skjobbe/osmtoolkit), using the `gh` CLI. See `docs/agents/issue-tracker.md`.

### Domain docs

Single-context: `CONTEXT.md` at repo root, ADRs in `docs/decisions/`. See `docs/agents/domain.md`.

## Language
From now on, all code, XML doc, commit messages, and documents in this repo are written in English. This includes `CONTEXT.md`, ADRs, and GitHub issues.
The existing Norwegian ADRs in `docs/decisions/` (01-03) are not retroactively translated by this rule — that's tracked as separate work.
Conversation with the agent can happen in Norwegian.
