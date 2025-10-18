using System;
using System.Collections.Generic;

namespace Handbook.Parser.BlockTypes
{
    [Serializable]
    public sealed class ListBlock : HandbookBlockBase
    {
        public bool ordered;
        public List<ListItemBlock> items = new();
    }
}