namespace SemanticSlicer.Service.Models
{
    public class SliceRequest
    {
        public string Content { get; set; } = string.Empty;
        public Dictionary<string, object?>? Metadata { get; set; }
        public string? ChunkHeader { get; set; }
    }
}
