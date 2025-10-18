using System;
using System.Collections.Generic;
using Handbook.Parser.InlineTypes;

namespace Handbook.Parser.BlockTypes
{
    [Serializable]
    public sealed class HeadingBlock : HandbookBlockBase
    {
        public int level;
        public string anchorId;
        public List<HandbookInlineBase> inlines = new();
    }
}