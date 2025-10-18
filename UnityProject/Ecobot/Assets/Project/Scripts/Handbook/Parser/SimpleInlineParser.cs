using System;
using System.Collections.Generic;
using Handbook.Parser.InlineTypes;

namespace Handbook.Parser
{
    public class SimpleInlineParser : IHandbookInlineParser
    {
        public List<HandbookInlineBase> Parse(string text)
    {
        var inlines = new List<HandbookInlineBase>();
        if (string.IsNullOrEmpty(text))
            return inlines;

        int i = 0;
        var buf = new System.Text.StringBuilder();

        void FlushText()
        {
            if (buf.Length > 0)
            {
                inlines.Add(new TextRunInline { text = buf.ToString() });
                buf.Clear();
            }
        }

        while (i < text.Length)
        {
            var ch = text[i];

            // Кодовый спан: `...`
            if (ch == '`')
            {
                FlushText();

                // Ищем следующую обратную кавычку. Если её нет — берём до конца строки.
                int j = text.IndexOf('`', i + 1);
                if (j < 0) j = text.Length;
                var code = text.Substring(i + 1, Math.Max(0, j - i - 1));

                inlines.Add(new CodeSpanInline { text = code });
                i = j + 1;
                continue;
            }

            // Ссылка: [label](url)
            if (ch == '[')
            {
                int closeBracket = text.IndexOf(']', i + 1);
                if (closeBracket > i)
                {
                    // Требуем немедленно следующую '(' — иначе считаем это текстом
                    if (closeBracket + 1 < text.Length && text[closeBracket + 1] == '(')
                    {
                        int openParen = closeBracket + 1;

                        // Поиск закрывающей ')' без вложенных скобок (v1)
                        int depth = 0;
                        int k = openParen + 1;
                        int closeParen = -1;

                        for (; k < text.Length; k++)
                        {
                            if (text[k] == '(') depth++;
                            else if (text[k] == ')')
                            {
                                if (depth == 0)
                                {
                                    closeParen = k;
                                    break;
                                }
                                depth--;
                            }
                        }

                        if (closeParen > 0)
                        {
                            FlushText();

                            var label = text.Substring(i + 1, closeBracket - i - 1);
                            var url = text.Substring(openParen + 1, closeParen - openParen - 1);

                            var link = new LinkInline { url = url.Trim() };
                            if (!string.IsNullOrEmpty(label))
                                link.children.Add(new TextRunInline { text = label });

                            inlines.Add(link);
                            i = closeParen + 1;
                            continue;
                        }
                    }
                }
            }

            buf.Append(ch);
            i++;
        }

        FlushText();
        return inlines;
    }
    }
}