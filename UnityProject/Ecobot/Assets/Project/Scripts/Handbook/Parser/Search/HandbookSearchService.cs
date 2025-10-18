using System;
using System.Collections.Generic;

namespace Handbook.Parser.Search
{
    public class HandbookSearchService : IHandbookSearchService
    {
        public IReadOnlyList<HandbookSearchResult> Query(IHandbookRepository repo, string text, bool includeHidden = false, int limit = 50)
        {
            if (repo == null) throw new ArgumentNullException(nameof(repo));

            var query = Normalize(text);
            if (string.IsNullOrEmpty(query))
                return Array.Empty<HandbookSearchResult>();

            var tokens = Tokenize(query);
            var pages = repo.EnumerateAllPages(includeHidden);
            var results = new List<HandbookSearchResult>(Math.Min(pages.Count, limit));

            for (int i = 0; i < pages.Count; i++)
            {
                var page = pages[i];
                if (!includeHidden && page.hidden)
                    continue;

                var title = (page.title ?? string.Empty).Trim();
                var titleNorm = Normalize(title);

                float score = 0f;
                string snippet = null;

                // Полное совпадение всей строки запроса в заголовке
                if (!string.IsNullOrEmpty(titleNorm) && titleNorm.Contains(query))
                {
                    score += 3f;
                    snippet ??= $"В заголовке: {title}";
                }

                // Частичные совпадения по токенам в заголовке
                for (int t = 0; t < tokens.Count; t++)
                {
                    if (!string.IsNullOrEmpty(titleNorm) && titleNorm.Contains(tokens[t]))
                    {
                        score += 1f;
                        snippet ??= $"В заголовке: {title}";
                    }
                }

                // Совпадения в тегах
                var tags = page.tags;
                if (tags != null && tags.Length > 0 && tokens.Count > 0)
                {
                    for (int j = 0; j < tags.Length; j++)
                    {
                        var tag = tags[j] ?? string.Empty;
                        var tagNorm = Normalize(tag);
                        if (string.IsNullOrEmpty(tagNorm))
                            continue;

                        for (int t = 0; t < tokens.Count; t++)
                        {
                            if (tagNorm.Contains(tokens[t]))
                            {
                                score += 0.5f;
                                snippet ??= $"Тег: {tag}";
                            }
                        }
                    }
                }

                if (score <= 0f)
                    continue;

                results.Add(new HandbookSearchResult(page, score, snippet));
            }

            results.Sort(CompareResults);

            if (results.Count > limit)
                results.RemoveRange(limit, results.Count - limit);

            return results;
        }

        private int CompareResults(HandbookSearchResult a, HandbookSearchResult b)
        {
            // Сначала по убыванию Score, затем по заголовку по возрастанию
            var byScore = -a.Score.CompareTo(b.Score);
            if (byScore != 0)
                return byScore;

            var at = a.Page?.title ?? string.Empty;
            var bt = b.Page?.title ?? string.Empty;
            return string.Compare(at, bt, StringComparison.OrdinalIgnoreCase);
        }

        private string Normalize(string s)
        {
            return string.IsNullOrWhiteSpace(s) ? null : s.Trim().ToLowerInvariant();
        }

        private List<string> Tokenize(string query)
        {
            var list = new List<string>();
            var parts = query.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                var p = parts[i].Trim();
                if (p.Length > 0)
                    list.Add(p.ToLowerInvariant());
            }
            return list;
        }
    }
}