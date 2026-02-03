# Context

## Current Focus
- Advanced API expansion complete: exposed split engine and preprocessing utilities while keeping internal methods private.
- Align documentation with repository sources: [README.md](README.md), [SemanticSlicer.Slicer()](SemanticSlicer/Slicer.cs:37), [WebApplication.MapPost()](SemanticSlicer.Service/Program.cs:12), [SemanticSlicer.Cli.Program](SemanticSlicer.Cli/Program.cs:1).

## Recent Changes
- **Advanced API Expansion (v1.1.0 proposed):**
  - Added public [`SemanticSlicer.Slicer.SplitDocumentChunksRaw()`](SemanticSlicer/Slicer.cs:122) for custom preprocessing pipelines
  - Added public [`SemanticSlicer.TextUtilities`](SemanticSlicer/TextUtilities.cs) static class with `NormalizeLineEndings` and `CollapseWhitespace`
  - Added public [`SemanticSlicer.Slicer.CountTokens()`](SemanticSlicer/Slicer.cs:70) for token counting
  - Added public [`SemanticSlicer.Slicer.PrepareContentForChunking()`](SemanticSlicer/Slicer.cs:118) for separate preprocessing
  - Internal split methods remain private by design
  - Zero breaking changes; fully backward compatible
  - Comprehensive test coverage (34 tests passing)
  - Updated [README.md](README.md) with Advanced Usage section
  - Created architecture plan [plans/public-api-expansion.md](plans/public-api-expansion.md)
  - Created release notes [plans/release-notes-advanced-api.md](plans/release-notes-advanced-api.md)

## Next Steps
- Create repeatable task entries for publishing CLI/service and using NuGet, including CLI `--overlap` and Service `overlapPercentage` examples [memory-bank/tasks.md](.kilocode/rules/memory-bank/tasks.md).
- Validate Memory Bank alignment against [README.md](README.md), specifically:
  - CLI overlap flags in Run once [README.md](README.md:125) and Daemon mode [README.md](README.md:141)
  - Service POST with `overlapPercentage` [README.md](README.md:206)
  - Offsets and metadata sections [README.md](README.md:270), [README.md](README.md:273)
- Request maintainer verification of updated product, architecture, tech, and tasks documents.

## Assumptions
- .NET 8 tooling; core library targets netstandard2.1 [SemanticSlicer.SemanticSlicer.csproj](SemanticSlicer/SemanticSlicer.csproj:1).
- Token counting via Tiktoken encoders [SemanticSlicer.Slicer.GetEncoder()](SemanticSlicer/Slicer.cs:50).
- Defaults: MaxChunkTokenCount=1000, MinChunkPercentage=10, Encoding=Cl100K, Separators=Text [SemanticSlicer.SlicerOptions](SemanticSlicer/SlicerOptions.cs:13).
- OverlapPercentage is clamped to $[0,100]$; header tokens reduce overlap budget and $TokenCount \leq MaxChunkTokenCount$ is always enforced [SemanticSlicer.SlicerOptions](SemanticSlicer/SlicerOptions.cs:8).

## Risks
- Separator regex ordering impacts split quality [SemanticSlicer.Separators](SemanticSlicer/Separators.cs:10).
- Chunk headers contribute tokens and are validated $chunkHeaderTokens \leq MaxChunkTokenCount$ [SemanticSlicer.Slicer.GetDocumentChunks()](SemanticSlicer/Slicer.cs:81).