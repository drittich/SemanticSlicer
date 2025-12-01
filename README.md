# 🧠✂️ SemanticSlicer

[![.NET 8 - Build](https://github.com/drittich/SemanticSlicer/actions/workflows/build.yml/badge.svg)](https://github.com/drittich/SemanticSlicer/actions/workflows/build.yml)
[![.NET 8 - Tests](https://github.com/drittich/SemanticSlicer/actions/workflows/tests.yml/badge.svg)](https://github.com/drittich/SemanticSlicer/actions/workflows/tests.yml)



Smart, recursive text slicing for LLM-ready documents.

**SemanticSlicer** is a lightweight C# application that **recursively splits text** into meaningful chunks—preserving semantic boundaries (sentences, headings, HTML tags) and ideal for **embedding generation** (OpenAI, Azure OpenAI, LangChain, etc.).


GitHub: [https://github.com/drittich/SemanticSlicer](https://github.com/drittich/SemanticSlicer)

## Table of Contents

- [🧠✂️ SemanticSlicer](#️-semanticslicer)
  - [Table of Contents](#table-of-contents)
  - [Overview](#overview)
  - [Installation](#installation)
  - [Download \& Run (no build)](#download--run-no-build)
  - [CLI Usage](#cli-usage)
    - [Run once](#run-once)
    - [Daemon mode](#daemon-mode)
  - [Service Installation](#service-installation)
    - [Linux (systemd)](#linux-systemd)
    - [Windows](#windows)
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

## Download & Run (no build)

Prebuilt binaries are published under GitHub Releases of this repository: https://github.com/drittich/SemanticSlicer/releases

Choose the asset that matches your platform:
- Windows x64: SemanticSlicer.Cli-win-x64.zip
- macOS Intel: SemanticSlicer.Cli-osx-x64.zip
- macOS Apple Silicon: SemanticSlicer.Cli-osx-arm64.zip
- Linux x64: SemanticSlicer.Cli-linux-x64.zip

After downloading:
- Windows:
  - Unzip the file.
  - Open Command Prompt in the unzipped folder.
  - Language: cmd
    Command:
    SemanticSlicer.Cli.exe MyDocument.txt
  - Or pipe input:
    Language: cmd
    Command:
    type MyDocument.txt | SemanticSlicer.Cli.exe

- macOS:
  - Unzip the file.
  - In Terminal, mark the binary executable if needed and run:
    - Intel:
      - Language: bash
        Command:
        chmod +x SemanticSlicer.Cli && ./SemanticSlicer.Cli MyDocument.txt
    - Apple Silicon:
      - Language: bash
        Command:
        chmod +x SemanticSlicer.Cli && ./SemanticSlicer.Cli MyDocument.txt
  - Pipe input:
    - Language: bash
      Command:
      cat MyDocument.txt | ./SemanticSlicer.Cli

- Linux:
  - Unzip the file.
  - Language: bash
    Command:
    chmod +x SemanticSlicer.Cli && ./SemanticSlicer.Cli MyDocument.txt
  - Pipe input:
    - Language: bash
      Command:
      cat MyDocument.txt | ./SemanticSlicer.Cli

Daemon mode (keeps slicer in memory):
- Language: bash
  Command:
  ./SemanticSlicer.Cli daemon
- Named pipe (Linux/macOS):
  - Language: bash
    Command:
    ./SemanticSlicer.Cli daemon --pipe slicerpipe

Notes:
- These builds are self-contained; the .NET runtime is not required.
- If your OS flags the binary (macOS Gatekeeper), you may need to allow it in System Settings → Privacy & Security.

## CLI Usage

Build the command-line tool:

```bash
dotnet publish SemanticSlicer.Cli/SemanticSlicer.Cli.csproj -c Release -o ./cli
```

### Run once

Slice a file and output JSON chunk data:

```bash
dotnet ./cli/SemanticSlicer.Cli.dll MyDocument.txt
```

You can also pipe text in:

```bash
cat MyDocument.txt | dotnet ./cli/SemanticSlicer.Cli.dll
```

### Daemon mode

Keep a slicer in memory and read lines from stdin (or a named pipe):

```bash
dotnet ./cli/SemanticSlicer.Cli.dll daemon
```

Optionally listen on a named pipe:

```bash
dotnet ./cli/SemanticSlicer.Cli.dll daemon --pipe slicerpipe
```

## Service Installation

The repository includes a small Web API (`SemanticSlicer.Service`) that can be
installed as a background service so the slicer stays in memory.

First publish the service:

```bash
dotnet publish SemanticSlicer.Service/SemanticSlicer.Service.csproj -c Release -o ./publish
```

### Linux (systemd)

1. Copy the `./publish` folder to `/opt/semanticslicer` (or a location of your choice).
2. Create `/etc/systemd/system/semanticslicer.service` with:

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

3. Enable and start the service:

```bash
sudo systemctl enable semanticslicer
sudo systemctl start semanticslicer
```

### Windows

1. Publish the service to a folder, e.g. `C:\SemanticSlicer`:

```ps
dotnet publish SemanticSlicer.Service/SemanticSlicer.Service.csproj -c Release -o C:\SemanticSlicer
```

2. From an elevated command prompt install and start the service:

```cmd
sc create SemanticSlicer binPath= "\"%ProgramFiles%\dotnet\dotnet.exe\" \"C:\\SemanticSlicer\\SemanticSlicer.Service.dll\""
sc start SemanticSlicer
```

Once running you can POST text to the service:

```bash
curl -X POST http://localhost:5000/slice -H "Content-Type: application/json" \
    -d '{"content":"Hello world"}'
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
