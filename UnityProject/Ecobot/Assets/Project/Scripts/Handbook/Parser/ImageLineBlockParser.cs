using System.Text.RegularExpressions;
using Handbook.Parser.BlockTypes;

namespace Handbook.Parser
{
    public class ImageLineBlockParser : IHandbookBlockParser
    {
        private static readonly Regex _rx = new(@"^!\[([^\]]*)\]\(([^)]+)\)\s*$", RegexOptions.Compiled);

        public bool CanParse(LineCursor cursor)
        {
            var line = cursor.Current?.Trim();
            return line != null && _rx.IsMatch(line);
        }

        public HandbookBlockBase Parse(LineCursor cursor, BlockParseContext context)
        {
            var line = cursor.Current.Trim();
            var m = _rx.Match(line);

            cursor.Advance();

            return new ImageBlock
            {
                alt = m.Groups[1].Value,
                src = m.Groups[2].Value
            };
        }
    }
}