using System.Collections.Generic;

using SemanticSlicer.Models;

namespace SemanticSlicer
{
	public interface ISlicer
	{
		List<DocumentChunk> GetDocumentChunks(string content, Dictionary<string, object?>? metadata = null, string chunkHeader = "");
	}
}