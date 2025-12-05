namespace SemanticSlicer.Tests
{
	public class SlicerTests
	{
		[Fact]
		public void GetDocumentChunks_ReturnsDocumentChunks()
		{
			// Arrange
			var slicer = new Slicer();
			string content = "This is test content.";

			// Act
			var result = slicer.GetDocumentChunks(content);

			// Assert
			Assert.NotEmpty(result);
			Assert.Equal(content, result[0].Content);
		}

		[Fact]
		public void GetDocumentChunks_ReturnsNormalizedString()
		{
			// Arrange
			var slicer = new Slicer();
			string input = "First line\r\nSecond line\nThird line\rFourth line";

			// Act
			var result = slicer.GetDocumentChunks(input, null);

			// Assert
			Assert.Equal("First line\nSecond line\nThird line\nFourth line", result[0].Content);
		}

		[Fact]
		public void GetDocumentChunks_ReturnsMetadata()
		{
			// Arrange
			var slicer = new Slicer();
			string content = "This is test content.";
			var metadata = new Dictionary<string, object?> { { "Id", 1 } };

			// Act
			var result = slicer.GetDocumentChunks(content, metadata);

			// Assert
			Assert.Equal(metadata["Id"], result[0].Metadata!["Id"]);
		}

		[Fact]
		public void GetDocumentChunks_StripsHtml()
		{
			// Arrange
			var options = new SlicerOptions { StripHtml = true, Separators = Separators.Html };
			var slicer = new Slicer(options);
			string input = "Some <b>HTML</b> content";

			// Act
			var result = slicer.GetDocumentChunks(input, null);

			// Assert
			Assert.Equal("Some HTML content", result[0].Content);
		}

		[Fact]
		public void GetDocumentChunks_PrependsChunkHeader()
		{
			// Arrange
			var slicer = new Slicer();
			string input = "Some content";

			// Act
			var result = slicer.GetDocumentChunks(input, null, "Title: All About Nothing");

			// Assert
			Assert.Equal("Title: All About Nothing\nSome content", result[0].Content);
		}

		[Fact]
		public void GetDocumentChunks_PrependsChunkHeaderToMultipleChunks()
		{
			// Arrange
			var options = new SlicerOptions { MaxChunkTokenCount = 100 };
			var slicer = new Slicer(options);
			string input = @"In the heart of an enchanting forest, kissed by the golden rays of the sun, stood a charming little cottage. The whitewashed wooden walls, thatched roof, and cobblestone path leading to the doorstep were blanketed in hues of vivid green by the elegant garlands of crawling ivy. Vivid flowers in bloom surrounded it, exhaling a perfume that pervaded the air, mingling with the earthy aroma of the forest. Every morning, the cottage awoke to the harmonious symphony of chirping birds, and every night, it fell asleep under the soft, lullaby-like rustling of leaves, rocked by the gentle wind. This cottage, nestled in the heart of nature, seemed an extension of the forest itself, a quiet haven of peace, echoing the profound tranquility of its surroundings.";

			// Act
			var result = slicer.GetDocumentChunks(input, null, "Title: Whispers of the Woods");

			// Assert
			Assert.StartsWith("Title: Whispers of the Woods", result[0].Content);
			Assert.StartsWith("Title: Whispers of the Woods", result[1].Content);
		}

		// test that an ArgumentOutOfRangeException error is thrown when the chunk header exceeds the max chunk token count
		[Fact]
                public void GetDocumentChunks_ThrowsWhenChunkHeaderExceedsMaxChunkTokenCount()
                {
                        // Arrange
                        var options = new SlicerOptions { MaxChunkTokenCount = 1 };
                        var slicer = new Slicer(options);
			var chunkHeader = "Title: Whispers of the Woods";
			string input = "Some content";

			// Act & Assert
                        Assert.Throws<ArgumentOutOfRangeException>(() => slicer.GetDocumentChunks(input, null, chunkHeader));
                }

                [Fact]
                public void GetDocumentChunks_AssignsSequentialIndexes()
                {
                        // Arrange
                        var options = new SlicerOptions { MaxChunkTokenCount = 100 };
                        var slicer = new Slicer(options);
                        string input = @"In the heart of an enchanting forest, kissed by the golden rays of the sun, stood a charming little cottage. The whitewashed wooden walls, thatched roof, and cobblestone path leading to the doorstep were blanketed in hues of vivid green by the elegant garlands of crawling ivy. Vivid flowers in bloom surrounded it, exhaling a perfume that pervaded the air, mingling with the earthy aroma of the forest. Every morning, the cottage awoke to the harmonious symphony of chirping birds, and every night, it fell asleep under the soft, lullaby-like rustling of leaves, rocked by the gentle wind. This cottage, nestled in the heart of nature, seemed an extension of the forest itself, a quiet haven of peace, echoing the profound tranquility of its surroundings.";

                        // Act
                        var result = slicer.GetDocumentChunks(input);

                // Assert
                Assert.True(result.Count > 1, "Expected multiple chunks to verify indexing.");
                for (int i = 0; i < result.Count; i++)
                {
                        Assert.Equal(i, result[i].Index);
                }
                }

		[Fact]
		public void GetDocumentChunks_ZeroOverlapPreservesBoundaries()
		{
			// Arrange
			var options = new SlicerOptions { MaxChunkTokenCount = 60, OverlapPercentage = 0 };
			var slicer = new Slicer(options);
			string input = @"In the heart of an enchanting forest, kissed by the golden rays of the sun, stood a charming little cottage. The whitewashed wooden walls, thatched roof, and cobblestone path leading to the doorstep were blanketed in hues of vivid green by the elegant garlands of crawling ivy. Vivid flowers in bloom surrounded it, exhaling a perfume that pervaded the air, mingling with the earthy aroma of the forest.";

			// Act
			var result = slicer.GetDocumentChunks(input);

			// Assert
			Assert.True(result.Count > 1, "Expected multiple chunks with the provided token limit.");
			for (int i = 1; i < result.Count; i++)
			{
				Assert.True(result[i].StartOffset >= result[i - 1].StartOffset);
			}
		}

		[Fact]
		public void GetDocumentChunks_AppliesOverlapWithinMaxTokenLimit()
		{
			// Arrange
			var baselineOptions = new SlicerOptions { MaxChunkTokenCount = 120, OverlapPercentage = 0 };
			var overlapOptions = new SlicerOptions { MaxChunkTokenCount = 120, OverlapPercentage = 50 };
			string input = @"In the heart of an enchanting forest, kissed by the golden rays of the sun, stood a charming little cottage. The whitewashed wooden walls, thatched roof, and cobblestone path leading to the doorstep were blanketed in hues of vivid green by the elegant garlands of crawling ivy. Vivid flowers in bloom surrounded it, exhaling a perfume that pervaded the air, mingling with the earthy aroma of the forest. Every morning, the cottage awoke to the harmonious symphony of chirping birds, and every night, it fell asleep under the soft, lullaby-like rustling of leaves, rocked by the gentle wind.";

			// Act
			var baseline = new Slicer(baselineOptions).GetDocumentChunks(input);
			var overlapped = new Slicer(overlapOptions).GetDocumentChunks(input);

			// Assert
			Assert.True(baseline.Count > 1, "Expected the input to split into multiple chunks.");
			Assert.Equal(baseline.Count, overlapped.Count);
			Assert.True(overlapped[1].StartOffset < baseline[1].StartOffset);
			Assert.All(overlapped, chunk => Assert.True(chunk.TokenCount <= overlapOptions.MaxChunkTokenCount));
		}

		[Fact]
		public void GetDocumentChunks_HeaderReducesAvailableOverlap()
		{
			// Arrange
			var optionsWithoutHeader = new SlicerOptions { MaxChunkTokenCount = 30, OverlapPercentage = 50 };
			var optionsWithHeader = new SlicerOptions { MaxChunkTokenCount = 30, OverlapPercentage = 50 };
			string input = @"Paragraph one with content to ensure chunking happens across separators. Paragraph two follows with similar length to force consistent splitting. Paragraph three adds more context and words.";
			var longHeader = string.Join(" ", Enumerable.Repeat("Header", 8));

			// Act
			var withoutHeader = new Slicer(optionsWithoutHeader).GetDocumentChunks(input);
			var withHeader = new Slicer(optionsWithHeader).GetDocumentChunks(input, null, longHeader);

			// Assert
			Assert.True(withoutHeader.Count > 1 && withHeader.Count > 1, "Expected multiple chunks to measure overlap.");
			Assert.Equal(withoutHeader.Count, withHeader.Count);
			Assert.True(withHeader[1].StartOffset >= withoutHeader[1].StartOffset);
			Assert.All(withHeader, chunk => Assert.True(chunk.TokenCount <= optionsWithHeader.MaxChunkTokenCount));
		}

		[Fact]
		public void GetDocumentChunks_OverlapsAfterHtmlStripping()
		{
			// Arrange
			string htmlInput = "<html><body><p>This is the first paragraph in the document.</p><p>This is the second paragraph with additional words for length.</p><p>This is the third paragraph with even more words to force multiple slices.</p><p>Fourth paragraph adds even more padding text to push over token limits.</p><p>Fifth paragraph keeps going with extra words and sentences to ensure splitting.</p></body></html>";
			var baseOptions = new SlicerOptions { Separators = Separators.Html, StripHtml = true, MaxChunkTokenCount = 50, OverlapPercentage = 0 };
			var overlapOptions = new SlicerOptions { Separators = Separators.Html, StripHtml = true, MaxChunkTokenCount = 50, OverlapPercentage = 30 };

			// Act
			var baseline = new Slicer(baseOptions).GetDocumentChunks(htmlInput);
			var overlapped = new Slicer(overlapOptions).GetDocumentChunks(htmlInput);

			// Assert
			Assert.True(baseline.Count > 1, "Expected multiple HTML chunks after stripping.");
			Assert.Equal(baseline.Count, overlapped.Count);
			Assert.True(overlapped[1].StartOffset < baseline[1].StartOffset);
			Assert.All(overlapped, chunk => Assert.True(chunk.TokenCount <= overlapOptions.MaxChunkTokenCount));
		}
        }
}
