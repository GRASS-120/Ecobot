using System.Collections.Generic;
using Handbook.Parser.InlineTypes;

namespace Handbook.Parser
{
    public interface IHandbookInlineParser
    {
        List<HandbookInlineBase> Parse(string text);
    }
}