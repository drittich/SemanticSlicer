using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using HtmlAgilityPack;

using SemanticSlicer.Models;
using SemanticSlicer.Services.Encoders;
using Tiktoken.Encodings;

namespace SemanticSlicer
{
	/// <summary>
	/// A utility class for chunking and subdividing text content based on specified separators.
	/// </summary>
	public class Slicer : ISlicer
	{
		static readonly Regex LINE_ENDING_REGEX = new Regex(@"\r\n?|\n", RegexOptions.Compiled);
		static readonly string LINE_ENDING_REPLACEMENT = "\n";
		static readonly HashSet<string> BLOCK_ELEMENTS = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
				"address", "article", "aside", "blockquote", "canvas", "dd", "div", "dl", "dt",
				"fieldset", "figcaption", "figure", "footer", "form", "h1", "h2", "h3", "h4",
				"h5", "h6", "header", "hr", "li", "main", "nav", "noscript", "ol", "output",
				"p", "pre", "section", "table", "tfoot", "ul", "video"
		};

		private SlicerOptions _options;
		private readonly IEncoder _encoder;

		/// <summary>
		/// Initializes a new instance of the <see cref="Slicer"/> class with optional SemanticSlicer options.
		/// </summary>
		/// <param name="options">Optional SemanticSlicer options.</param>
		public Slicer(SlicerOptions? options = null)
		{
			_options = options ?? new SlicerOptions();
			_encoder = GetEncoder(_options.Encoding);

		}

		/// <summary>
		/// Returns a Tiktoken.Encoder instance based on the specified encoding.
		/// </summary>
		/// <param name="encoding">The encoding type to use for the encoder.</param>
		/// <returns>A Tiktoken.Encoder instance corresponding to the specified encoding.</returns>
		/// <exception cref="ArgumentException">Thrown if the specified encoding is not supported.</exception>
		private IEncoder GetEncoder(Encoding encoding)
		{
			switch (encoding)
			{
				case Encoding.O200K:
					return new TikTokEncoder(new O200KBase());
				case Encoding.Cl100K:
					return new TikTokEncoder(new Cl100KBase());
				case Encoding.Custom:
					{
						if (_options.CustomEncoder == null)
							throw new ArgumentException($"CustomEncoder was not set in the options.");
						return _options.CustomEncoder;
					}
				default:
					throw new ArgumentException($"Encoding {encoding} is not supported.");
			}
		}

		/// <summary>
		/// Gets a list of document chunks for the given content.
		/// </summary>
		/// <param name="content">A string representing the content of the document to be chunked.</param>
		/// <param name="metadata">A dictionary representing the metadata of the document. It is a nullable parameter and its default value is null.</param>
		/// <param name="chunkHeader">A string representing the header of every chunk. It has a default value of an empty string. It will always have at least one newline character separating it from the chunk content.</param>
		/// <returns>Returns a list of DocumentChunks after performing a series of actions including normalization, token counting, splitting, indexing, and removing HTML tags, etc.</returns>
		public List<DocumentChunk> GetDocumentChunks(string content, Dictionary<string, object?>? metadata = null, string chunkHeader = "")
		{
			_options.OverlapPercentage = Math.Clamp(_options.OverlapPercentage, 0, 100);

			var massagedChunkHeader = chunkHeader;
			if (!string.IsNullOrWhiteSpace(chunkHeader))
			{
				if (!massagedChunkHeader.EndsWith(LINE_ENDING_REPLACEMENT))
				{
					massagedChunkHeader = $"{massagedChunkHeader}{LINE_ENDING_REPLACEMENT}";
				}
			}

			// make sure chunkHeader token count is less than maxChunkTokenCount
			var chunkHeaderTokenCount = _encoder.CountTokens(massagedChunkHeader);
			if (chunkHeaderTokenCount >= _options.MaxChunkTokenCount)
			{
				throw new ArgumentOutOfRangeException($"Chunk header token count ({chunkHeaderTokenCount}) is greater than max chunk token count ({_options.MaxChunkTokenCount})");
			}

			var massagedContent = NormalizeLineEndings(content).Trim();

			if (_options.StripHtml)
			{
				massagedContent = RemoveNonBodyContent(massagedContent);
			}

			massagedContent = CollapseWhitespace(massagedContent);

			var effectiveTokenCount = _encoder.CountTokens($"{massagedChunkHeader}{massagedContent}");

			var documentChunks = new List<DocumentChunk> {
				new DocumentChunk {
					Content = massagedContent,
					Metadata = metadata,
					TokenCount = effectiveTokenCount,
					StartOffset = 0,
					EndOffset = massagedContent.Length
				}
			};

			var chunks = SplitDocumentChunks(documentChunks, massagedChunkHeader);

			ApplyOverlap(chunks, massagedChunkHeader, massagedContent);

			for (int i = 0; i < chunks.Count; i++)
			{
				// Save the index with the chunk so they can be reassembled in the correct order
				chunks[i].Index = i;
			}

			return chunks;
		}

