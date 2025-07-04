# 🧠✂️ SemanticSlicer

[![.NET 8 - Build](https://github.com/drittich/SemanticSlicer/actions/workflows/build.yml/badge.svg)](https://github.com/drittich/SemanticSlicer/actions/workflows/build.yml)
[![.NET 8 - Tests](https://github.com/drittich/SemanticSlicer/actions/workflows/tests.yml/badge.svg)](https://github.com/drittich/SemanticSlicer/actions/workflows/tests.yml)



Smart, recursive text slicing for LLM-ready documents.

**SemanticSlicer** is a lightweight C# library that **recursively splits text** into meaningful chunks—preserving semantic boundaries (sentences, headings, HTML tags) and ideal for **embedding generation** (OpenAI, Azure OpenAI, LangChain, etc.).


GitHub: [https://github.com/drittich/SemanticSlicer](https://github.com/drittich/SemanticSlicer)

## Table of Contents

- [Overview](#overview)
- [Installation](#installation)
- [Sample Usage](#sample-usage)
- [Chunk Order](#chunk-order)
- [Additional Metadata](#additional-metadata)
- [Adding Headers to Chunks](#adding-headers-to-chunks)
- [License](#license)
- [Contact](#contact)

## Overview

This library accepts text and will break it into smaller chunks, typically useful for when creating [LLM AI embeddings](https://learn.microsoft.com/en-us/semantic-kernel/memories/embeddings).

## Installation

The package name is `drittich.SemanticSlicer`. You can install this from Nuget via the command line:
```ps
dotnet add package drittich.SemanticSlicer
```

or from the Package Manager Console:
```ps
NuGet\Install-Package drittich.SemanticSlicer
```

## Sample Usage

Simple text document:

```cs
// The default options uses text separators, a max chunk size of 1,000, and 
// cl100k_base encoding to count tokens.
var slicer = new Slicer();
var text = File.ReadAllText("MyDocument.txt");
var documentChunks = slicer.GetDocumentChunks(text);
```

Markdown document:

```cs
// Let's use Markdown separators and reduce the chunk size
var options = new SlicerOptions { MaxChunkTokenCount = 600, Separators = Separators.Markdown };
var slicer = new Slicer(options);
var text = File.ReadAllText("MyDocument.md");
var documentChunks = slicer.GetDocumentChunks(text);
```

HTML document:

```cs
var options = new SlicerOptions { Separators = Separators.Html };
var slicer = new Slicer(options);
var text = File.ReadAllText("MyDocument.html");
var documentChunks = slicer.GetDocumentChunks(text);
```

Removing HTML tags:

For any content you can choose to remove HTML tags from the chunks to minimize the number of tokens. The inner text is preserved, and if there is a `<Title>` tag the title will be pre-pended to the result:

```cs
// Let's remove the HTML tags as they just consume a lot of tokens without adding much value
var options = new SlicerOptions { Separators = Separators.Html, StripHtml = true };
var slicer = new Slicer(options);
var text = File.ReadAllText("MyDocument.html");
var documentChunks = slicer.GetDocumentChunks(text);
```

Custom separators:

You can pass in your own list if of separators if you wish, e.g., if you wish to add support for other documents.

## Chunk Order

Chunks will be returned in the order they were found in the document, and contain an Index property you can use to put them back in order if necessary.

## Additional Metadata

You can pass any additional metadata you wish in as a dictionary, and it will be returned with each document chunk, so it's easy to persist. 
You might use the metadata to store the document id, title or last modified date.

```cs
var slicer = new Slicer();
var text = File.ReadAllText("MyDocument.txt");
var metadata = new Dictionary<string, object?>();
metadata["Id"] = 123;
metadata["FileName"] = "MyDocument.txt";
var documentChunks = slicer.GetDocumentChunks(text, metadata);
// All chunks returned will have a Metadata property with the data you passed in.
```

## Adding Headers to Chunks

If you wish you can pass a header to be included at the top of each chunk. Example use cases are to include the document title or tags as 
part of the chunk content to help maintain context.

```cs
var slicer = new Slicer();
var fileName = "MyDocument.txt";
var text = File.ReadAllText(fileName);
var header = $"FileName: {fileName}";
var documentChunks = slicer.GetDocumentChunks(text, null, header);
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contact

If you have any questions or feedback, please open an issue on this repository.
