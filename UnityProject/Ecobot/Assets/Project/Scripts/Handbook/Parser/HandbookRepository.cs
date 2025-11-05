using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Handbook.ContentProvider;
using Handbook.Models;

namespace Handbook.Parser
{
    public class HandbookRepository : IHandbookRepository
    {
        public HandbookManifest Manifest => _manifest;
        public string Language => _language;

        private IHandbookContentProvider _provider;
        private IHandbookMarkdownParser _parser;

        private HandbookManifest _manifest;
        private string _language;

        private readonly Dictionary<string, HandbookPageRef> _pagesIndex = new();
        private readonly Dictionary<string, string> _redirects = new();
        private readonly Dictionary<string, CacheEntry> _cache = new();

        private readonly object _sync = new();

        public async Task InitializeAsync(IHandbookContentProvider provider, IHandbookMarkdownParser parser, CancellationToken ct = default)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            if (parser == null) throw new ArgumentNullException(nameof(parser));

            _provider = provider;
            _parser = parser;

            _manifest = await _provider.LoadManifestAsync(ct);
            if (_manifest == null) throw new InvalidOperationException("Handbook manifest is null");

            _language = string.IsNullOrWhiteSpace(_manifest.language) ? "ru" : _manifest.language.Trim();

            RebuildIndexes();
            InvalidateCacheAll();
        }

        public async Task RefreshManifestAsync(CancellationToken ct = default)
        {
            EnsureInitialized();

            var newManifest = await _provider.LoadManifestAsync(ct);
            if (newManifest == null) throw new InvalidOperationException("Handbook manifest is null");

            var oldIndex = new Dictionary<string, string>(_pagesIndex.Count);
            foreach (var kv in _pagesIndex)
                oldIndex[kv.Key] = GetVersionToken(kv.Value);

            _manifest = newManifest;
            _language = string.IsNullOrWhiteSpace(_manifest.language) ? "ru" : _manifest.language.Trim();

            RebuildIndexes();

            // Инвалидация кэша: если токен версии изменился или страницы больше нет — удалить
            var toRemove = new List<string>();
            lock (_sync)
            {
                foreach (var kv in _cache)
                {
                    var pageId = kv.Key;
                    if (!_pagesIndex.TryGetValue(pageId, out var newRef))
                    {
                        toRemove.Add(pageId);
                        continue;
                    }

                    var newToken = GetVersionToken(newRef);
                    if (!oldIndex.TryGetValue(pageId, out var oldToken))
                    {
                        toRemove.Add(pageId);
                        continue;
                    }

                    // Если токен отсутствует — безопаснее сбросить кэш
                    if (string.IsNullOrEmpty(newToken) || string.IsNullOrEmpty(oldToken) || !string.Equals(newToken, oldToken, StringComparison.Ordinal))
                        toRemove.Add(pageId);
                }

                for (int i = 0; i < toRemove.Count; i++)
                    _cache.Remove(toRemove[i]);
            }
        }

        public bool TryGetPageRef(string pageId, out HandbookPageRef pageRef)
        {
            pageId = NormalizeId(pageId);
            if (string.IsNullOrEmpty(pageId))
            {
                pageRef = null;
                return false;
            }

            pageId = ResolvePageId(pageId);
            return _pagesIndex.TryGetValue(pageId, out pageRef);
        }

        public bool TryResolveRedirect(string idOrAlias, out string resolvedPageId)
        {
            idOrAlias = NormalizeId(idOrAlias);
            if (string.IsNullOrEmpty(idOrAlias))
            {
                resolvedPageId = null;
                return false;
            }

            if (_redirects.TryGetValue(idOrAlias, out var to))
            {
                resolvedPageId = to;
                return true;
            }

            // Нет редиректа — считаем, что id уже канонический
            resolvedPageId = idOrAlias;
            return true;
        }

        public bool PageExists(string pageId)
        {
            pageId = NormalizeId(pageId);
            if (string.IsNullOrEmpty(pageId))
                return false;

            pageId = ResolvePageId(pageId);
            return _pagesIndex.ContainsKey(pageId);
        }

        public async Task<HandbookPage> LoadPageAsync(string pageId, CancellationToken ct = default)
        {
            EnsureInitialized();

            pageId = NormalizeId(pageId);
            if (string.IsNullOrEmpty(pageId))
                throw new ArgumentException("Page id must not be empty.", nameof(pageId));

            pageId = ResolvePageId(pageId);

            if (!_pagesIndex.TryGetValue(pageId, out var pageRef))
                throw new KeyNotFoundException($"Handbook page not found in manifest: {pageId}");

            var token = GetVersionToken(pageRef);

            // Вернуть из кэша, если версия совпадает
            lock (_sync)
            {
                if (_cache.TryGetValue(pageId, out var entry) && string.Equals(entry.Hash, token, StringComparison.Ordinal))
                    return entry.Page;
            }

            var fileKey = !string.IsNullOrWhiteSpace(pageRef.filePath)
                ? pageRef.filePath
                : (!string.IsNullOrWhiteSpace(pageRef.fileName) ? pageRef.fileName : pageId);

            var raw = await _provider.LoadPageMarkdownAsync(fileKey, _language, ct);
            
            // Парсим markdown → AST/anchors/links
            var parsed = _parser.Parse(pageId, raw);

            var page = new HandbookPage
            {
                id = pageId,
                title = pageRef.title,
                rawMarkdown = raw,
                meta = new HandbookPageMeta
                {
                    tags = pageRef.tags,
                    updatedAt = pageRef.updatedAt,
                    hidden = pageRef.hidden
                },
                anchors = parsed.Anchors,
                links = parsed.Links,
                blocks = parsed.Blocks
            };

            lock (_sync)
            {
                _cache[pageId] = new CacheEntry { Page = page, Hash = token };
            }

            return page;
        }