		private void ApplyOverlap(List<DocumentChunk> chunks, string chunkHeader, string sourceContent)
		{
			if (_options.OverlapPercentage <= 0 || chunks.Count < 2)
			{
				return;
			}

			for (int i = 0; i < chunks.Count - 1; i++)
			{
				var previous = chunks[i];
				var current = chunks[i + 1];

				int requestedOverlapTokens = (int)Math.Floor(previous.TokenCount * _options.OverlapPercentage / 100.0);
				if (requestedOverlapTokens <= 0 || current.EndOffset <= current.StartOffset)
				{
					continue;
				}

				var baseContentSlice = GetContentSlice(sourceContent, current.StartOffset, current.EndOffset);
				int baseTokenCount = _encoder.CountTokens($"{chunkHeader}{baseContentSlice}");
				current.TokenCount = baseTokenCount;
				var baseChunkContent = $"{chunkHeader}{baseContentSlice}";

				int allowedAdditionalTokens = Math.Max(_options.MaxChunkTokenCount - baseTokenCount, 0);
				if (allowedAdditionalTokens <= 0)
				{
					current.Content = baseChunkContent;
					continue;
				}

				int targetAdditionalTokens = Math.Min(requestedOverlapTokens, allowedAdditionalTokens);

				int minStart = Math.Max(previous.StartOffset, 0);
				int maxStart = current.StartOffset;

				if (minStart >= maxStart)
				{
					continue;
				}

				int targetTokenCeiling = Math.Min(_options.MaxChunkTokenCount, baseTokenCount + targetAdditionalTokens);
				int bestStart = maxStart;
				int bestTokenCount = baseTokenCount;

				int low = minStart;
				int high = maxStart;

				while (low <= high)
				{
					int mid = (low + high) / 2;
					var overlappedContent = GetContentSlice(sourceContent, mid, current.EndOffset);
					int tokenCount = _encoder.CountTokens($"{chunkHeader}{overlappedContent}");

					if (tokenCount <= targetTokenCeiling)
					{
						bestStart = mid;
						bestTokenCount = tokenCount;
						high = mid - 1;
					}
					else
					{
						low = mid + 1;
					}
				}

				if (bestStart == maxStart)
				{
					current.Content = baseChunkContent;
					continue;
				}

				var overlappedSlice = GetContentSlice(sourceContent, bestStart, current.EndOffset);
				current.StartOffset = bestStart;
				current.Content = $"{chunkHeader}{overlappedSlice}";
				current.TokenCount = bestTokenCount;
			}
		}

		/// <summary>
		/// Removes all non-body content from the provided HTML string, including scripts and styles, and returns the plain text content.
		/// If a &lt;title&gt; tag is present, its text is prepended to the result, separated by two line breaks.
		/// If no &lt;body&gt; tag is found, all nodes are appended to a new body node before processing.
		/// </summary>
		/// <param name="content">The HTML content as a string.</param>
		/// <returns>
		/// A string containing the extracted title (if present) and the inner text of the body, with scripts and styles removed.
		/// </returns>
		public string RemoveNonBodyContent(string content)
		{
			var doc = new HtmlDocument();
			doc.LoadHtml(content);

			// get body node
			var body = doc.DocumentNode.SelectSingleNode("//body");

			if (body == null)
			{
				//create a new body node and append all nodes to it
				body = doc.CreateElement("body");
				foreach (var node in doc.DocumentNode.ChildNodes)
				{
					body.AppendChild(node.Clone());
				}
			}

			var title = ExtractTitle(doc);

			if (!string.IsNullOrWhiteSpace(title))
			{
				title += $"{LINE_ENDING_REPLACEMENT}{LINE_ENDING_REPLACEMENT}";
			}

			// remove any script and style tags from body
			var nodes = body.SelectNodes("//script|//style");
			if (nodes != null)
			{
				foreach (var node in nodes)
				{
					node.Remove();
				}
			}

			return $"{title}{GetInnerTextWithSpaces(body).Trim()}";
		}

