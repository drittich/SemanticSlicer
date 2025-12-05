using System.IO.Pipes;
using System.Text.Json;
using SemanticSlicer;

static void PrintUsage()
{
	Console.WriteLine("Usage:");
	Console.WriteLine("  SemanticSlicer.Cli [--overlap <0-100>] [file]");
	Console.WriteLine("  SemanticSlicer.Cli daemon [--pipe name] [--overlap <0-100>]");
	Console.WriteLine();
	Console.WriteLine("If a file is specified, slices are written as JSON to stdout.");
	Console.WriteLine("If no file is specified, content is read from standard input.");
	Console.WriteLine("The daemon subcommand keeps a slicer in memory and reads lines");
	Console.WriteLine("from stdin or the named pipe, outputting JSON slices to stdout.");
	Console.WriteLine("Overlap is a percentage of the previous chunk to prepend as context (default 0).");
}

bool isDaemon = args.Length > 0 && args[0] == "daemon";
int overlapPercentage = 0;
string? pipeName = null;
string? file = null;

int argIndex = isDaemon ? 1 : 0;
for (int i = argIndex; i < args.Length; i++)
{
	switch (args[i])
	{
		case "--pipe" when isDaemon:
			if (i + 1 >= args.Length)
			{
				Console.Error.WriteLine("Missing pipe name for --pipe option.");
				return;
			}
			pipeName = args[++i];
			break;
		case "--overlap":
			if (i + 1 >= args.Length || !int.TryParse(args[i + 1], out var overlapValue))
			{
				Console.Error.WriteLine("Invalid overlap percentage. Provide an integer between 0 and 100.");
				return;
			}
			overlapPercentage = Math.Clamp(overlapValue, 0, 100);
			i++;
			break;
		default:
			if (!isDaemon && file == null)
			{
				file = args[i];
			}
			else
			{
				Console.Error.WriteLine($"Unknown argument: {args[i]}");
				PrintUsage();
				return;
			}
			break;
	}
}

if (isDaemon)
{
	StartDaemon(pipeName, overlapPercentage);
	return;
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

var slicerOptions = new SlicerOptions { OverlapPercentage = overlapPercentage };
var slicer = new Slicer(slicerOptions);
var chunks = slicer.GetDocumentChunks(input);
Console.WriteLine(JsonSerializer.Serialize(chunks, new JsonSerializerOptions { WriteIndented = true }));

static void StartDaemon(string? pipeName, int overlapPercentage)
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

	var slicer = new Slicer(new SlicerOptions { OverlapPercentage = overlapPercentage });
	while (true)
	{
		var line = reader.ReadLine();
		if (line == null) break;
		if (string.IsNullOrWhiteSpace(line)) continue;
		var chunks = slicer.GetDocumentChunks(line);
		Console.WriteLine(JsonSerializer.Serialize(chunks));
	}
}

