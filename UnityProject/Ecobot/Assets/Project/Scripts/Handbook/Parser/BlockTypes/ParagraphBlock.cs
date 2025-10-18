using System;
using System.Collections.Generic;
using Handbook.Parser.InlineTypes;

namespace Handbook.Parser.BlockTypes
{
    [Serializable]
    public sealed class ParagraphBlock : HandbookBlockBase
    {
        public List<HandbookInlineBase> inlines = new();
    }
}