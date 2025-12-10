# Product

## Why this project exists
SemanticSlicer exists to split large documents into semantically meaningful, LLM-ready chunks to optimize embeddings and retrieval. It preserves document structure (text, Markdown, HTML), controls chunk size via token-aware limits, and supports multiple deployment/use modes: library (NuGet), CLI (one-shot and daemon), and minimal REST service.

References:
- [README.md](README.md)
- [SemanticSlicer.Slicer()](SemanticSlicer/Slicer.cs:37)
- [SemanticSlicer.Service/Program.cs](SemanticSlicer/Service/Program.cs)

## Problems it solves
- Excessive token usage in naive chunking leading to poor embeddings and higher costs.
- Loss of semantic boundaries causing degraded retrieval relevance.
- Need for consistent, repeatable chunking across different content types (text, Markdown, HTML).
- Operational flexibility: single-run CLI, in-memory daemon, and REST service for integration.
- Metadata passthrough and optional chunk headers for RAG pipelines.

## How it should work
- Input content is normalized and optionally HTML-stripped for token efficiency [SemanticSlicer.Slicer.RemoveNonBodyContent()](SemanticSlicer/Slicer.cs:127).
- Token counting is performed using tiktoken encoders $cl100k\_base$ and $o200k$ [SemanticSlicer.Slicer.GetEncoder()](SemanticSlicer/Slicer.cs:50).
- Recursive splitting uses centermost-aware matching across configured separators [SemanticSlicer.Slicer.SplitDocumentChunks()](SemanticSlicer/Slicer.cs:275).
- Ensures chunk size $\\leq$ MaxChunkTokenCount and safeguards with MinChunkPercentage [SemanticSlicer.Slicer.IsSplitBelowThreshold()](SemanticSlicer/Slicer.cs:333).
- Supports overlapping chunks via an $OverlapPercentage$ (0–100) carry-forward of previous chunk tokens, clamped and budgeted alongside header tokens [SemanticSlicer.SlicerOptions](SemanticSlicer/SlicerOptions.cs:8); CLI flag `--overlap` and service field `overlapPercentage` align with this behavior [SemanticSlicer.Cli.Program](SemanticSlicer.Cli/Program.cs:1) [SemanticSlicer.Service/Models/SliceRequest](SemanticSlicer.Service/Models/SliceRequest.cs:1).
- Stable ordering with Index assigned per chunk for reassembly [SemanticSlicer.Slicer.GetDocumentChunks()](SemanticSlicer/Slicer.cs:109).
- Each chunk includes $StartOffset$ and $EndOffset$ character positions relative to normalized input for source alignment [SemanticSlicer.Models.DocumentChunk](SemanticSlicer/Models/DocumentChunk.cs:1).
- Optional metadata dictionary is preserved on each chunk [SemanticSlicer.Slicer.GetDocumentChunks()](SemanticSlicer/Slicer.cs:100).
- Optional per-chunk header is prepended to content and accounted for in tokens; headers also reduce available overlap budget [SemanticSlicer.Slicer.GetDocumentChunks()](SemanticSlicer/Slicer.cs:72).

## Usage modes
- Library (NuGet): `drittich.SemanticSlicer` for direct integration in .NET apps [SemanticSlicer.Slicer()](SemanticSlicer/Slicer.cs:37).
- CLI:
  - One-off execution for files or piped input (cross-platform).
  - Daemon mode keeps slicer in memory; optional named pipe on Unix.
- Service:
  - Minimal Web API with POST /slice accepting content, metadata, and chunkHeader [WebApplication.MapPost()](SemanticSlicer.Service/Program.cs:12).

## User experience goals
- Simple defaults: text separators, MaxChunkTokenCount = 1000, Encoding = cl100k_base [SemanticSlicer.SlicerOptions](SemanticSlicer/SlicerOptions.cs:13).
- Clear control via SlicerOptions: separators, encoding, token limits, StripHtml [SemanticSlicer.SlicerOptions](SemanticSlicer/SlicerOptions.cs:9).
- Deterministic, stable chunk ordering and reproducible results.
- Efficient token usage and minimal waste by collapsing whitespace and stripping extraneous HTML when requested [SemanticSlicer.Slicer.CollapseWhitespace()](SemanticSlicer/Slicer.cs:189).
- Flexible metadata and optional chunk headers for downstream context preservation.
- Easy operational paths: binaries, CLI publish, daemon, and REST service with low setup friction.

## Document types and separators
- Text: sentence/end punctuation and whitespace-aware splitting [SemanticSlicer.Separators.Text](SemanticSlicer/Separators.cs:12).
- Markdown: headers prioritized to preserve structure [SemanticSlicer.Separators.Markdown](SemanticSlicer/Separators.cs:27).
- HTML: element-aware prefixes to align splits with block boundaries [SemanticSlicer.Separators.Html](SemanticSlicer/Separators.cs:47).

## Non-goals
- Full HTML sanitization or layout inference beyond stripping scripts/styles and reading inner text.
- Content understanding or summarization; focus is on robust structural splitting and token control.
- Managing downstream embedding creation or vector store operations.

## Success criteria
- Chunks consistently within token limits with minimal semantic breakage.
- Reduced token consumption relative to naive splitting.
- Reliable operation across library, CLI, daemon, and REST service modes.
- Clear, minimal API surface with sane defaults and optional extensibility via custom separators.