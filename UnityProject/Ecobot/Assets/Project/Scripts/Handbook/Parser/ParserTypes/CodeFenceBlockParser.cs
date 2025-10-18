using System.Text;
using System.Text.RegularExpressions;
using Handbook.Parser.BlockTypes;

namespace Handbook.Parser
{
    public class CodeFenceBlockParser : IHandbookBlockParser
    {
        private static readonly Regex _open = new(@"^\s*```([a-zA-Z0-9_-]*)\s*$", RegexOptions.Compiled);
        private static readonly Regex _close = new(@"^\s*```", RegexOptions.Compiled);

        public bool CanParse(LineCursor cursor)
        {
            var line = cursor.Current;
            return line != null && _open.IsMatch(line);
        }

        public HandbookBlockBase Parse(LineCursor cursor, BlockParseContext context)
        {
            var open = cursor.Current;
            var lang = _open.Match(open).Groups[1].Value;
            cursor.Advance();

            // Собираем строки до закрывающей "```"
            var sb = new StringBuilder();
            while (!cursor.End && !_close.IsMatch(cursor.Current))
            {
                sb.AppendLine(cursor.Current);
                cursor.Advance();
            }

            if (!cursor.End) cursor.Advance(); // съесть закрывающую ограду

            return new CodeBlock
            {
                language = string.IsNullOrWhiteSpace(lang) ? null : lang,
                code = sb.ToString()
            };
        }
    }
}