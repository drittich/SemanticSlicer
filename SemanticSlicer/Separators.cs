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

		public static readonly Separator[] Html = new Separator[] {
			new Separator(@"<body", SeparatorBehavior.Prefix),
			new Separator(@"<div", SeparatorBehavior.Prefix),
			new Separator(@"<p", SeparatorBehavior.Prefix),
			new Separator(@"<br", SeparatorBehavior.Prefix),
			new Separator(@"<li", SeparatorBehavior.Prefix),
			new Separator(@"<h1", SeparatorBehavior.Prefix),
			new Separator(@"<h2", SeparatorBehavior.Prefix),
			new Separator(@"<h3", SeparatorBehavior.Prefix),
			new Separator(@"<h4", SeparatorBehavior.Prefix),
			new Separator(@"<h5", SeparatorBehavior.Prefix),
			new Separator(@"<h6", SeparatorBehavior.Prefix),
			new Separator(@"<span", SeparatorBehavior.Prefix),
			new Separator(@"<table", SeparatorBehavior.Prefix),
			new Separator(@"<tr", SeparatorBehavior.Prefix),
			new Separator(@"<td", SeparatorBehavior.Prefix),
			new Separator(@"<th", SeparatorBehavior.Prefix),
			new Separator(@"<ul", SeparatorBehavior.Prefix),
			new Separator(@"<ol", SeparatorBehavior.Prefix),
			new Separator(@"<header", SeparatorBehavior.Prefix),
			new Separator(@"<footer", SeparatorBehavior.Prefix),
			new Separator(@"<nav", SeparatorBehavior.Prefix),
			new Separator(@"<head", SeparatorBehavior.Prefix),
			new Separator(@"<style", SeparatorBehavior.Prefix),
			new Separator(@"<script", SeparatorBehavior.Prefix),
			new Separator(@"<meta", SeparatorBehavior.Prefix),
			new Separator(@"<title", SeparatorBehavior.Prefix),
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

