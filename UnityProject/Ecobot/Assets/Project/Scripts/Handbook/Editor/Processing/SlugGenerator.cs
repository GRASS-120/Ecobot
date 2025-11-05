using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Handbook.Editor.Processing
{
    public static class SlugGenerator
    {
        public static string ToSlug(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return "node";

            var s = title.Trim().ToLowerInvariant();
            s = TransliterateRuToLat(s);
            s = Regex.Replace(s, @"[^a-z0-9]+", "-");
            s = Regex.Replace(s, @"-+", "-");
            s = s.Trim('-');
            return string.IsNullOrEmpty(s) ? "node" : s;
        }

        private static string TransliterateRuToLat(string s)
        {
            var map = new Dictionary<char, string>
            {
                ['а']="a",['б']="b",['в']="v",['г']="g",['д']="d",['е']="e",['ё']="e",['ж']="zh",['з']="z",
                ['и']="i",['й']="y",['к']="k",['л']="l",['м']="m",['н']="n",['о']="o",['п']="p",['р']="r",
                ['с']="s",['т']="t",['у']="u",['ф']="f",['х']="h",['ц']="c",['ч']="ch",['ш']="sh",['щ']="sch",
                ['ъ']="", ['ы']="y",['ь']="", ['э']="e",['ю']="yu",['я']="ya"
            };

            var sb = new StringBuilder(s.Length * 2);
            for (int i = 0; i < s.Length; i++)
            {
                var ch = s[i];
                if (map.TryGetValue(ch, out var repl))
                    sb.Append(repl);
                else
                    sb.Append(ch);
            }
            return sb.ToString();
        }
    }
}