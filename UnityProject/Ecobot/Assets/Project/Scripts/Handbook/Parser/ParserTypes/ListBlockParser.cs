using System.Text.RegularExpressions;
using Handbook.Parser.BlockTypes;
using UnityEngine.Rendering;

namespace Handbook.Parser
{
    public class ListBlockParser : IHandbookBlockParser
    {
        private static readonly Regex _unordered = new(@"^(\s*)([-+*])\s+(.+)$", RegexOptions.Compiled);
    private static readonly Regex _ordered = new(@"^(\s*)(\d+)\.\s+(.+)$", RegexOptions.Compiled);

    public bool CanParse(LineCursor cursor)
    {
        var line = cursor.Current;
        if (line == null) return false;
        return _unordered.IsMatch(line.TrimStart()) || _ordered.IsMatch(line.TrimStart());
    }

    public HandbookBlockBase Parse(LineCursor cursor, BlockParseContext context)
    {
        var first = cursor.Current.TrimStart();
        var ordered = _ordered.IsMatch(first);
        var list = new ListBlock { ordered = ordered };

        while (!cursor.End)
        {
            var line = cursor.Current;
            if (line == null) break;

            var t = line.TrimStart();
            var m = ordered ? _ordered.Match(t) : _unordered.Match(t);
            if (!m.Success) break;

            var content = m.Groups[3].Value;

            var item = new ListItemBlock();
            // Основное содержимое пункта — абзац
            var p = new ParagraphBlock();
            p.inlines.AddRange(context.InlineParser.Parse(content));
            item.children.Add(p);

            cursor.Advance();

            // Продолжение пункта многострочным текстом:
            // читаем строки с отступом, пока не начнётся новый пункт/блок
            while (!cursor.End)
            {
                var cont = cursor.Current;
                if (string.IsNullOrWhiteSpace(cont))
                {
                    cursor.Advance();
                    continue;
                }

                var tt = cont.TrimStart();

                // Остановка, если начинается новый пункт
                var isNextItem = _unordered.IsMatch(tt) || _ordered.IsMatch(tt);
                if (isNextItem) break;

                // Остановка, если структурный блок (заголовок, код, цитата, hr, картинка)
                if (IsStructuralStart(tt)) break;

                var para = new ParagraphBlock();
                para.inlines.AddRange(context.InlineParser.Parse(tt.Trim()));
                item.children.Add(para);

                cursor.Advance();
            }

            list.items.Add(item);
        }

        return list;
    }

    private bool IsStructuralStart(string t)
    {
        if (Regex.IsMatch(t, @"^#{1,6}\s+")) return true;
        if (Regex.IsMatch(t, @"^\s*```")) return true;
        if (Regex.IsMatch(t, @"^\s*>\s")) return true;
        var hr = t.Trim();
        if (hr.Length >= 3)
        {
            if (hr.Replace("-", "").Trim().Length == 0) return true;
            if (hr.Replace("*", "").Trim().Length == 0) return true;
            if (hr.Replace("_", "").Trim().Length == 0) return true;
        }
        if (Regex.IsMatch(t, @"^!\[([^\]]*)\]\(([^)]+)\)\s*$")) return true;
        return false;
    }
    }
}