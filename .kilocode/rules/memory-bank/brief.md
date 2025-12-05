SemanticSlicer is a lightweight .NET library and tooling suite for recursively splitting large documents into semantically meaningful, LLM-ready chunks. It preserves structure using configurable separators (text, Markdown, HTML), counts tokens via tiktoken encoders (cl100k_base, o200k), and supports optional HTML stripping, metadata passthrough, and chunk headers.

Key features:
- Recursive, centermost-aware splitting with prefix/suffix/remove behaviors
- Token-aware chunk sizing with min-percentage safeguards and stable ordering
- CLI for one-off or daemon mode with stdin/pipe processing
- Minimal Web API service for POST /slice chunking
- NuGet package for direct library use across Windows, macOS, Linux

Technologies:
- .NET (netstandard2.1, .NET 8 tooling), C#
- HtmlAgilityPack, Tiktoken (token encoders)

Significance:
- Produces high-quality chunks for embeddings and RAG pipelines, reducing token waste and improving retrieval relevance while offering flexible deployment: library, CLI, daemon, and REST service.