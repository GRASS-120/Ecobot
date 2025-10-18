using System;
using System.Collections.Generic;

namespace Handbook.Parser.InlineTypes
{
    [Serializable]
    public sealed class LinkInline : HandbookInlineBase
    {
        public string url;
        public List<HandbookInlineBase> children = new();
    }
}