using System.Text.RegularExpressions;

namespace SemanticSlicer.Models
{
	/// <summary>
	/// Represents a chunk separator with a regular expression pattern and a specified separator type.
	/// </summary>
	public class Separator
	{
		/// <summary>
		/// Gets or sets the regular expression pattern used as the chunk separator.
		/// </summary>
		public Regex Regex { get; set; }

		/// <summary>
		/// Gets or sets the type of separator behavior associated with the chunk.
		/// </summary>
		public SeparatorBehavior Behavior { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="Separator"/> class with the specified regular expression pattern and separator type.
		/// </summary>
		/// <param name="regex">The regular expression pattern used as the chunk separator.</param>
		/// <param name="type">The type of separator behavior. Default is <see cref="SeparatorBehavior.Remove"/>.</param>
		public Separator(string regex, SeparatorBehavior type = SeparatorBehavior.Remove)
		{
			// Using RegexOptions.Compiled for improved performance, especially with repeated use.
			Regex = new Regex(regex, RegexOptions.Compiled | RegexOptions.Multiline);
			Behavior = type;
		}
	}
}
