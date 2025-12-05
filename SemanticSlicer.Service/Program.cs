using SemanticSlicer;
using SemanticSlicer.Service.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.MapPost("/slice", (SliceRequest request) =>
{
	var metadata = request.Metadata;
	var overlapPercentage = Math.Clamp(request.OverlapPercentage, 0, 100);
	var slicerOptions = new SlicerOptions { OverlapPercentage = overlapPercentage };
	var slicer = new Slicer(slicerOptions);
	return slicer.GetDocumentChunks(request.Content ?? string.Empty, metadata, request.ChunkHeader ?? string.Empty);
});

app.Run();
