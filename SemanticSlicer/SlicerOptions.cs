using SemanticSlicer.Models;

namespace SemanticSlicer
{
	/// <summary>
	/// The options for configuring SemanticSlicer processing.
	/// </summary>
	public class SlicerOptions
	{
		/// <summary>
		/// Gets or sets the maximum token count allowed in a chunk. Default is 1000.
		/// </summary>
		public int MaxChunkTokenCount { get; set; } = 1000;

		/// <summary>
		/// Gets or sets the minimum chunk percentage relative to MaxChunkTokenCount. Default is 10%.
		/// </summary>
		public int MinChunkPercentage { get; set; } = 10;

		/// <summary>
		/// Gets or sets the encoding used for semantic processing. Default is "cl100k_base".
		/// </summary>
		public string Encoding { get; set; } = "cl100k_base";

		/// <summary>
		/// Gets or sets the separators used for splitting documents. Default is Separators.Text.
		/// </summary>
		public Separator[] Separators { get; set; } = SemanticSlicer.Separators.Text;

		/// <summary>
		/// Gets or sets a value indicating whether to strip HTML tags from the input text before chunking. Default is false.
		/// </summary>
		public bool StripHtml { get; set; }
	}
}
