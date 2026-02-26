# SemanticSlicer - Agent Memory

This file contains project context for AI agents working on SemanticSlicer. It consolidates the memory bank content for agent reference.

## Brief

SemanticSlicer is a lightweight .NET library and tooling suite for recursively splitting large documents into semantically meaningful, LLM-ready chunks. It preserves structure using configurable separators (text, Markdown, HTML), counts tokens via tiktoken encoders (cl100k_base, o200k), and supports optional HTML stripping, metadata passthrough, and chunk headers.

### Key Features
- Recursive, centermost-aware splitting with prefix/suffix/remove behaviors
- Token-aware chunk sizing with min-percentage safeguards and stable ordering
- CLI for one-off or daemon mode with stdin/pipe processing
- Minimal Web API service for POST /slice chunking
- NuGet package for direct library use across Windows, macOS, Linux

### Technologies
- .NET (netstandard2.1, .NET 8 tooling), C#
- HtmlAgilityPack, Tiktoken (token encoders)

### Significance
- Produces high-quality chunks for embeddings and RAG pipelines, reducing token waste and improving retrieval relevance while offering flexible deployment: library, CLI, daemon, and REST service.

---

## Product

### Why This Project Exists
SemanticSlicer exists to split large documents into semantically meaningful, LLM-ready chunks to optimize embeddings and retrieval. It preserves document structure (text, Markdown, HTML), controls chunk size via token-aware limits, and supports multiple deployment/use modes: library (NuGet), CLI (one-shot and daemon), and minimal REST service.

### Problems It Solves
- Excessive token usage in naive chunking leading to poor embeddings and higher costs
- Loss of semantic boundaries causing degraded retrieval relevance
- Need for consistent, repeatable chunking across different content types (text, Markdown, HTML)
- Operational flexibility: single-run CLI, in-memory daemon, and REST service for integration
- Metadata passthrough and optional chunk headers for RAG pipelines

### How It Should Work
- Input content is normalized and optionally HTML-stripped for token efficiency
- Token counting is performed using tiktoken encoders cl100k_base and o200k
- Recursive splitting uses centermost-aware matching across configured separators
- Ensures chunk size <= MaxChunkTokenCount and safeguards with MinChunkPercentage
- Supports overlapping chunks via an OverlapPercentage (0-100) carry-forward of previous chunk tokens
- Stable ordering with Index assigned per chunk for reassembly
- Each chunk includes StartOffset and EndOffset character positions relative to normalized input
- Optional metadata dictionary is preserved on each chunk
- Optional per-chunk header is prepended to content and accounted for in tokens

### Usage Modes
- **Library (NuGet)**: `drittich.SemanticSlicer` for direct integration in .NET apps
- **CLI**: One-off execution for files or piped input; daemon mode keeps slicer in memory
- **Service**: Minimal Web API with POST /slice accepting content, metadata, and chunkHeader

### Document Types and Separators
- **Text**: Sentence/end punctuation and whitespace-aware splitting
- **Markdown**: Headers prioritized to preserve structure
- **HTML**: Element-aware prefixes to align splits with block boundaries

### Success Criteria
- Chunks consistently within token limits with minimal semantic breakage
- Reduced token consumption relative to naive splitting
- Reliable operation across library, CLI, daemon, and REST service modes
- Clear, minimal API surface with sane defaults and optional extensibility via custom separators

---

## Architecture

### System Overview
SemanticSlicer provides three operation modes over a shared core:
- **Library**: Direct use of the slicer class in .NET apps
- **CLI**: Command-line interface wrapping the slicer
- **Service**: Minimal REST API for in-memory slicing

### Core Responsibilities
- Normalize and prepare input
- Token-aware chunk sizing
- Recursive splitting via separators
- Stable chunk ordering and optional metadata/header passthrough

### Components

#### Core
- **Slicer** (`SemanticSlicer/Slicer.cs`)
  - `GetDocumentChunks()` - Standard preprocessing + splitting
  - `SplitDocumentChunksRaw()` - Advanced API: splitting without preprocessing
  - `PrepareContentForChunking()` - Separate preprocessing step
  - `CountTokens()` - Public token counting
  - `RemoveNonBodyContent()` - Public HTML-to-text
  - `ExtractTitle()` - Public title extraction
- **TextUtilities** (`SemanticSlicer/TextUtilities.cs`) - Public static preprocessing helpers
  - `NormalizeLineEndings()`
  - `CollapseWhitespace()`
