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
	}
}
