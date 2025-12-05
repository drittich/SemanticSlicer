# Tech

## Technologies
- .NET: core library targets netstandard2.1; tooling uses .NET 8
- C# 10+ across projects
- HtmlAgilityPack for HTML parsing [SemanticSlicer.Slicer.RemoveNonBodyContent()](SemanticSlicer/Slicer.cs:127)
- Tiktoken encoders via Tiktoken.Encodings [SemanticSlicer.Slicer.GetEncoder()](SemanticSlicer/Slicer.cs:50)

## Dependencies
- HtmlAgilityPack (nuget)
- Tiktoken.Encodings (nuget)
- ASP.NET Core Minimal API for service [WebApplication.MapPost()](SemanticSlicer.Service/Program.cs:12)

## Build and Publish
- Library: packaged as NuGet `drittich.SemanticSlicer` (install from README)
- CLI publish:
  - Language: bash
  - Command:
  - dotnet publish SemanticSlicer.Cli/SemanticSlicer.Cli.csproj -c Release -o ./cli
- Service publish:
  - Language: bash
  - Command:
  - dotnet publish SemanticSlicer.Service/SemanticSlicer.Service.csproj -c Release -o ./publish
- Prebuilt binaries in Releases (Windows/macOS/Linux) [README.md](README.md:52)

## Runtime Modes
- Library: construct [SemanticSlicer.Slicer()](SemanticSlicer/Slicer.cs:37)
- CLI: run once or daemon; optional named pipe (Unix) [SemanticSlicer.Cli.Program](SemanticSlicer.Cli/Program.cs:1)
- Service: POST /slice with content, metadata, chunkHeader [SemanticSlicer.Service/Program.cs](SemanticSlicer.Service/Program.cs:12)

## Configuration
- Options: [SemanticSlicer.SlicerOptions](SemanticSlicer/SlicerOptions.cs:8)
  - MaxChunkTokenCount default 1000 [SemanticSlicer.SlicerOptions](SemanticSlicer/SlicerOptions.cs:13)
  - MinChunkPercentage default 10 [SemanticSlicer.SlicerOptions](SemanticSlicerOptions.cs:18)
  - Encoding default Cl100K [SemanticSlicer.SlicerOptions](SemanticSlicerOptions.cs:23)
  - Separators default Text [SemanticSlicer.SlicerOptions](SemanticSlicerOptions.cs:28)
  - StripHtml default false [SemanticSlicer.SlicerOptions](SemanticSlicerOptions.cs:33)
- Separators lists:
  - Text/Markdown/Html [SemanticSlicer.Separators](SemanticSlicer/Separators.cs:10)

## Constraints
- Token limits bound by selected encoder; headers included in counts [SemanticSlicer.Slicer.GetDocumentChunks()](SemanticSlicer/Slicer.cs:81)
- Service keeps Slicer as singleton for in-memory performance [WebApplicationBuilder.Services.AddSingleton](SemanticSlicer.Service/Program.cs:7)
- Center-aware splitting relies on regex matches and behaviors [SemanticSlicer.Slicer.SplitChunkBySeparatorMatch()](SemanticSlicer/Slicer.cs:360)

## Operations
- Sample usage library [README.md](README.md:212)
- Metadata passthrough [README.md](README.md:264)
- Chunk headers [README.md](README.md:279)

## Platform Support
- Windows/macOS/Linux self-contained CLI builds [README.md](README.md:55)