using SemanticSlicer;
using SemanticSlicer.Service.Models;

var builder = WebApplication.CreateBuilder(args);

// Keep the Slicer in memory as a singleton
builder.Services.AddSingleton<Slicer>();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.MapPost("/slice", (SliceRequest request, Slicer slicer) =>
{
    var metadata = request.Metadata;
    return slicer.GetDocumentChunks(request.Content ?? string.Empty, metadata, request.ChunkHeader ?? string.Empty);
});

app.Run();
