using System;
using SemanticSlicer.Models;
using SemanticSlicer.Services.Encoders;

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
		public Encoding Encoding { get; set; } = Encoding.Cl100K;

		/// <summary>
		/// 
		/// </summary>
		public IEncoder CustomEncoder { get; set; }

		/// <summary>
		/// Gets or sets the separators used for splitting documents. Default is Separators.Text.
		/// </summary>
		public Separator[] Separators { get; set; } = SemanticSlicer.Separators.Text;

		/// <summary>
		/// Gets or sets a value indicating whether to strip HTML tags from the input text before chunking. Default is false.
		/// </summary>
		public bool StripHtml { get; set; }

		private int _overlapPercentage;

		/// <summary>
		/// Gets or sets the overlap percentage (0-100) to reuse tokens from the previous chunk.
		/// </summary>
		public int OverlapPercentage
		{
			get => _overlapPercentage;
			set => _overlapPercentage = Math.Clamp(value, 0, 100);
		}
	}
}

