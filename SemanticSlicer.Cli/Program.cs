using System.IO.Pipes;
using System.Text.Json;
using SemanticSlicer;

static void PrintUsage()
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  SemanticSlicer.Cli [file]");
    Console.WriteLine("  SemanticSlicer.Cli daemon [--pipe name]");
    Console.WriteLine();
    Console.WriteLine("If a file is specified, slices are written as JSON to stdout.");
    Console.WriteLine("If no file is specified, content is read from standard input.");
    Console.WriteLine("The daemon subcommand keeps a slicer in memory and reads lines\n" +
                      "from stdin or the named pipe, outputting JSON slices to stdout.");
}

if (args.Length > 0 && args[0] == "daemon")
{
    string? pipeName = null;
    if (args.Length > 2 && args[1] == "--pipe")
    {
        pipeName = args[2];
    }
    StartDaemon(pipeName);
    return;
}

// Option 1: run once
string? file = null;
if (args.Length > 0)
{
    file = args[0];
}

string input;
if (file != null)
{
    if (!File.Exists(file))
    {
        Console.Error.WriteLine($"File not found: {file}");
        return;
    }
    input = File.ReadAllText(file);
}
else
{
    using var reader = new StreamReader(Console.OpenStandardInput());
    input = reader.ReadToEnd();
    if (string.IsNullOrWhiteSpace(input))
    {
        PrintUsage();
        return;
    }
}

var slicer = new Slicer();
var chunks = slicer.GetDocumentChunks(input);
Console.WriteLine(JsonSerializer.Serialize(chunks, new JsonSerializerOptions { WriteIndented = true }));

static void StartDaemon(string? pipeName)
{
    TextReader reader;
    if (!string.IsNullOrWhiteSpace(pipeName))
    {
        var pipe = new NamedPipeServerStream(pipeName, PipeDirection.In);
        Console.WriteLine($"Waiting for connection on pipe '{pipeName}'...");
        pipe.WaitForConnection();
        reader = new StreamReader(pipe);
    }
    else
    {
        reader = Console.In;
        Console.WriteLine("Reading requests from stdin. Press Ctrl+C to exit.");
    }

    var slicer = new Slicer();
    while (true)
    {
        var line = reader.ReadLine();
        if (line == null) break;
        if (string.IsNullOrWhiteSpace(line)) continue;
        var chunks = slicer.GetDocumentChunks(line);
        Console.WriteLine(JsonSerializer.Serialize(chunks));
    }
}