        public IReadOnlyList<HandbookSection> EnumerateSections()
        {
            EnsureInitialized();
            return _manifest.sections;
        }

        public IReadOnlyList<HandbookPageRef> EnumerateAllPages(bool includeHidden = false)
        {
            EnsureInitialized();

            var list = new List<HandbookPageRef>(capacity: _pagesIndex.Count);
            foreach (var section in _manifest.sections)
                CollectPages(section, list, includeHidden);

            return list;
        }

        public string BuildMediaPath(string relativePath)
        {
            EnsureInitialized();

            if (string.IsNullOrWhiteSpace(relativePath))
                return _provider.BuildMediaPath(_manifest.mediaBasePath ?? string.Empty);

            // Если относительный путь не содержит базу — добавляем её
            var rel = relativePath;
            if (!string.IsNullOrWhiteSpace(_manifest.mediaBasePath))
            {
                // Не дублировать базу, если пользователь уже указал её в ссылке
                var normalized = relativePath.Replace('\\', '/');
                var baseNorm = _manifest.mediaBasePath.Replace('\\', '/');

                if (!normalized.StartsWith(baseNorm, StringComparison.OrdinalIgnoreCase))
                    rel = Path.Combine(_manifest.mediaBasePath, relativePath);
            }

            rel = rel.Replace('\\', '/');
            return _provider.BuildMediaPath(rel);
        }

        public void Dispose()
        {
            lock (_sync)
            {
                _cache.Clear();
            }

            _pagesIndex.Clear();
            _redirects.Clear();

            _manifest = null;
            _provider = null;
            _parser = null;
            _language = null;
        }

        private void RebuildIndexes()
        {
            _pagesIndex.Clear();
            _redirects.Clear();

            if (_manifest.redirects != null)
            {
                for (int i = 0; i < _manifest.redirects.Count; i++)
                {
                    var r = _manifest.redirects[i];
                    var from = NormalizeId(r.from);
                    var to = NormalizeId(r.to);
                    if (!string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(to))
                        _redirects[from] = to;
                }
            }

            if (_manifest.sections != null)
            {
                for (int i = 0; i < _manifest.sections.Count; i++)
                    IndexSection(_manifest.sections[i]);
            }
        }

        private void IndexSection(HandbookSection section)
        {
            if (section.pages != null)
            {
                for (int i = 0; i < section.pages.Count; i++)
                {
                    var p = section.pages[i];
                    var id = NormalizeId(p.id);
                    if (!string.IsNullOrEmpty(id))
                        _pagesIndex[id] = p; // последний побеждает; валидатор поймает дубликаты на шаге 6
                }
            }

            if (section.children != null)
            {
                for (int i = 0; i < section.children.Count; i++)
                    IndexSection(section.children[i]);
            }
        }

        private void CollectPages(HandbookSection section, List<HandbookPageRef> sink, bool includeHidden)
        {
            if (section.pages != null)
            {
                for (int i = 0; i < section.pages.Count; i++)
                {
                    var p = section.pages[i];
                    if (!p.hidden || includeHidden)
                        sink.Add(p);
                }
            }

            if (section.children != null)
            {
                for (int i = 0; i < section.children.Count; i++)
                    CollectPages(section.children[i], sink, includeHidden);
            }
        }

        private void InvalidateCacheAll()
        {
            lock (_sync)
                _cache.Clear();
        }

        private string ResolvePageId(string pageId)
        {
            if (_redirects.TryGetValue(pageId, out var to))
                return to;
            return pageId;
        }

        private string NormalizeId(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();
        }

        private string GetVersionToken(HandbookPageRef pageRef)
        {
            // Предпочитаем hash; если нет — используем updatedAt; если нет — пусто (тогда кэш будет инвалидирован при Refresh)
            if (!string.IsNullOrWhiteSpace(pageRef.hash))
                return pageRef.hash.Trim();

            if (!string.IsNullOrWhiteSpace(pageRef.updatedAt))
                return pageRef.updatedAt.Trim();

            return string.Empty;
        }

        private void EnsureInitialized()
        {
            if (_provider == null || _parser == null || _manifest == null)
                throw new InvalidOperationException("HandbookRepository is not initialized. Call InitializeAsync first.");
        }

        private sealed class CacheEntry
        {
            public HandbookPage Page;
            public string Hash;
        }
    }
}