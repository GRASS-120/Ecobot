using System.Collections.Generic;
using Handbook.Models;
using Handbook.Parser.BlockTypes;

namespace Handbook.Parser
{
    public class HandbookParseResult
    {
        public List<HandbookBlockBase> Blocks { get; } = new();
        public List<HandbookAnchor> Anchors { get; } = new();
        public List<HandbookLink> Links { get; } = new();
    }
}