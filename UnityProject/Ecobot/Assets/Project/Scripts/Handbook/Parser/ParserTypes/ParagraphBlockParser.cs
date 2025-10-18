using System.Text.RegularExpressions;
using Handbook.Parser.BlockTypes;

namespace Handbook.Parser
{
    public class ParagraphBlockParser : IHandbookBlockParser
    {
        public bool CanParse(LineCursor cursor)
    {
        var line = cursor.Current;
        if (line == null) return false;
        if (string.IsNullOrWhiteSpace(line)) return false;

        // Не начинать абзац, если начинается любой структурный блок
        var t = line.TrimStart();
        if (Regex.IsMatch(t, @"^\s*```")) return false;
        if (Regex.IsMatch(t, @"^#{1,6}\s+")) return false;
        if (Regex.IsMatch(t, @"^\s*>\s")) return false;
        if (Regex.IsMatch(t, @"^(\*|\-|\+)\s+")) return false;
        if (Regex.IsMatch(t, @"^\d+\.\s+")) return false;
        if (Regex.IsMatch(t, @"^!\[([^\]]*)\]\(([^)]+)\)\s*$")) return false;

        var hr = line.Trim();
        if (hr.Length >= 3)
        {
            if (hr.Replace("-", "").Trim().Length == 0) return false;
            if (hr.Replace("*", "").Trim().Length == 0) return false;
            if (hr.Replace("_", "").Trim().Length == 0) return false;
        }

        return true;
    }

    public HandbookBlockBase Parse(LineCursor cursor, BlockParseContext context)
    {
        var buf = new System.Text.StringBuilder();

        // Абзац идёт до пустой строки или начала структурного блока
        while (!cursor.End && !string.IsNullOrWhiteSpace(cursor.Current))
        {
            var t = cursor.Current;

            if (IsStructuralStart(t)) break;

            if (buf.Length > 0) buf.AppendLine();
            buf.Append(t);
            cursor.Advance();
        }

        // Пропустить пустые строки между блоками
        while (!cursor.End && string.IsNullOrWhiteSpace(cursor.Current))
            cursor.Advance();

        var p = new ParagraphBlock();
        p.inlines.AddRange(context.InlineParser.Parse(buf.ToString().Trim()));
        return p;
    }

    private bool IsStructuralStart(string line)
    {
        var t = line.TrimStart();
        if (Regex.IsMatch(t, @"^\s*```")) return true;
        if (Regex.IsMatch(t, @"^#{1,6}\s+")) return true;
        if (Regex.IsMatch(t, @"^\s*>\s")) return true;
        if (Regex.IsMatch(t, @"^(\*|\-|\+)\s+")) return true;
        if (Regex.IsMatch(t, @"^\d+\.\s+")) return true;

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