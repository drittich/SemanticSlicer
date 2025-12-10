using System;
using System.Collections.Generic;
using System.Text;

namespace SemanticSlicer.Services.Encoders
{
  public interface IEncoder
  {
    int CountTokens(string text);
  }
}
