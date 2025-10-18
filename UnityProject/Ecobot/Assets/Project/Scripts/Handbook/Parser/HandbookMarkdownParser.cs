using System;
using System.Collections.Generic;
using Handbook.Models;
using Handbook.Parser.BlockTypes;
using Handbook.Parser.InlineTypes;
using Handbook.Routing;

namespace Handbook.Parser
{
    public class HandbookMarkdownParser : IHandbookMarkdownParser
    {
        private readonly List<IHandbookBlockParser> _parsers = new();
        private readonly IHandbookInlineParser _inlineParser;
        private readonly IHandbookLinkRouter _linkRouter;

        public HandbookMarkdownParser(IHandbookLinkRouter linkRouter)
        {
            _linkRouter = linkRouter ?? throw new ArgumentNullException(nameof(linkRouter));
            _inlineParser = new SimpleInlineParser();

            // Порядок приоритета важен: сначала наиболее специфичные
            _parsers.Add(new CodeFenceBlockParser());
            _parsers.Add(new HorizontalRuleBlockParser());
            _parsers.Add(new HeadingBlockParser());
            _parsers.Add(new ImageLineBlockParser());
            _parsers.Add(new ListBlockParser());
            _parsers.Add(new QuoteBlockParser());
            _parsers.Add(new ParagraphBlockParser()); // fallback
        }

        public HandbookParseResult Parse(string pageId, string rawMarkdown)
        {
            var result = new HandbookParseResult();
            if (string.IsNullOrEmpty(rawMarkdown))
                return result;

            var lines = SplitLines(rawMarkdown);
            var cursor = new LineCursor(lines);

            var anchorGen = new AnchorIdGenerator();
            var context = new BlockParseContext(
                anchorGen,
                _inlineParser,
                _linkRouter,
                inner => ParseInner(inner) // рекурсивный парсинг для цитат и прочего
            );

            while (!cursor.End)
            {
                // Пропускаем ведущие пустые строки между блоками
                while (!cursor.End && string.IsNullOrWhiteSpace(cursor.Current))
                    cursor.Advance();

                if (cursor.End) break;

                HandbookBlockBase block = null;
                var pos = cursor.Save();

                // Выбираем первый парсер, который может разобрать текущую позицию
                for (int i = 0; i < _parsers.Count; i++)
                {
                    var parser = _parsers[i];

                    // Важно: CanParse не должен менять состояние cursor
                    if (parser is ParagraphBlockParser) continue; // абзац как fallback
                    if (parser.CanParse(cursor))
                    {
                        block = parser.Parse(cursor, context);
                        break;
                    }
                }

                // Если ни один не сработал — пробуем абзац
                if (block == null)
                {
                    cursor.Restore(pos);
                    var paragraph = (ParagraphBlockParser)_parsers[_parsers.Count - 1];
                    if (paragraph.CanParse(cursor))
                        block = paragraph.Parse(cursor, context);
                    else
                        cursor.Advance(); // защитный сдвиг, чтобы не зациклиться
                }

                if (block != null)
                    result.Blocks.Add(block);
            }

            // Anchors из заголовков
            for (int i = 0; i < result.Blocks.Count; i++)
            {
                if (result.Blocks[i] is HeadingBlock h)
                {
                    // Якоря формируем из заголовков. level нужен для UI и оглавления.
                    result.Anchors.Add(new HandbookAnchor
                    {
                        id = h.anchorId,
                        title = GetPlainText(h.inlines),
                        level = h.level
                    });
                }
            }

            // Ссылки из инлайнов — отдельным визитором
            var collector = new LinkCollector(_linkRouter);
            result.Links.AddRange(collector.Collect(result.Blocks));

            return result;
        }

        private HandbookParseResult ParseInner(string raw)
        {
            // Приватный разбор вложенного текста тем же пайплайном
            return Parse("inner", raw);
        }

        private List<string> SplitLines(string text)
        {
            var lines = new List<string>();
            using var reader = new System.IO.StringReader(text);
            string line;
            while ((line = reader.ReadLine()) != null)
                lines.Add(line);
            return lines;
        }

        private string GetPlainText(List<HandbookInlineBase> inlines)
        {
            if (inlines == null || inlines.Count == 0)
                return string.Empty;

            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < inlines.Count; i++)
            {
                if (inlines[i] is TextRunInline t)
                    sb.Append(t.text);
                else if (inlines[i] is CodeSpanInline c)
                    sb.Append(c.text);
                else if (inlines[i] is LinkInline l)
                    sb.Append(GetPlainText(l.children));
            }

            return sb.ToString().Trim();
        }
    }
}