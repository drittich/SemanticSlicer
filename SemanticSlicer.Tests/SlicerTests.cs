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
			var options = new SlicerOptions { StripHtml = true };
			var slicer = new Slicer(options);
			string input = "Some <b>HTML</b> content";

			// Act
			var result = slicer.GetDocumentChunks(input, null);

			// Assert
			Assert.Equal("Some HTML content", result[0].Content);
		}
	}
}