		/// <summary>
		/// Recursively extracts the inner text from the specified <see cref="HtmlNode"/> and its children,
		/// preserving spaces and line breaks for block elements.
		/// </summary>
		/// <param name="node">The root <see cref="HtmlNode"/> from which to extract text.</param>
		/// <returns>
		/// A <see cref="string"/> containing the concatenated inner text of the node and its descendants,
		/// with appropriate spacing and line breaks for readability.
		/// </returns>
		private string GetInnerTextWithSpaces(HtmlNode node)
		{
			var sb = new StringBuilder();
			ProcessNode(node, sb);
			return sb.ToString();
		}

		/// <summary>
		/// Collapses excessive whitespace in the input string.
		/// Ensures that no more than two consecutive line breaks or spaces appear in the result.
		/// </summary>
		/// <param name="input">The input string to process.</param>
		/// <returns>
		/// A string with no more than two consecutive line breaks or spaces.
		/// </returns>
		private string CollapseWhitespace(string input)
		{
			// don't allow more than 2 line breaks in a row or 2 spaces in a row
			return Regex.Replace(input, @"(\r?\n){3,}|\s{3,}", "  ");
		}

		/// <summary>
		/// Recursively processes the specified <see cref="HtmlNode"/> and its children,
		/// appending their inner text to the provided <see cref="StringBuilder"/>.
		/// For block elements, line breaks are inserted before and after their content
		/// to preserve structure and readability.
		/// </summary>
		/// <param name="node">The root <see cref="HtmlNode"/> to process.</param>
		/// <param name="sb">The <see cref="StringBuilder"/> to which the extracted text is appended.</param>
		private void ProcessNode(HtmlNode node, StringBuilder sb)
		{
			foreach (var child in node.ChildNodes)
			{
				if (child.NodeType == HtmlNodeType.Text)
				{
					sb.Append(child.InnerText);
				}
				else if (child.NodeType == HtmlNodeType.Element)
				{
					if (IsBlockElement(child.Name))
					{
						sb.Append(LINE_ENDING_REPLACEMENT);
					}
					ProcessNode(child, sb);
					if (IsBlockElement(child.Name))
					{
						sb.Append(LINE_ENDING_REPLACEMENT);
					}
				}
			}
		}

		/// <summary>
		/// Determines whether the specified HTML element name is a block-level element.
		/// </summary>
		/// <param name="name">The name of the HTML element to check.</param>
		/// <returns>
		/// <c>true</c> if the specified element name is a block-level element; otherwise, <c>false</c>.
		/// </returns>
		private static bool IsBlockElement(string name)
		{
			return BLOCK_ELEMENTS.Contains(name);
		}

		/// <summary>
		/// Extracts the inner text of the first <title> tag from the HTML content.
		/// </summary>
		/// <param name="content">The HTML content as a string.</param>
		/// <returns>The inner text of the <title> tag, or an empty string if not found.</returns>
		public string ExtractTitle(HtmlDocument doc)
		{
			try
			{
				// Select the first <title> node using XPath
				var titleNode = doc.DocumentNode.SelectSingleNode("//head/title");

				// If not found in <head>, try searching the entire document
				if (titleNode == null)
				{
					titleNode = doc.DocumentNode.SelectSingleNode("//title");
				}

				// Return the inner text of the <title> node, or an empty string if not found
				return titleNode?.InnerText.Trim() ?? string.Empty;
			}
			catch (Exception ex)
			{
				// Optionally log the exception
				Console.WriteLine($"Error extracting title: {ex.Message}");
				return string.Empty;
			}
		}

