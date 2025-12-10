namespace SemanticSlicer.Services.Encoders
{
  /// <summary>
  /// Defines a contract for encoding text into tokens. Implement this interface to provide custom tokenization logic.
  /// </summary>
	public interface IEncoder
	{
		int CountTokens(string text);
	}
}
