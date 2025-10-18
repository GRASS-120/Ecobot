using Handbook.Parser.BlockTypes;

namespace Handbook.Parser
{
    public class HorizontalRuleBlockParser : IHandbookBlockParser
    {
        public bool CanParse(LineCursor cursor)
        {
            var t = cursor.Current?.Trim();
            if (string.IsNullOrEmpty(t) || t.Length < 3) return false;

            // HR если строка целиком состоит из одного из символов и их 3+
            if (Strip(t, '-') == 0) return true;
            if (Strip(t, '*') == 0) return true;
            if (Strip(t, '_') == 0) return true;

            return false;
        }

        public HandbookBlockBase Parse(LineCursor cursor, BlockParseContext context)
        {
            cursor.Advance();
            return new HorizontalRuleBlock();
        }

        private int Strip(string s, char ch)
        {
            // Возвращает количество символов, отличных от ch
            int not = 0;
            for (int i = 0; i < s.Length; i++)
                if (s[i] != ch && !char.IsWhiteSpace(s[i]))
                    not++;
            return not;
        }
    }
}