		/// <summary>
		/// Recursively subdivides a list of DocumentChunks into chunks that are less than or equal to maxTokens in length.
		/// </summary>
		/// <param name="documentChunks">The list of document chunks to be subdivided.</param>
		/// <param name="separators">The array of chunk separators.</param>
		/// <param name="maxTokens">The maximum number of tokens allowed in a chunk.</param>
		/// <returns>The list of subdivided document chunks.</returns>
		/// <exception cref="Exception">Thrown when unable to subdivide the string with given regular expressions.</exception>
		private List<DocumentChunk> SplitDocumentChunks(List<DocumentChunk> documentChunks, string chunkHeader)
		{
			var output = new List<DocumentChunk>();

			foreach (var documentChunk in documentChunks)
			{
				if (documentChunk.TokenCount <= _options.MaxChunkTokenCount)
				{
					documentChunk.Content = $"{chunkHeader}{documentChunk.Content}";
					output.Add(documentChunk);
					continue;
				}

				bool subdivided = false;
				foreach (var separator in _options.Separators)
				{
					var matches = separator.Regex.Matches(documentChunk.Content);
					if (matches.Count > 0)
					{
						Match? centermostMatch = GetCentermostMatch(documentChunk, matches);

						if (centermostMatch!.Index == 0)
						{
							continue;
						}

						var splitChunks = SplitChunkBySeparatorMatch(documentChunk, chunkHeader, separator, centermostMatch);

						if (IsSplitBelowThreshold(splitChunks))
						{
							continue;
						}

						// sanity check
						if (splitChunks.Item1.Content.Length < documentChunk.Content.Length && splitChunks.Item2.Content.Length < documentChunk.Content.Length)
						{
							output.AddRange(SplitDocumentChunks(new List<DocumentChunk> { splitChunks.Item1, splitChunks.Item2 }, chunkHeader));
						}

						subdivided = true;
						break;
					}
				}

				if (!subdivided)
				{
					throw new Exception("Unable to subdivide string with given regular expressions");
				}
			}

			return output;
		}

		/// <summary>
		/// Checks if the token percentage of either of the two provided chunks is below the defined threshold.
		/// </summary>
		/// <param name="splitChunks">A tuple containing two chunks of a document.</param>
		/// <returns>Returns true if either of the chunk's token percentage is below the threshold, otherwise false.</returns>
		private bool IsSplitBelowThreshold(Tuple<DocumentChunk, DocumentChunk> splitChunks)
		{
			// Deconstruct the tuple to get the first and second half of the split chunks
			(DocumentChunk firstHalfChunk, DocumentChunk secondHalfChunk) = splitChunks;

			// Calculate the token percentage of the first half of the chunk
			float firstHalfChunkPercentage = (float)firstHalfChunk.TokenCount / _options.MaxChunkTokenCount * 100;

			// Calculate the token percentage of the second half of the chunk
			float secondHalfChunkPercentage = (float)secondHalfChunk.TokenCount / _options.MaxChunkTokenCount * 100;

			// Return true if either of the chunk's token percentage is below the threshold
			return firstHalfChunkPercentage < _options.MinChunkPercentage || secondHalfChunkPercentage < _options.MinChunkPercentage;
		}

		/// <summary>
		/// Splits a <see cref="DocumentChunk"/> into two chunks at the specified separator match.
		/// </summary>
		/// <param name="documentChunk">The original document chunk to split.</param>
		/// <param name="chunkHeader">The header string to prepend to each resulting chunk for token counting.</param>
		/// <param name="separator">The <see cref="Separator"/> used to determine the split location and behavior.</param>
		/// <param name="match">The <see cref="Match"/> object representing the separator match in the content.</param>
		/// <returns>
		/// A <see cref="Tuple{T1, T2}"/> containing two <see cref="DocumentChunk"/> instances:
		/// the first chunk contains the content before the separator, and the second contains the content after the separator.
		/// Both chunks have their token counts calculated with the <paramref name="chunkHeader"/> prepended.
		/// </returns>
		private Tuple<DocumentChunk, DocumentChunk> SplitChunkBySeparatorMatch(
			DocumentChunk documentChunk,
			string chunkHeader,
			Separator separator,
			Match? match)
		{
			int matchIndex = match!.Index;
			var splitContent = DoTextSplit(documentChunk.Content, matchIndex, match.Value, separator.Behavior);

			var firstHalfContent = splitContent.Item1.Trim();
			var secondHalfContent = splitContent.Item2.Trim();

			var firstHalfEffectiveTokenCount = _encoder.CountTokens($"{chunkHeader}{firstHalfContent}");
			var secondHalfEffectiveTokenCount = _encoder.CountTokens($"{chunkHeader}{secondHalfContent}");

			var ret = new Tuple<DocumentChunk, DocumentChunk>(
				new DocumentChunk
				{
					Content = firstHalfContent,
					Metadata = documentChunk.Metadata,
					TokenCount = firstHalfEffectiveTokenCount,
					StartOffset = documentChunk.StartOffset,
					EndOffset = GetFirstEndOffset(documentChunk, matchIndex, match.Value.Length, separator.Behavior)
				},
				new DocumentChunk
				{
					Content = secondHalfContent,
					Metadata = documentChunk.Metadata,
					TokenCount = secondHalfEffectiveTokenCount,
					StartOffset = GetSecondStartOffset(documentChunk, matchIndex, match.Value.Length, separator.Behavior),
					EndOffset = documentChunk.EndOffset
				}
			);

			return ret;
		}

