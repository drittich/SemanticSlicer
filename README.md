# SemanticSlicer
SemanticSlicer is a C# library for slicing text data into smaller pieces while attempting to preserve semantic cohesion.

## Table of Contents

- [Overview](#overview)
- [Sample Usage](#sample-usage)
- [Chunk Order](#chunk-order)
- [Additional Metadata](#additional-metadata)
- [License](#license)
- [Contact](#contact)

## Overview

This library accepts text and will break it into smaller chunks, typically useful for when creating [LLM AI embeddings](https://learn.microsoft.com/en-us/semantic-kernel/memories/embeddings).

## Sample Usage

Simple text document:

```cs
// The default options uses text separators, a max chunk size of 1,000, and cl100k_base encoding to count tokens.
var slicer = new Slicer();
var text = File.ReadAllText("MyDocument.txt");
var documentChunks = slicer.GetDocumentChunks(text);
```

Markdown document:

```cs
var options = new SlicerOptions { MaxChunkTokenCount = 600, Separators = Separators.Markdown };
var slicer = new Slicer(options);
var text = File.ReadAllText("MyDocument.md");
var documentChunks = slicer.GetDocumentChunks(text);
```

You can pass in your own separators if you wish, e.g., if you wish to add support for HTML documents.

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
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contact

If you have any questions or feedback, please open an issue on this repository.










