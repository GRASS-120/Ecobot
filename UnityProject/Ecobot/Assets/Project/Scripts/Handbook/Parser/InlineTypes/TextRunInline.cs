using System;

namespace Handbook.Parser.InlineTypes
{
    [Serializable]
    public sealed class TextRunInline : HandbookInlineBase
    {
        public string text;
    }
}