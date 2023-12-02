using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using HtmlAgilityPack;

using SemanticSlicer.Models;

namespace SemanticSlicer
{
	/// <summary>
	/// A utility class for chunking and subdividing text content based on specified separators.
	/// </summary>
	public class Slicer : ISlicer
	{
		static readonly Regex LINE_ENDING_REGEX = new Regex(@"\r\n?|\n", RegexOptions.Compiled);
		static readonly string LINE_ENDING_REPLACEMENT = "\n";

		private SlicerOptions _options;
		private readonly Tiktoken.Encoding _encoding;

		/// <summary>
		/// Initializes a new instance of the <see cref="Slicer"/> class with optional SemanticSlicer options.
		/// </summary>
		/// <param name="options">Optional SemanticSlicer options.</param>
		public Slicer(SlicerOptions? options = null)
		{
			_options = options ?? new SlicerOptions();
			_encoding = Tiktoken.Encoding.Get(_options.Encoding);
		}

		/// <summary>
		/// Gets a list of document chunks for the specified content and document ID.
		/// </summary>
		/// <param name="content">The input content to be chunked.</param>
		/// <param name="documentId">The identifier for the document.</param>
		/// <returns>A list of document chunks.</returns>
		public List<DocumentChunk> GetDocumentChunks(string content, Dictionary<string, object?>? metadata = null)
		{
			var massagedContent = NormalizeLineEndings(content).Trim();
			var effectiveTokenCount = _options.StripHtml
				? _encoding.CountTokens(StripHtmlTags(massagedContent))
				: _encoding.CountTokens(massagedContent);

			var documentChunks = new List<DocumentChunk> {
				new DocumentChunk {
					Content = massagedContent,
					Metadata = metadata,
					TokenCount = effectiveTokenCount
				}
			};
			var chunks = SplitDocumentChunks(documentChunks);

			foreach (var chunk in chunks)
			{
				// Save the index with the chunk so they can be reassembled in the correct order
				chunk.Index = chunks.IndexOf(chunk);

				// Strip HTML tags from the content if requested
				if (_options.StripHtml)
					chunk.Content = StripHtmlTags(chunk.Content);
			}

			return chunks;
		}

		public string StripHtmlTags(string content)
		{
			var doc = new HtmlDocument();
			doc.LoadHtml(content);
			return doc.DocumentNode.InnerText;
		}

		/// <summary>
		/// Recursively subdivides a list of DocumentChunks into chunks that are less than or equal to maxTokens in length.
		/// </summary>
		/// <param name="documentChunks">The list of document chunks to be subdivided.</param>
		/// <param name="separators">The array of chunk separators.</param>
		/// <param name="maxTokens">The maximum number of tokens allowed in a chunk.</param>
		/// <returns>The list of subdivided document chunks.</returns>
		/// <exception cref="Exception">Thrown when unable to subdivide the string with given regular expressions.</exception>
		private List<DocumentChunk> SplitDocumentChunks(List<DocumentChunk> documentChunks)
		{
			var output = new List<DocumentChunk>();

			foreach (var documentChunk in documentChunks)
			{
				if (documentChunk.TokenCount <= _options.MaxChunkTokenCount)
				{
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

						var splitChunks = SplitChunkBySeparatorMatch(documentChunk, separator, centermostMatch);

						if (IsSplitBelowThreshold(splitChunks))
						{
							continue;
						}

						// sanity check
						if (splitChunks.Item1.Content.Length < documentChunk.Content.Length && splitChunks.Item2.Content.Length < documentChunk.Content.Length)
						{
							output.AddRange(SplitDocumentChunks(new List<DocumentChunk> { splitChunks.Item1, splitChunks.Item2 }));
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

		private Tuple<DocumentChunk, DocumentChunk> SplitChunkBySeparatorMatch(DocumentChunk documentChunk, Separator separator, Match? match)
		{
			int matchIndex = match!.Index;
			var splitContent = DoTextSplit(documentChunk.Content, matchIndex, match.Value, separator.Behavior);

			var firstHalfContent = splitContent.Item1.Trim();
			var secondHalfContent = splitContent.Item2.Trim();

			var effectiveFirstHalfTokenCount = _options.StripHtml
				? _encoding.CountTokens(StripHtmlTags(firstHalfContent))
				: _encoding.CountTokens(firstHalfContent);
			var effectiveSecondHalfTokenCount = _options.StripHtml
				? _encoding.CountTokens(StripHtmlTags(secondHalfContent))
				: _encoding.CountTokens(secondHalfContent);

			var ret = new Tuple<DocumentChunk, DocumentChunk>(
				new DocumentChunk
				{
					Content = firstHalfContent,
					Metadata = documentChunk.Metadata,
					TokenCount = effectiveFirstHalfTokenCount
				},
				new DocumentChunk
				{
					Content = secondHalfContent,
					Metadata = documentChunk.Metadata,
					TokenCount = effectiveSecondHalfTokenCount
				}
			);

			return ret;
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