- **Options** (`SemanticSlicer/SlicerOptions.cs`)
  - Defaults: MaxChunkTokenCount=1000, MinChunkPercentage=10, Encoding=Cl100K, Separators=Text
  - Toggle: StripHtml
- **Separators** (`SemanticSlicer/Separators.cs`)
  - Text/Markdown/Html ordered regex lists

#### Models
- `DocumentChunk` - Output chunk model with content, tokens, metadata, index, offsets
- `Separator` - Separator definition with regex and behavior
- `Encoding` enum - Token encoding options (Cl100K, O200K)

#### CLI
- Entry and mode parsing (`SemanticSlicer.Cli/Program.cs`)

#### Service
- DI singleton Slicer
- API contract (`SemanticSlicer.Service/Models/SliceRequest.cs`)
- Endpoint POST /slice

### Data Flow

Sequence for GetDocumentChunks:
1. **Input**: Content, optional metadata, optional chunkHeader
2. **Preprocessing**: Normalize line endings, optional HTML strip, collapse whitespace
3. **Header tokens check**: Validate header token count <= MaxChunkTokenCount
4. **Initial chunk**: Create single chunk with effective token count (header+content)
5. **Recursive splitting**: While chunk exceeds MaxChunkTokenCount, find centermost separator, split by behavior, recompute tokens, guard with MinChunkPercentage threshold
6. **Ordering**: Assign Index sequentially
7. **Output**: List<DocumentChunk> with content, token count, optional metadata, index

### Key Decisions
- Token counting via Tiktoken encoders (Cl100K, O200K)
- Centermost-aware splitting to maintain semantic locality
- Separator behavior types to control boundary inclusion
- MinChunkPercentage to avoid pathological tiny chunks
- OverlapPercentage (0-100) is clamped; header tokens reduce overlap budget
- Optional HTML stripping focuses on inner text and removes scripts/styles

### Constraints
- Requires .NET 8 for tooling; core targets netstandard2.1 for broad compatibility
- HTML parsing via HtmlAgilityPack; behavior limited to text extraction
- Token limits bounded by selected encoder; header tokens count toward limits

---

## Tech

### Technologies
- .NET: core library targets netstandard2.1; tooling uses .NET 8
- C# 10+ across projects
- HtmlAgilityPack for HTML parsing
- Tiktoken encoders via Tiktoken.Encodings

### Dependencies
- HtmlAgilityPack (nuget)
- Tiktoken.Encodings (nuget)
- ASP.NET Core Minimal API for service

### Build and Publish
- **CLI publish**: `dotnet publish SemanticSlicer.Cli/SemanticSlicer.Cli.csproj -c Release -o ./cli`
- **Service publish**: `dotnet publish SemanticSlicer.Service/SemanticSlicer.Service.csproj -c Release -o ./publish`
- Prebuilt binaries in Releases (Windows/macOS/Linux)

### Runtime Modes
- **Library**: Construct Slicer with SlicerOptions and call GetDocumentChunks
- **CLI**: Run once or daemon; support overlap via `--overlap` (0-100)
- **Service**: POST /slice with content, metadata, chunkHeader, optional `overlapPercentage` (0-100)

### Configuration
- **SlicerOptions**:
  - MaxChunkTokenCount default 1000
  - MinChunkPercentage default 10
  - Encoding default Cl100K
  - Separators default Text
  - StripHtml default false
  - OverlapPercentage in [0,100] and clamped

### Platform Support
- Windows/macOS/Linux self-contained CLI builds

---

## Context

### Current Focus
- Advanced API expansion complete: exposed split engine and preprocessing utilities while keeping internal methods private
- Align documentation with repository sources

### Recent Changes
- **Advanced API Expansion (v1.1.0 proposed)**:
  - Added public `Slicer.SplitDocumentChunksRaw()` for custom preprocessing pipelines
  - Added public `TextUtilities` static class with `NormalizeLineEndings` and `CollapseWhitespace`
  - Added public `Slicer.CountTokens()` for token counting
  - Added public `Slicer.PrepareContentForChunking()` for separate preprocessing
  - Internal split methods remain private by design
  - Zero breaking changes; fully backward compatible
  - Comprehensive test coverage (34 tests passing)

### Next Steps
- Validate Memory Bank alignment against README.md
- Request maintainer verification of updated documents

