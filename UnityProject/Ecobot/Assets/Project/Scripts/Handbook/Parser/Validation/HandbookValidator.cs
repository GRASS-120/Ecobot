using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Handbook.Models;

namespace Handbook.Parser.Validation
{
    public class HandbookValidator : IHandbookValidator
    {
        public async Task<HandbookValidationReport> ValidateAllAsync(IHandbookRepository repo, CancellationToken ct = default)
        {
            if (repo == null) throw new ArgumentNullException(nameof(repo));

            var report = new HandbookValidationReport();

            ValidateManifestStructure(repo, report);
            ValidateRedirects(repo, report);

            // Проверяем наличие и содержимое всех страниц
            var pages = repo.EnumerateAllPages(includeHidden: true);
            var anchorsCache = new Dictionary<string, HashSet<string>>(pages.Count);

            for (int i = 0; i < pages.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                var pageRef = pages[i];
                var issues = await ValidateSinglePageInternal(repo, pageRef.id, anchorsCache, ct);
                report.AddRange(issues);
            }

            return report;
        }

        public async Task<HandbookValidationReport> ValidatePageAsync(IHandbookRepository repo, string pageId, CancellationToken ct = default)
        {
            if (repo == null) throw new ArgumentNullException(nameof(repo));

            var report = new HandbookValidationReport();
            var anchorsCache = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

            var issues = await ValidateSinglePageInternal(repo, pageId, anchorsCache, ct);
            report.AddRange(issues);

            return report;
        }

        private void ValidateManifestStructure(IHandbookRepository repo, HandbookValidationReport report)
        {
            var manifest = repo.Manifest;
            var pageIdCounts = new Dictionary<string, int>(StringComparer.Ordinal);

            // Собираем id страниц и помечаем дубли (HB003)
            var allPages = repo.EnumerateAllPages(includeHidden: true);
            for (int i = 0; i < allPages.Count; i++)
            {
                var id = Normalize(allPages[i].id);
                if (string.IsNullOrEmpty(id)) continue;

                if (!pageIdCounts.TryGetValue(id, out var count))
                    pageIdCounts[id] = 1;
                else
                    pageIdCounts[id] = count + 1;
            }

            foreach (var kv in pageIdCounts)
            {
                if (kv.Value > 1)
                {
                    report.Add(new HandbookValidationIssue(
                        HandbookValidationSeverity.Error,
                        "HB003",
                        $"Дублирующийся pageId в манифесте: {kv.Key}"
                    ));
                }
            }

            // defaultPageId должен существовать (HB001) и не быть скрытым (HB002 - warning)
            var defaultId = Normalize(manifest.defaultPageId);
            if (!string.IsNullOrEmpty(defaultId))
            {
                if (!repo.PageExists(defaultId))
                {
                    report.Add(new HandbookValidationIssue(
                        HandbookValidationSeverity.Error,
                        "HB001",
                        $"defaultPageId указывает на несуществующую страницу: {manifest.defaultPageId}"
                    ));
                }
                else
                {
                    if (repo.TryGetPageRef(defaultId, out var defRef) && defRef.hidden)
                    {
                        report.Add(new HandbookValidationIssue(
                            HandbookValidationSeverity.Warning,
                            "HB002",
                            $"defaultPageId ('{manifest.defaultPageId}') помечена hidden"
                        ));
                    }
                }
            }
        }

        private void ValidateRedirects(IHandbookRepository repo, HandbookValidationReport report)
        {
            var manifest = repo.Manifest;
            if (manifest.redirects == null || manifest.redirects.Count == 0)
                return;

            // redirect.to должен вести на существующую страницу (HB011)
            for (int i = 0; i < manifest.redirects.Count; i++)
            {
                var r = manifest.redirects[i];
                var to = Normalize(r.to);
                if (string.IsNullOrEmpty(to) || !repo.PageExists(to))
                {
                    report.Add(new HandbookValidationIssue(
                        HandbookValidationSeverity.Error,
                        "HB011",
                        $"Редирект ведёт на несуществующую страницу: '{r.from}' -> '{r.to}'"
                    ));
                }
            }

            // Поиск циклов A->B->A... (HB012)
            var graph = new Dictionary<string, string>(StringComparer.Ordinal);
            for (int i = 0; i < manifest.redirects.Count; i++)
            {
                var r = manifest.redirects[i];
                var from = Normalize(r.from);
                var to = Normalize(r.to);
                if (string.IsNullOrEmpty(from) || string.IsNullOrEmpty(to))
                    continue;

                graph[from] = to;
            }

            var visited = new HashSet<string>(StringComparer.Ordinal);
            var stack = new HashSet<string>(StringComparer.Ordinal);

            foreach (var start in graph.Keys)
            {
                if (DetectCycle(start, graph, visited, stack))
                {
                    report.Add(new HandbookValidationIssue(
                        HandbookValidationSeverity.Error,
                        "HB012",
                        $"Обнаружен цикл в редиректах начиная с: {start}"
                    ));
                }
            }
        }

