# Architecture

## System Overview
SemanticSlicer provides three operation modes over a shared core:
- Library: direct use of the slicer class in .NET apps [SemanticSlicer.Slicer()](SemanticSlicer/Slicer.cs:37).
- CLI: command-line interface wrapping the slicer [Program.Main()](SemanticSlicer.Cli/Program.cs:1).
- Service: minimal REST API for in-memory slicing [WebApplication.MapPost()](SemanticSlicer.Service/Program.cs:12).

Core responsibilities:
- Normalize and prepare input
- Token-aware chunk sizing
- Recursive splitting via separators
- Stable chunk ordering and optional metadata/header passthrough

## Components
- Core
  - Slicer [SemanticSlicer.Slicer](SemanticSlicer/Slicer.cs:15)
    - GetDocumentChunks [SemanticSlicer.Slicer.GetDocumentChunks()](SemanticSlicer/Slicer.cs:130) - Standard preprocessing + splitting
    - SplitDocumentChunksRaw [SemanticSlicer.Slicer.SplitDocumentChunksRaw()](SemanticSlicer/Slicer.cs:122) - Advanced API: splitting without preprocessing
    - PrepareContentForChunking [SemanticSlicer.Slicer.PrepareContentForChunking()](SemanticSlicer/Slicer.cs:118) - Separate preprocessing step
    - CountTokens [SemanticSlicer.Slicer.CountTokens()](SemanticSlicer/Slicer.cs:70) - Public token counting
    - SplitDocumentChunks [SemanticSlicer.Slicer.SplitDocumentChunks()](SemanticSlicer/Slicer.cs:395) - Internal recursive splitting engine (private)
    - SplitChunkBySeparatorMatch [SemanticSlicer.Slicer.SplitChunkBySeparatorMatch()](SemanticSlicer/Slicer.cs:480) - Internal (private)
    - GetCentermostMatch [SemanticSlicer.Slicer.GetCentermostMatch()](SemanticSlicer/Slicer.cs:547) - Internal (private)
    - DoTextSplit [SemanticSlicer.Slicer.DoTextSplit()](SemanticSlicer/Slicer.cs:594) - Internal (private)
    - NormalizeLineEndings [SemanticSlicer.Slicer.NormalizeLineEndings()](SemanticSlicer/Slicer.cs:627) - Internal wrapper (private)
    - CollapseWhitespace [SemanticSlicer.Slicer.CollapseWhitespace()](SemanticSlicer/Slicer.cs:310) - Internal wrapper (private)
    - RemoveNonBodyContent [SemanticSlicer.Slicer.RemoveNonBodyContent()](SemanticSlicer/Slicer.cs:248) - Public HTML-to-text
    - ExtractTitle [SemanticSlicer.Slicer.ExtractTitle()](SemanticSlicer/Slicer.cs:363) - Public title extraction
    - Encoder selection [SemanticSlicer.Slicer.GetEncoder()](SemanticSlicer/Slicer.cs:50) - Internal (private)
  - TextUtilities [SemanticSlicer.TextUtilities](SemanticSlicer/TextUtilities.cs) - Public static preprocessing helpers
    - NormalizeLineEndings [SemanticSlicer.TextUtilities.NormalizeLineEndings()](SemanticSlicer/TextUtilities.cs:24)
    - CollapseWhitespace [SemanticSlicer.TextUtilities.CollapseWhitespace()](SemanticSlicer/TextUtilities.cs:37)
  - Options [SemanticSlicer.SlicerOptions](SemanticSlicer/SlicerOptions.cs:8)
    - Defaults: MaxChunkTokenCount=1000, MinChunkPercentage=10, Encoding=Cl100K, Separators=Text [SemanticSlicer.SlicerOptions](SemanticSlicer/SlicerOptions.cs:13)
    - Toggle: StripHtml [SemanticSlicer.SlicerOptions](SemanticSlicer/SlicerOptions.cs:31)
  - Separators [SemanticSlicer.Separators](SemanticSlicer/Separators.cs:10)
    - Text/Markdown/Html ordered regex lists

- Models
  - DocumentChunk [SemanticSlicer.Models.DocumentChunk](SemanticSlicer/Models/DocumentChunk.cs:1)
  - Separator [SemanticSlicer.Models.Separator](SemanticSlicer/Models/Separator.cs:1)
  - Encoding enum [SemanticSlicer.Encoding](SemanticSlicer/Encoding.cs:1)

