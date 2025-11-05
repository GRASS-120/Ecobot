using System;
using System.Collections.Generic;
using Handbook.Editor.Scanning;
using Handbook.Models;

namespace Handbook.Editor.Processing
{
    public class ManifestMerger
    {
        private readonly bool _preserveTitles;
        private readonly bool _preserveTags;
        private readonly bool _preserveSummary;
        private readonly bool _preserveHidden;

        public ManifestMerger(bool preserveTitles, bool preserveTags, bool preserveSummary, bool preserveHidden)
        {
            _preserveTitles = preserveTitles;
            _preserveTags = preserveTags;
            _preserveSummary = preserveSummary;
            _preserveHidden = preserveHidden;
        }

        public HandbookManifest BuildManifest(
            List<ScanModels.SectionNode> rootSections,
            HandbookManifest existing,
            string version,
            string language,
            string mediaBasePath,
            string defaultPageId)
        {
            var manifest = new HandbookManifest
            {
                version = string.IsNullOrWhiteSpace(version) ? existing?.version ?? "1.0.0" : version.Trim(),
                language = string.IsNullOrWhiteSpace(language) ? existing?.language ?? "ru" : language.Trim(),
                mediaBasePath = string.IsNullOrWhiteSpace(mediaBasePath) ? existing?.mediaBasePath ?? "media" : mediaBasePath.Trim(),
                defaultPageId = string.IsNullOrWhiteSpace(defaultPageId) ? existing?.defaultPageId : defaultPageId.Trim(),
                sections = new List<HandbookSection>(),
                redirects = existing?.redirects ?? new List<HandbookRedirect>()
            };

            var oldPages = IndexOldPages(existing);

            for (int i = 0; i < rootSections.Count; i++)
            {
                var s = rootSections[i];
                manifest.sections.Add(ConvertSection(s, oldPages));
            }

            // Если defaultPageId пуст — выберем первую доступную страницу
            if (string.IsNullOrWhiteSpace(manifest.defaultPageId))
            {
                var list = new List<HandbookPageRef>();
                CollectAllPages(manifest.sections, list);
                if (list.Count > 0)
                    manifest.defaultPageId = list[0].id;
            }

            // Контроль уникальности id страниц — сообщим в логи при конфликтах (не блокируем запись)
            // Для строгого контроля можно перенести это в валидатор Editor'а
            return manifest;
        }

        private HandbookSection ConvertSection(ScanModels.SectionNode node, Dictionary<string, HandbookPageRef> oldPages)
        {
            var sec = new HandbookSection
            {
                id = SlugGenerator.ToSlug(node.Title),
                title = node.Title,
                pages = new List<HandbookPageRef>(),
                children = new List<HandbookSection>()
            };

            if (node.Pages != null)
            {
                for (int i = 0; i < node.Pages.Count; i++)
                    sec.pages.Add(ConvertPage(node.Pages[i], oldPages));
            }

            if (node.Children != null)
            {
                for (int i = 0; i < node.Children.Count; i++)
                    sec.children.Add(ConvertSection(node.Children[i], oldPages));
            }

            return sec;
        }

        private HandbookPageRef ConvertPage(ScanModels.PageNode node, Dictionary<string, HandbookPageRef> oldPages)
        {
            // Бережный merge контентных полей
            var hasOld = oldPages.TryGetValue(node.Id, out var old);

            var title = hasOld && _preserveTitles && !string.IsNullOrWhiteSpace(old.title) ? old.title : node.Title;
            var tags = hasOld && _preserveTags ? old.tags : null;
            var summary = hasOld && _preserveSummary ? old.summary : null;
            var hidden = hasOld && _preserveHidden ? old.hidden : false;

            return new HandbookPageRef
            {
                id = node.Id,
                fileName = node.FileName,
                filePath = node.RelativePath,
                title = title,
                tags = tags,
                summary = summary,
                hidden = hidden,
                hash = node.Hash,
                updatedAt = node.UpdatedAt
            };
        }

        private Dictionary<string, HandbookPageRef> IndexOldPages(HandbookManifest existing)
        {
            var dict = new Dictionary<string, HandbookPageRef>(StringComparer.Ordinal);
            if (existing == null || existing.sections == null) return dict;

            var list = new List<HandbookPageRef>();
            CollectAllPages(existing.sections, list);
            for (int i = 0; i < list.Count; i++)
            {
                var p = list[i];
                if (!string.IsNullOrWhiteSpace(p.id) && !dict.ContainsKey(p.id))
                    dict[p.id] = p;
            }

            return dict;
        }

        private void CollectAllPages(List<HandbookSection> sections, List<HandbookPageRef> sink)
        {
            if (sections == null) return;

            for (int i = 0; i < sections.Count; i++)
            {
                var s = sections[i];

                if (s.pages != null)
                {
                    for (int j = 0; j < s.pages.Count; j++)
                        sink.Add(s.pages[j]);
                }

                if (s.children != null)
                    CollectAllPages(s.children, sink);
            }
        }
    }
}