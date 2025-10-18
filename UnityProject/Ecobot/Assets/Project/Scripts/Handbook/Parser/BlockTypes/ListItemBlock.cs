using System;
using System.Collections.Generic;

namespace Handbook.Parser.BlockTypes
{
    [Serializable]
    public sealed class ListItemBlock : HandbookBlockBase
    {
        public List<HandbookBlockBase> children = new();
    }
}