using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Handbook.Parser
{
    public class AnchorIdGenerator
    {
        private readonly Dictionary<string, int> _counts = new();

        public string Create(string headingText)
        {
            var baseId = Slugify(headingText);
            if (string.IsNullOrEmpty(baseId))
                baseId = "section";

            if (!_counts.TryGetValue(baseId, out var count))
            {
                _counts[baseId] = 1;
                return baseId;
            }

            count++;
            _counts[baseId] = count;
            return $"{baseId}-{count}";
        }

        private string Slugify(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var s = text.Trim().ToLowerInvariant();
            s = TransliterateRuToLat(s);

            s = Regex.Replace(s, @"[^a-z0-9]+", "-");
            s = Regex.Replace(s, @"-+", "-");
            s = s.Trim('-');

            return s;
        }

        private string TransliterateRuToLat(string s)
        {
            var map = new Dictionary<char, string>
            {
                ['а']="a",['б']="b",['в']="v",['г']="g",['д']="d",['е']="e",['ё']="e",['ж']="zh",['з']="z",
                ['и']="i",['й']="y",['к']="k",['л']="l",['м']="m",['н']="n",['о']="o",['п']="p",['р']="r",
                ['с']="s",['т']="t",['у']="u",['ф']="f",['х']="h",['ц']="c",['ч']="ch",['ш']="sh",['щ']="sch",
                ['ъ']="", ['ы']="y",['ь']="", ['э']="e",['ю']="yu",['я']="ya"
            };

            var sb = new StringBuilder(s.Length * 2);
            foreach (var ch in s)
            {
                if (map.TryGetValue(ch, out var repl))
                {
                    sb.Append(repl);
                }
                else
                {
                    sb.Append(ch);
                }
            }
            return sb.ToString();
        }
    }
}