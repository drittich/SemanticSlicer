using SemanticSlicer.Models;

namespace SemanticSlicer
{
	/// <summary>
	/// Defines lists of separators defined as regular expressions for splitting documents.
	/// Separators indicating content structure are placed early in the list.
	/// . is used as a catch-all separator at the end of the list, so we can split any content.
	/// </summary>
	public static class Separators
	{
		public static readonly Separator[] Text = new Separator[] {
			new Separator(@"\n\n", SeparatorBehavior.Remove),
			new Separator(@"\. ", SeparatorBehavior.Suffix),
			new Separator(@"! ", SeparatorBehavior.Suffix),
			new Separator(@"\? ", SeparatorBehavior.Suffix),
			new Separator(@"\n", SeparatorBehavior.Remove),
			new Separator(@";", SeparatorBehavior.Remove),
			new Separator(@"\(", SeparatorBehavior.Prefix),
			new Separator(@"\)", SeparatorBehavior.Suffix),
			new Separator(@",", SeparatorBehavior.Remove),
			new Separator(@"-", SeparatorBehavior.Remove),
			new Separator(@" ", SeparatorBehavior.Remove),
			new Separator(@".", SeparatorBehavior.Suffix),
		};

		public static readonly Separator[] Markdown = new Separator[] {
			new Separator(@"^##\s+.+$", SeparatorBehavior.Prefix),
			new Separator(@"^###\s+.+$", SeparatorBehavior.Prefix),
			new Separator(@"^####\s+.+$", SeparatorBehavior.Prefix),
			new Separator(@"^#####\s+.+$", SeparatorBehavior.Prefix),
			new Separator(@"^######\s+.+$", SeparatorBehavior.Prefix),
			new Separator(@"\n\n", SeparatorBehavior.Remove),
			new Separator(@"\. ", SeparatorBehavior.Suffix),
			new Separator(@"! ", SeparatorBehavior.Suffix),
			new Separator(@"\? ", SeparatorBehavior.Suffix),
			new Separator(@"\n", SeparatorBehavior.Remove),
			new Separator(@";", SeparatorBehavior.Remove),
			new Separator(@"\(", SeparatorBehavior.Prefix),
			new Separator(@"\)", SeparatorBehavior.Suffix),
			new Separator(@",", SeparatorBehavior.Remove),
			new Separator(@"-", SeparatorBehavior.Remove),
			new Separator(@" ", SeparatorBehavior.Remove),
			new Separator(@".", SeparatorBehavior.Suffix),
		};
	}
}
