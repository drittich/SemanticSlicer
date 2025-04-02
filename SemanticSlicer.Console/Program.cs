using System.CommandLine;

namespace SemanticSlicer.Console;

internal class Program
{
	static async Task Main(string[] args)
	{
		System.Console.WriteLine("Hello, World!");

		// Define file path argument
		var filePathArg = new Argument<FileInfo>(
			name: "file-path",
			description: "Path to the file to process");

		// Define options for SlicerOptions properties
		var maxChunkTokenCountOption = new Option<int>(
			name: "--max-chunk-token-count",
			description: "Maximum token count allowed in a chunk",
			getDefaultValue: () => 1000);

		var minChunkPercentageOption = new Option<int>(
			name: "--min-chunk-percentage",
			description: "Minimum chunk percentage relative to MaxChunkTokenCount",
			getDefaultValue: () => 10);

		var rootCommand = new RootCommand("SemanticSlicer.Console");
		rootCommand.Add(filePathArg);
		rootCommand.Add(minChunkPercentageOption);
		rootCommand.Add(maxChunkTokenCountOption);

		rootCommand.SetHandler(
			async (filePath, minChunkPercentage, maxChunkTokenCount) =>
			{
				await DisplayIntAndString(filePath, minChunkPercentage, maxChunkTokenCount);
			},
			filePathArg, minChunkPercentageOption, maxChunkTokenCountOption);

		await rootCommand.InvokeAsync(args);
	}

	private static Task DisplayIntAndString(FileInfo filePath, int minChunkPercentage, int maxChunkTokenCount)
	{
		// Implementation of the method
		throw new NotImplementedException();
	}
}
