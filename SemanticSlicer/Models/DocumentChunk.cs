using System.Collections.Generic;

namespace SemanticSlicer.Models
{
	/// <summary>
	/// Represents a document chunk with content, token count, and an optional document identifier.
	/// </summary>
	public class DocumentChunk
	{
		/// <summary>
		/// Gets or sets the content of the document chunk.
		/// </summary>
		public string Content { get; set; } = string.Empty;

		public int Index { get; set; }

		public Dictionary<string, object?>? Metadata { get; set; }

		/// <summary>
		/// Gets or sets the number of tokens in the document chunk.
		/// </summary>
		public int TokenCount { get; set; }

		/// <summary>
		/// Gets or sets the start offset (inclusive) of the chunk within the source text.
		/// </summary>
		public int StartOffset { get; set; }

		/// <summary>
		/// Gets or sets the end offset (exclusive) of the chunk within the source text.
		/// </summary>
		public int EndOffset { get; set; }
	}
}
