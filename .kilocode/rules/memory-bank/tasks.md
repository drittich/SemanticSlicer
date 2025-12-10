# Tasks

## Publish CLI
**Goal:** Build and publish the CLI to a local folder for cross-platform execution.

**Files/Refs:**
- Project [SemanticSlicer.Cli/SemanticSlicer.Cli.csproj](SemanticSlicer.Cli/SemanticSlicer.Cli.csproj:1)
- README instructions [README.md](README.md:116)

**Steps:**
1. Build publish output:
   - Language: bash
   - Command:
   - dotnet publish SemanticSlicer.Cli/SemanticSlicer.Cli.csproj -c Release -o ./cli
   - Note: You can use the `--self-contained` and `-r <RID>` options for platform-specific binaries if needed.
2. Run once:
   - Language: bash
   - Command:
   - dotnet ./cli/SemanticSlicer.Cli.dll --overlap 30 MyDocument.txt
   - Explanation: `--overlap` carries forward a percentage (0–100) of previous chunk tokens while respecting MaxChunkTokenCount.
3. Pipe input:
   - Language: bash
   - Command:
   - cat MyDocument.txt | dotnet ./cli/SemanticSlicer.Cli.dll --overlap 20
4. Daemon:
   - Language: bash
   - Command:
   - dotnet ./cli/SemanticSlicer.Cli.dll daemon --overlap 25
5. Daemon with named pipe (Unix):
   - Language: bash
   - Command:
   - dotnet ./cli/SemanticSlicer.Cli.dll daemon --pipe slicerpipe --overlap 25

**Notes:**
- Prebuilt binaries are available in Releases [README.md](README.md:52).

## Publish Service
**Goal:** Publish and run the minimal API service to keep Slicer in memory and expose POST /slice.

**Files/Refs:**
- Project [SemanticSlicer.Service/SemanticSlicer.Service.csproj](SemanticSlicer.Service/SemanticSlicer.Service.csproj:1)
- Endpoint [WebApplication.MapPost()](SemanticSlicer.Service/Program.cs:12)
- SliceRequest model [SemanticSlicer.Service/Models/SliceRequest.cs](SemanticSlicer.Service/Models/SliceRequest.cs:1)
- README systemd/Windows examples [README.md](README.md:155)

**Steps:**
1. Publish:
   - Language: bash
   - Command:
   - dotnet publish SemanticSlicer.Service/SemanticSlicer.Service.csproj -c Release -o ./publish
2. Linux systemd unit file:
   - Language: ini
   - Content:
   - [Unit]
     Description=Semantic Slicer Service
     After=network.target

     [Service]
     Type=simple
     WorkingDirectory=/opt/semanticslicer
     ExecStart=/usr/bin/dotnet /opt/semanticslicer/SemanticSlicer.Service.dll
     Restart=always

     [Install]
     WantedBy=multi-user.target
3. Enable/start on Linux:
   - Language: bash
   - Command:
   - sudo systemctl enable semanticslicer
   - sudo systemctl start semanticslicer
4. Windows service creation:
   - Language: cmd
   - Command:
   - sc create SemanticSlicer binPath= "\"%ProgramFiles%\\dotnet\\dotnet.exe\" \"C:\\SemanticSlicer\\SemanticSlicer.Service.dll\""
   - sc start SemanticSlicer
5. Test endpoint:
   - Language: bash
   - Command:
   - curl -X POST http://localhost:5000/slice -H "Content-Type: application/json" -d '{"content":"Hello world","overlapPercentage":30}'
   - Note: `overlapPercentage` is optional (0–100) and header tokens reduce the available overlap budget.

## Use Library via NuGet
**Goal:** Integrate the library directly into a .NET app.

**Files/Refs:**
- README NuGet section [README.md](README.md:39)
- Slicer API [SemanticSlicer.Slicer()](SemanticSlicer/Slicer.cs:37)
- Options [SemanticSlicer.SlicerOptions](SemanticSlicer/SlicerOptions.cs:8)
- Separators [SemanticSlicer.Separators](SemanticSlicer/Separators.cs:10)

**Steps:**
1. Install package:
   - Language: ps
   - Command:
   - dotnet add package drittich.SemanticSlicer
2. Basic usage:
   - Language: csharp
   - Code:
   - var slicer = new SemanticSlicer.Slicer();
     var text = File.ReadAllText("MyDocument.txt");
     var chunks = slicer.GetDocumentChunks(text);
3. Markdown usage:
   - Language: csharp
   - Code:
   - var options = new SemanticSlicer.SlicerOptions { MaxChunkTokenCount = 600, Separators = SemanticSlicer.Separators.Markdown };
     var slicer = new SemanticSlicer.Slicer(options);
     var chunks = slicer.GetDocumentChunks(File.ReadAllText("MyDocument.md"));
4. HTML with stripping:
   - Language: csharp
   - Code:
   - var options = new SemanticSlicer.SlicerOptions { Separators = SemanticSlicer.Separators.Html, StripHtml = true };
     var slicer = new SemanticSlicer.Slicer(options);
     var chunks = slicer.GetDocumentChunks(File.ReadAllText("MyDocument.html"));
5. Metadata passthrough:
   - Language: csharp
   - Code:
   - var meta = new Dictionary<string, object?> { ["Id"] = 123, ["FileName"] = "MyDocument.txt" };
     var chunks = slicer.GetDocumentChunks(File.ReadAllText("MyDocument.txt"), meta);
6. Chunk header:
   - Language: csharp
   - Code:
   - var header = $"FileName: MyDocument.txt";
     var chunks = slicer.GetDocumentChunks(File.ReadAllText("MyDocument.txt"), null, header);

**Considerations:**
- Header token count must be $<$= MaxChunkTokenCount [SemanticSlicer.Slicer.GetDocumentChunks()](SemanticSlicer/Slicer.cs:81).
- Defaults: MaxChunkTokenCount=1000, MinChunkPercentage=10, Encoding=Cl100K, Separators=Text [SemanticSlicer.SlicerOptions](SemanticSlicer/SlicerOptions.cs:13).