        private async Task<List<HandbookValidationIssue>> ValidateSinglePageInternal(
            IHandbookRepository repo,
            string pageId,
            Dictionary<string, HashSet<string>> anchorsCache,
            CancellationToken ct)
        {
            var issues = new List<HandbookValidationIssue>();

            // Проверяем наличие файла и парсинг. Если файла нет — HB020.
            HandbookPage page = null;
            try
            {
                page = await repo.LoadPageAsync(pageId, ct);
            }
            catch (FileNotFoundException)
            {
                issues.Add(new HandbookValidationIssue(
                    HandbookValidationSeverity.Error,
                    "HB020",
                    "Файл страницы не найден",
                    pageId: pageId
                ));
                return issues;
            }

            // Пустая страница после Trim — HB021 (warning)
            if (string.IsNullOrWhiteSpace(page.rawMarkdown))
            {
                issues.Add(new HandbookValidationIssue(
                    HandbookValidationSeverity.Warning,
                    "HB021",
                    "Пустая страница",
                    pageId: pageId
                ));
            }

            // Кэшируем якоря страницы для последующей проверки ссылок
            CacheAnchors(page, anchorsCache);

            // Проверяем ссылки внутри страницы
            await ValidateLinksOnPage(repo, page, anchorsCache, issues, ct);

            return issues;
        }

        private async Task ValidateLinksOnPage(
            IHandbookRepository repo,
            HandbookPage page,
            Dictionary<string, HashSet<string>> anchorsCache,
            List<HandbookValidationIssue> sink,
            CancellationToken ct)
        {
            if (page.links == null || page.links.Count == 0)
                return;

            for (int i = 0; i < page.links.Count; i++)
            {
                ct.ThrowIfCancellationRequested();

                var link = page.links[i];
                var kind = link.kind?.Trim().ToLowerInvariant();

                if (kind == "handbook")
                {
                    var targetId = Normalize(link.pageId);
                    if (string.IsNullOrEmpty(targetId) || !repo.PageExists(targetId))
                    {
                        sink.Add(new HandbookValidationIssue(
                            HandbookValidationSeverity.Error,
                            "HB030",
                            $"Ссылка на несуществующую страницу: {link.url}",
                            pageId: page.id,
                            linkUrl: link.url
                        ));
                        continue;
                    }

                    // Проверка якоря, если указан: лениво загружаем целевую страницу и кэшируем её якоря
                    var anchor = Normalize(link.anchor);
                    if (!string.IsNullOrEmpty(anchor))
                    {
                        if (!anchorsCache.TryGetValue(targetId, out var set))
                        {
                            var targetPage = await repo.LoadPageAsync(targetId, ct);
                            CacheAnchors(targetPage, anchorsCache);
                            set = anchorsCache[targetId];
                        }

                        if (!set.Contains(anchor))
                        {
                            sink.Add(new HandbookValidationIssue(
                                HandbookValidationSeverity.Error,
                                "HB031",
                                $"Ссылка на несуществующий якорь: {link.url}",
                                pageId: page.id,
                                anchor: link.anchor,
                                linkUrl: link.url
                            ));
                        }
                    }
                }
                else if (kind == "tutorial")
                {
                    // Туториал ещё не интегрирован — предупреждаем, чтобы позже связать stepId
                    sink.Add(new HandbookValidationIssue(
                        HandbookValidationSeverity.Warning,
                        "HB032",
                        $"Ссылка на шаг туториала требует проверки: {link.stepId}",
                        pageId: page.id,
                        stepId: link.stepId,
                        linkUrl: link.url
                    ));
                }
                else if (kind == "external")
                {
                    // Базовая проверка корректности URL
                    if (string.IsNullOrWhiteSpace(link.url) || !IsValidHttpUrl(link.url))
                    {
                        sink.Add(new HandbookValidationIssue(
                            HandbookValidationSeverity.Warning,
                            "HB033",
                            "Подозрительный внешний URL",
                            pageId: page.id,
                            linkUrl: link.url
                        ));
                    }
                }
            }
        }

        private void CacheAnchors(HandbookPage page, Dictionary<string, HashSet<string>> anchorsCache)
        {
            if (page == null) return;

            if (!anchorsCache.ContainsKey(page.id))
            {
                var set = new HashSet<string>(StringComparer.Ordinal);
                if (page.anchors != null)
                {
                    for (int i = 0; i < page.anchors.Count; i++)
                    {
                        // Валидация по якорям идёт в формате [a-z0-9-] — id уже сгенерирован парсером
                        var a = Normalize(page.anchors[i].id);
                        if (!string.IsNullOrEmpty(a))
                            set.Add(a);
                    }
                }
                anchorsCache[page.id] = set;
            }
        }

        private bool DetectCycle(string start, Dictionary<string, string> graph, HashSet<string> visited, HashSet<string> stack)
        {
            // DFS по направленным рёбрам редиректов. stack — текущий путь; если попадаем в уже посещённую вершину из stack, есть цикл.
            if (visited.Contains(start))
                return false;

            visited.Add(start);
            stack.Add(start);

            if (graph.TryGetValue(start, out var to))
            {
                if (stack.Contains(to))
                    return true;

                if (DetectCycle(to, graph, visited, stack))
                    return true;
            }

            stack.Remove(start);
            return false;
        }

        private string Normalize(string s)
        {
            return string.IsNullOrWhiteSpace(s) ? null : s.Trim().ToLowerInvariant();
        }

        private bool IsValidHttpUrl(string url)
        {
            // Простейшая проверка абсолютного http/https URL без сетевых вызовов
            if (!Uri.TryCreate(url, UriKind.Absolute, out var u))
                return false;

            if (!string.Equals(u.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(u.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
                return false;

            return true;
        }
    }
}