### Assumptions
- .NET 8 tooling; core library targets netstandard2.1
- Token counting via Tiktoken encoders
- Defaults: MaxChunkTokenCount=1000, MinChunkPercentage=10, Encoding=Cl100K, Separators=Text
- OverlapPercentage is clamped to [0,100]; header tokens reduce overlap budget

### Risks
- Separator regex ordering impacts split quality
- Chunk headers contribute tokens and are validated chunkHeaderTokens <= MaxChunkTokenCount

---

## Tasks

### Publish CLI

**Goal:** Build and publish the CLI to a local folder for cross-platform execution.

**Steps:**
1. Build publish output:
   ```
   dotnet publish SemanticSlicer.Cli/SemanticSlicer.Cli.csproj -c Release -o ./cli
   ```
   Note: You can use the `--self-contained` and `-r <RID>` options for platform-specific binaries if needed.
2. Run once:
   ```
   dotnet ./cli/SemanticSlicer.Cli.dll --overlap 30 MyDocument.txt
   ```
3. Pipe input:
   ```
   cat MyDocument.txt | dotnet ./cli/SemanticSlicer.Cli.dll --overlap 20
   ```
4. Daemon:
   ```
   dotnet ./cli/SemanticSlicer.Cli.dll daemon --overlap 25
   ```
5. Daemon with named pipe (Unix):
   ```
   dotnet ./cli/SemanticSlicer.Cli.dll daemon --pipe slicerpipe --overlap 25
   ```

**Notes:**
- Prebuilt binaries are available in Releases

### Publish Service

**Goal:** Publish and run the minimal API service to keep Slicer in memory and expose POST /slice.

**Steps:**
1. Publish:
   ```
   dotnet publish SemanticSlicer.Service/SemanticSlicer.Service.csproj -c Release -o ./publish
   ```
2. Linux systemd unit file:
   ```ini
   [Unit]
   Description=Semantic Slicer Service
   After=network.target

   [Service]
   Type=simple
   WorkingDirectory=/opt/semanticslicer
   ExecStart=/usr/bin/dotnet /opt/semanticslicer/SemanticSlicer.Service.dll
   Restart=always

   [Install]
   WantedBy=multi-user.target
   ```
3. Enable/start on Linux:
   ```
   sudo systemctl enable semanticslicer
   sudo systemctl start semanticslicer
   ```
4. Windows service creation:
   ```
   sc create SemanticSlicer binPath= "%ProgramFiles%\dotnet\dotnet.exe" "C:\SemanticSlicer\SemanticSlicer.Service.dll"
   sc start SemanticSlicer
   ```
5. Test endpoint:
   ```
   curl -X POST http://localhost:5000/slice -H "Content-Type: application/json" -d '{"content":"Hello world","overlapPercentage":30}'
   ```

### Use Library via NuGet

**Goal:** Integrate the library directly into a .NET app.

**Steps:**
1. Install package:
   ```
   dotnet add package drittich.SemanticSlicer
   ```
2. Basic usage:
   ```csharp
   var slicer = new SemanticSlicer.Slicer();
   var text = File.ReadAllText("MyDocument.txt");
   var chunks = slicer.GetDocumentChunks(text);
   ```
3. Markdown usage:
   ```csharp
   var options = new SemanticSlicer.SlicerOptions { MaxChunkTokenCount = 600, Separators = SemanticSlicer.Separators.Markdown };
   var slicer = new SemanticSlicer.Slicer(options);
   var chunks = slicer.GetDocumentChunks(File.ReadAllText("MyDocument.md"));
   ```
4. HTML with stripping:
   ```csharp
   var options = new SemanticSlicer.SlicerOptions { Separators = SemanticSlicer.Separators.Html, StripHtml = true };
   var slicer = new SemanticSlicer.Slicer(options);
   var chunks = slicer.GetDocumentChunks(File.ReadAllText("MyDocument.html"));
   ```
5. Metadata passthrough:
   ```csharp
   var meta = new Dictionary<string, object?> { ["Id"] = 123, ["FileName"] = "MyDocument.txt" };
   var chunks = slicer.GetDocumentChunks(File.ReadAllText("MyDocument.txt"), meta);
   ```
6. Chunk header:
   ```csharp
   var header = $"FileName: MyDocument.txt";
   var chunks = slicer.GetDocumentChunks(File.ReadAllText("MyDocument.txt"), null, header);
   ```

**Considerations:**
- Header token count must be <= MaxChunkTokenCount
- Defaults: MaxChunkTokenCount=1000, MinChunkPercentage=10, Encoding=Cl100K, Separators=Text
