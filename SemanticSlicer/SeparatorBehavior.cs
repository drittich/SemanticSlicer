namespace SemanticSlicer
{

	/// <summary>
	/// Specifies the type of separator behavior when processing chunks.
	/// </summary>
	public enum SeparatorBehavior
	{
		/// <summary>
		/// The separator will be removed from the chunk.
		/// </summary>
		Remove,

		/// <summary>
		/// The separator will be added to the beginning of the chunk.
		/// </summary>
		Prefix,

		/// <summary>
		/// The separator will be added to the end of the chunk.
		/// </summary>
		Suffix
	}

}
