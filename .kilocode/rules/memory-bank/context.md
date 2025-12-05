# Context

## Current Focus
- Establish a complete Memory Bank for SemanticSlicer covering product, architecture, tech, and operational tasks.
- Align documentation with repository sources: [README.md](README.md), [SemanticSlicer.Slicer()](SemanticSlicer/Slicer.cs:37), [WebApplication.MapPost()](SemanticSlicer.Service/Program.cs:12), [SemanticSlicer.Cli.Program](SemanticSlicer.Cli/Program.cs:1).

## Recent Changes
- Added product overview [memory-bank/product.md](.kilocode/rules/memory-bank/product.md).
- Documented architecture with components and data flow [memory-bank/architecture.md](.kilocode/rules/memory-bank/architecture.md).
- Captured technologies, dependencies, build/publish steps [memory-bank/tech.md](.kilocode/rules/memory-bank/tech.md).

## Next Steps
- Create repeatable task entries for publishing CLI/service and using NuGet [memory-bank/tasks.md](.kilocode/rules/memory-bank/tasks.md).
- Validate Memory Bank alignment against [README.md](README.md) and correct any discrepancies.
- Request verification of product and architecture documents from the maintainer.

## Assumptions
- .NET 8 tooling; core library targets netstandard2.1 [SemanticSlicer.SemanticSlicer.csproj](SemanticSlicer/SemanticSlicer.csproj:1).
- Token counting via Tiktoken encoders [SemanticSlicer.Slicer.GetEncoder()](SemanticSlicer/Slicer.cs:50).
- Defaults: MaxChunkTokenCount=1000, MinChunkPercentage=10, Encoding=Cl100K, Separators=Text [SemanticSlicer.SlicerOptions](SemanticSlicer/SlicerOptions.cs:13).

## Risks
- Separator regex ordering impacts split quality [SemanticSlicer.Separators](SemanticSlicer/Separators.cs:10).
- Chunk headers contribute tokens and are validated $chunkHeaderTokens \leq MaxChunkTokenCount$ [SemanticSlicer.Slicer.GetDocumentChunks()](SemanticSlicer/Slicer.cs:81).