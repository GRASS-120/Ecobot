using System.Text.RegularExpressions;
using Handbook.Parser.BlockTypes;
using Handbook.Parser.InlineTypes;

namespace Handbook.Parser
{
    public class HeadingBlockParser : IHandbookBlockParser
    {
        private static readonly Regex _rx = new(@"^(#{1,6})\s+(.*)$", RegexOptions.Compiled);

        public bool CanParse(LineCursor cursor)
        {
            var line = cursor.Current;
            return line != null && _rx.IsMatch(line);
        }

        public HandbookBlockBase Parse(LineCursor cursor, BlockParseContext context)
        {
            var line = cursor.Current;
            var m = _rx.Match(line);

            var level = m.Groups[1].Value.Length;
            var text = m.Groups[2].Value.Trim().TrimEnd('#').Trim();

            var block = new HeadingBlock { level = level };
            block.inlines.Add(new TextRunInline { text = text });
            block.anchorId = context.AnchorIdGenerator.Create(text);

            cursor.Advance();
            return block;
        }
    }
}