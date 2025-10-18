using System.Collections.Generic;
using System.Text.RegularExpressions;
using Handbook.Parser.BlockTypes;

namespace Handbook.Parser
{
    public class QuoteBlockParser : IHandbookBlockParser
    {
        private static readonly Regex _rx = new(@"^\s*>\s?(.*)$", RegexOptions.Compiled);

        public bool CanParse(LineCursor cursor)
        {
            var line = cursor.Current;
            return line != null && _rx.IsMatch(line);
        }

        public HandbookBlockBase Parse(LineCursor cursor, BlockParseContext context)
        {
            var buf = new List<string>();

            // Считываем подряд строки с префиксом "> "
            while (!cursor.End && _rx.IsMatch(cursor.Current))
            {
                var m = _rx.Match(cursor.Current);
                buf.Add(m.Groups[1].Value);
                cursor.Advance();
            }

            // Парсим внутренности цитаты тем же пайплайном (без дублирования правил)
            var innerRaw = string.Join("\n", buf);
            var inner = context.ParseInner(innerRaw);

            var quote = new QuoteBlock();
            quote.children.AddRange(inner.Blocks);
            return quote;
        }
    }
}