using System;
using System.Collections.Generic;

namespace Handbook.Parser.BlockTypes
{
    [Serializable]
    public sealed class QuoteBlock : HandbookBlockBase
    {
        public List<HandbookBlockBase> children = new();
    }
}