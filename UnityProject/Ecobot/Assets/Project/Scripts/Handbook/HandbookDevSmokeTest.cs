using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Handbook.ContentProvider;
using Handbook.Models;
using Handbook.Parser;
using Handbook.Parser.BlockTypes;
using Handbook.Parser.InlineTypes;
using Handbook.Parser.Validation;
using Handbook.Routing;
using UnityEngine;

namespace Handbook
{
    public class HandbookDevSmokeTest : MonoBehaviour
    {
        [Header("Source")]
        [SerializeField] private string _relativeRoot = "Handbook";
        [SerializeField] private string _language = "ru";

        [Header("Options")]
        [SerializeField] private bool _includeHiddenInLogs = true;
        [SerializeField] private int _maxIssuesToPrint = 50;

        private IHandbookContentProvider _provider;
        private IHandbookLinkRouter _router;
        private IHandbookMarkdownParser _parser;
        private IHandbookRepository _repo;
        private IHandbookValidator _validator;

        [ContextMenu("Run Smoke Test")]
        public async void RunSmokeTest()
        {
            try
            {
                await InitializeAsync();

                LogManifest();
                LogTree();

                var pages = _repo.EnumerateAllPages(includeHidden: _includeHiddenInLogs);
                foreach (var p in pages)
                    await LoadAndLogPageAsync(p.id);

                await RunValidationAsync();
                Debug.Log("[HandbookSmoke] DONE");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[HandbookSmoke] ERROR: {ex}");
            }
        }

        private async Task InitializeAsync()
        {
            var root = Path.Combine(Application.streamingAssetsPath, _relativeRoot);
            _provider = new FileSystemHandbookContentProvider(root);

            _router = new HandbookLinkRouter();
            _parser = new HandbookMarkdownParser(_router);

            _repo = new HandbookRepository();
            await _repo.InitializeAsync(_provider, _parser, CancellationToken.None);

            // Переопределяем язык, если нужно
            if (!string.IsNullOrWhiteSpace(_language) && !string.Equals(_repo.Language, _language, StringComparison.OrdinalIgnoreCase))
            {
                // В v1 язык задаётся манифестом; для smoke — просто используем _language при загрузке страниц
                // (репозиторий читает язык из манифеста, но провайдеру мы передаём _language здесь в LoadAndLogPageAsync)
            }

            _validator = new HandbookValidator();
        }

        private void LogManifest()
        {
            var m = _repo.Manifest;
            Debug.Log($"[HandbookSmoke] Manifest: version='{m.version}', language='{m.language}', mediaBasePath='{m.mediaBasePath}', defaultPageId='{m.defaultPageId}'");
        }

        private void LogTree()
        {
            var sections = _repo.EnumerateSections();
            Debug.Log($"[HandbookSmoke] Sections: {sections.Count}");
            for (int i = 0; i < sections.Count; i++)
                LogSectionRecursive(sections[i], 0);
        }

        private void LogSectionRecursive(HandbookSection s, int level)
        {
            var indent = new string(' ', level * 2);
            Debug.Log($"{indent}- Section '{s.title}' (id='{s.id}')");

            if (s.pages != null)
            {
                for (int i = 0; i < s.pages.Count; i++)
                {
                    var p = s.pages[i];
                    Debug.Log($"{indent}  • Page '{p.title}' (id='{p.id}', hidden={p.hidden})");
                }
            }

            if (s.children != null)
            {
                for (int i = 0; i < s.children.Count; i++)
                    LogSectionRecursive(s.children[i], level + 1);
            }
        }

        private async Task LoadAndLogPageAsync(string pageId)
        {
            // Загружаем и парсим страницу
            var page = await _repo.LoadPageAsync(pageId, CancellationToken.None);

            Debug.Log($"[HandbookSmoke] Page: id='{page.id}', title='{page.title}'");

            // Anchors (из заголовков)
            if (page.anchors != null && page.anchors.Count > 0)
            {
                for (int i = 0; i < page.anchors.Count; i++)
                {
                    var a = page.anchors[i];
                    Debug.Log($"[HandbookSmoke]   Anchor: id='{a.id}', level={a.level}, title='{a.title}'");
                }
            }

            // Links (результат парсинга инлайнов + роутер)
            if (page.links != null && page.links.Count > 0)
            {
                for (int i = 0; i < page.links.Count; i++)
                {
                    var l = page.links[i];
                    Debug.Log($"[HandbookSmoke]   Link: kind='{l.kind}', url='{l.url}', pageId='{l.pageId}', anchor='{l.anchor}', stepId='{l.stepId}'");
                }
            }

            // Blocks (это и есть «стили» на логическом уровне)
            if (page.blocks != null && page.blocks.Count > 0)
            {
                for (int i = 0; i < page.blocks.Count; i++)
                {
                    var b = page.blocks[i];
                    LogBlockSummary(b);
                }
            }
        }

