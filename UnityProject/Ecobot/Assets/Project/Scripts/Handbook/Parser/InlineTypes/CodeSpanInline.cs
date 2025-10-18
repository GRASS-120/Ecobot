using System;

namespace Handbook.Parser.InlineTypes
{
    [Serializable]
    public sealed class CodeSpanInline : HandbookInlineBase
    {
        public string text;
    }
}