using System;
using System.Collections.Generic;
using System.Text;

namespace SemanticSlicer.Services.Encoders
{
  internal class TikTokEncoder : IEncoder
  {
    private readonly Tiktoken.Encoder tiktokenEncoder;

    public TikTokEncoder(Tiktoken.Encodings.Encoding encoding)
    {
      tiktokenEncoder = new Tiktoken.Encoder(encoding);
    }

    public int CountTokens(string text)
      => tiktokenEncoder.CountTokens(text);
  }
}
