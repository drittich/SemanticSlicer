using System.Text.RegularExpressions;

namespace SemanticSlicer
{
	/// <summary>
	/// Public utility methods for text preprocessing operations used by SemanticSlicer.
	/// These methods can be used independently for custom preprocessing pipelines.
	/// </summary>
	public static class TextUtilities
	{
		private static readonly Regex LINE_ENDING_REGEX = new Regex(@"\r\n?|\n", RegexOptions.Compiled);
		private const string LINE_ENDING_REPLACEMENT = "\n";

		/// <summary>
		/// Normalizes line endings in the input string to use Unix-style line endings (\n).
		/// Converts \r\n (Windows) and \r (Mac) to \n.
		/// </summary>
		/// <param name="input">The input string.</param>
		/// <returns>The string with normalized line endings.</returns>
		public static string NormalizeLineEndings(string input)
		{
			return LINE_ENDING_REGEX.Replace(input, LINE_ENDING_REPLACEMENT);
		}

		/// <summary>
		/// Collapses excessive whitespace in the input string.
		/// Ensures that no more than two consecutive line breaks or spaces appear in the result.
		/// </summary>
		/// <param name="input">The input string to process.</param>
		/// <returns>
		/// A string with no more than two consecutive line breaks or spaces.
		/// </returns>
		public static string CollapseWhitespace(string input)
		{
			// don't allow more than 2 line breaks in a row or 2 spaces in a row
			return Regex.Replace(input, @"(\r?\n){3,}|\s{3,}", "  ");
		}
	}
}