        private void LogBlockSummary(HandbookBlockBase block)
        {
            switch (block)
            {
                case HeadingBlock h:
                    Debug.Log($"[HandbookSmoke]   Block: Heading h{h.level} '{ExtractInlineText(h.inlines)}' (anchor='{h.anchorId}')");
                    break;

                case ParagraphBlock p:
                    Debug.Log($"[HandbookSmoke]   Block: Paragraph '{ExtractInlineText(p.inlines)}'");
                    break;

                case CodeBlock c:
                    var firstLine = FirstLine(c.code);
                    Debug.Log($"[HandbookSmoke]   Block: Code lang='{c.language}' preview='{firstLine}'");
                    break;

                case ImageBlock img:
                    var mediaPath = _repo.BuildMediaPath(img.src);
                    Debug.Log($"[HandbookSmoke]   Block: Image alt='{img.alt}' src='{img.src}' fullPath='{mediaPath}'");
                    break;

                case ListBlock list:
                    Debug.Log($"[HandbookSmoke]   Block: List ordered={list.ordered} items={list.items.Count}");
                    for (int i = 0; i < list.items.Count; i++)
                    {
                        var item = list.items[i];
                        // Выведем текст первых параграфов пункта
                        var itemText = SummarizeListItem(item);
                        Debug.Log($"[HandbookSmoke]     - {itemText}");
                    }
                    break;

                case QuoteBlock q:
                    Debug.Log($"[HandbookSmoke]   Block: Quote ({q.children.Count} children)");
                    break;

                case HorizontalRuleBlock:
                    Debug.Log($"[HandbookSmoke]   Block: HorizontalRule");
                    break;
            }
        }

        private string ExtractInlineText(List<HandbookInlineBase> inlines)
        {
            if (inlines == null || inlines.Count == 0)
                return string.Empty;

            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < inlines.Count; i++)
            {
                var inline = inlines[i];
                if (inline is TextRunInline t) sb.Append(t.text);
                else if (inline is CodeSpanInline c) sb.Append(c.text);
                else if (inline is LinkInline l) sb.Append(ExtractInlineText(l.children));
            }
            return sb.ToString().Trim();
        }

        private string SummarizeListItem(ListItemBlock item)
        {
            if (item?.children == null || item.children.Count == 0)
                return string.Empty;

            for (int i = 0; i < item.children.Count; i++)
            {
                if (item.children[i] is ParagraphBlock p)
                    return ExtractInlineText(p.inlines);
            }
            return $"({item.children.Count} blocks)";
        }

        private string FirstLine(string code)
        {
            if (string.IsNullOrEmpty(code)) return string.Empty;
            var idx = code.IndexOf('\n');
            return idx < 0 ? code.Trim() : code.Substring(0, idx).Trim();
        }

        private async Task RunValidationAsync()
        {
            var report = await _validator.ValidateAllAsync(_repo, CancellationToken.None);

            int errors = 0, warnings = 0, infos = 0;
            foreach (var i in report.Issues)
            {
                if (i.Severity == HandbookValidationSeverity.Error) errors++;
                else if (i.Severity == HandbookValidationSeverity.Warning) warnings++;
                else infos++;
            }

            Debug.Log($"[HandbookSmoke] Validation: Errors={errors}, Warnings={warnings}, Infos={infos}");

            var printed = 0;
            foreach (var i in report.Issues)
            {
                if (printed >= _maxIssuesToPrint) break;
                Debug.Log($"[HandbookSmoke][{i.Severity}] {i.Code}: {i.Message} (page='{i.PageId}', anchor='{i.Anchor}', url='{i.LinkUrl}', step='{i.StepId}')");
                printed++;
            }
        }
    }
}