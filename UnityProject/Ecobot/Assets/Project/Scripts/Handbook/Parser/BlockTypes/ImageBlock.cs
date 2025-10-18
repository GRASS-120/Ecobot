using System;

namespace Handbook.Parser.BlockTypes
{
    [Serializable]
    public sealed class ImageBlock : HandbookBlockBase
    {
        public string src;
        public string alt;
    }
}