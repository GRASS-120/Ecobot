using System;
using Handbook.Routing;

namespace Handbook.Parser
{
    public class BlockParseContext
    {
        public AnchorIdGenerator AnchorIdGenerator { get; }
        public IHandbookInlineParser InlineParser { get; }
        public IHandbookLinkRouter LinkRouter { get; }
        public Func<string, HandbookParseResult> ParseInner { get; }

        public BlockParseContext(
            AnchorIdGenerator anchorIdGenerator,
            IHandbookInlineParser inlineParser,
            IHandbookLinkRouter linkRouter,
            Func<string, HandbookParseResult> parseInner)
        {
            AnchorIdGenerator = anchorIdGenerator;
            InlineParser = inlineParser;
            LinkRouter = linkRouter;
            ParseInner = parseInner;
        }
    }
}