- CLI
  - Entry and mode parsing [SemanticSlicer.Cli.Program](SemanticSlicer.Cli/Program.cs:1)

- Service
  - DI singleton Slicer [WebApplicationBuilder.Services.AddSingleton](SemanticSlicer.Service/Program.cs:7)
  - API contract [SemanticSlicer.Service.Models.SliceRequest](SemanticSlicer.Service/Models/SliceRequest.cs:1)
  - Endpoint [WebApplication.MapPost](SemanticSlicer.Service/Program.cs:12)

## Data Flow

Sequence for GetDocumentChunks:
1. Input
   - Content, optional metadata, optional chunkHeader
2. Preprocessing
   - Normalize line endings [SemanticSlicer.Slicer.NormalizeLineEndings()](SemanticSlicer/Slicer.cs:466)
   - Optional HTML strip to body text and title [SemanticSlicer.Slicer.RemoveNonBodyContent()](SemanticSlicer/Slicer.cs:127)
   - Collapse whitespace [SemanticSlicer.Slicer.CollapseWhitespace()](SemanticSlicer/Slicer.cs:189)
3. Header tokens check
   - Validate header token count $<=$ MaxChunkTokenCount [SemanticSlicer.Slicer.GetDocumentChunks()](SemanticSlicer/Slicer.cs:81)
4. Initial chunk
   - Create single chunk with effective token count (header+content) [SemanticSlicer.Slicer.GetDocumentChunks()](SemanticSlicer/Slicer.cs:97)
5. Recursive splitting
   - While chunk exceeds MaxChunkTokenCount:
     - Find centermost separator match from current Separators list
     - Split by separator behavior (Prefix/Suffix/Remove) [SemanticSlicer.Slicer.DoTextSplit()](SemanticSlicer/Slicer.cs:433)
     - Recompute token counts including header
     - Guard: MinChunkPercentage threshold [SemanticSlicer.Slicer.IsSplitBelowThreshold()](SemanticSlicer/Slicer.cs:333)
     - Recurse until all chunks fit
6. Ordering
   - Assign Index sequentially [SemanticSlicer.Slicer.GetDocumentChunks()](SemanticSlicer/Slicer.cs:109)
7. Output
   - List<DocumentChunk> with content, token count, optional metadata, index

## Key Decisions
- Token counting via Tiktoken encoders (Cl100K, O200K) [SemanticSlicer.Slicer.GetEncoder()](SemanticSlicer/Slicer.cs:50).
- Centermost-aware splitting to maintain semantic locality [SemanticSlicer.Slicer.GetCentermostMatch()](SemanticSlicer/Slicer.cs:399).
- Separator behavior types to control boundary inclusion [SemanticSlicer.SeparatorBehavior](SemanticSlicer/SeparatorBehavior.cs:1).
- MinChunkPercentage to avoid pathological tiny chunks [SemanticSlicer.Slicer.IsSplitBelowThreshold()](SemanticSlicer/Slicer.cs:333).
- OverlapPercentage (0–100) is clamped; header tokens reduce overlap budget so MaxChunkTokenCount is always respected [SemanticSlicer.SlicerOptions](SemanticSlicer/SlicerOptions.cs:8).
- Optional HTML stripping focuses on inner text and removes scripts/styles, prepends title when present [SemanticSlicer.Slicer.RemoveNonBodyContent()](SemanticSlicer/Slicer.cs:127).

## Interactions
- Library consumers construct Slicer with SlicerOptions and call GetDocumentChunks [SemanticSlicer.Slicer()](SemanticSlicer/Slicer.cs:37).
- CLI wraps library: file input, stdin, daemon mode.
- Service exposes POST /slice receiving content, metadata, chunkHeader; returns chunks [WebApplication.MapPost()](SemanticSlicer.Service/Program.cs:12).

## Constraints
- Requires .NET 8 for tooling; core targets netstandard2.1 for broad compatibility.
- HTML parsing via HtmlAgilityPack; behavior limited to text extraction and simple layout hints.
- Token limits bounded by selected encoder; header tokens count toward limits.

## Future Extensibility
- Custom separator lists per content type or domain.
- Additional encoders or dynamic model token limits.
- More service endpoints (health, config) and auth if needed.