		private static int GetFirstEndOffset(DocumentChunk parentChunk, int matchIndex, int matchLength, SeparatorBehavior behavior)
		{
			int absoluteMatchIndex = parentChunk.StartOffset + matchIndex;

			if (behavior == SeparatorBehavior.Suffix)
			{
				return absoluteMatchIndex + matchLength;
			}

			return absoluteMatchIndex;
		}

		private static int GetSecondStartOffset(DocumentChunk parentChunk, int matchIndex, int matchLength, SeparatorBehavior behavior)
		{
			int absoluteMatchIndex = parentChunk.StartOffset + matchIndex;

			if (behavior == SeparatorBehavior.Suffix || behavior == SeparatorBehavior.Remove)
			{
				return absoluteMatchIndex + matchLength;
			}

			return absoluteMatchIndex;
		}

		/// <summary>
		/// Finds the match in the given matches collection that is closest to the center of the document chunk.
		/// </summary>
		/// <param name="documentChunk">The document chunk.</param>
		/// <param name="matches">The matches collection.</param>
		/// <returns>The match that is closest to the center of the document chunk, or null if the matches collection is empty.</returns>
		private static Match? GetCentermostMatch(DocumentChunk documentChunk, MatchCollection matches)
		{
			// In the case where we're removing HTML tags from the chunks, it's too complex to try to find the
			// centermost match after stripping tags so we do it before, with the asuumption it will be close enough.
			int centerIndex = documentChunk.Content.Length / 2;
			Match? centermostMatch = null;
			int closestDistance = int.MaxValue;

			foreach (Match match in matches.Cast<Match>())
			{
				int distance = Math.Abs(centerIndex - match.Index);
				if (distance < closestDistance)
				{
					closestDistance = distance;
					centermostMatch = match;
				}
			}

			return centermostMatch;
		}

		private static string GetContentSlice(string sourceContent, int startOffset, int endOffset)
		{
			int safeStart = Math.Clamp(startOffset, 0, sourceContent.Length);
			int safeEnd = Math.Clamp(endOffset, 0, sourceContent.Length);

			if (safeEnd < safeStart)
			{
				return string.Empty;
			}

			return sourceContent[safeStart..safeEnd];
		}

		/// <summary>
		/// Splits the content into two strings at the given matchIndex, using the given matchValue as a separator.
		/// The split point varies based on the separatorType.
		/// For example, if the separatorType is Prefix, the split point will be the beginning of the matchValue.
		/// If the separatorType is Suffix, the split point will be the end of the matchValue.
		/// If the separatorType is Default, the matching content will be removed when splitting.
		/// </summary>
		/// <param name="content"></param>
		/// <param name="matchIndex"></param>
		/// <param name="matchValue"></param>
		/// <param name="separatorType"></param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
		private static Tuple<string, string> DoTextSplit(string content, int matchIndex, string matchValue, SeparatorBehavior separatorType)
		{
			int splitIndex1;
			int splitIndex2;

			if (separatorType == SeparatorBehavior.Prefix)
			{
				splitIndex1 = matchIndex;
				splitIndex2 = matchIndex;
			}
			else if (separatorType == SeparatorBehavior.Suffix)
			{
				splitIndex1 = matchIndex + matchValue.Length;
				splitIndex2 = matchIndex + matchValue.Length;
			}
			else if (separatorType == SeparatorBehavior.Remove)
			{
				splitIndex1 = matchIndex;
				splitIndex2 = matchIndex + matchValue.Length;
			}
			else
			{
				throw new Exception($"Unknown SeparatorType: {separatorType}");
			}

			return new Tuple<string, string>(content[..splitIndex1], content[splitIndex2..]);
		}

		/// <summary>
		/// Normalizes line endings in the input string.
		/// </summary>
		/// <param name="input">The input string.</param>
		/// <returns>The string with normalized line endings.</returns>
		private static string NormalizeLineEndings(string input)
		{
			return LINE_ENDING_REGEX.Replace(input, LINE_ENDING_REPLACEMENT);
		}
	}
}