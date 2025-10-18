using System;

namespace Handbook.Parser.BlockTypes
{
    [Serializable]
    public sealed class CodeBlock : HandbookBlockBase
    {
        public string language;
        public string code;
    }
}