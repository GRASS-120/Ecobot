using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GUI.Gameplay.Windows.View;
using GUI.UIFramework;
using Handbook;
using Handbook.Models;
using Handbook.Parser.BlockTypes;
using Handbook.Routing.Actions;
using Handbook.UI;
using Handbook.UI.BlockView;
using R3;
using UnityEngine;

namespace GUI.Gameplay.Windows.Controller
{
    [Window(WindowType.Popup, "HandbookPopup")]
    public class HandbookPopupController : WindowController<HandbookPopupView>
    {
        public override string Id => "HandbookPopup";

        private HandbookManager _manager;
        private readonly AnchorIndex _anchors = new();
        private CancellationTokenSource _lifecycleCts;
        private readonly Dictionary<string, (int sectionIndex, int pageIndex)> _pageToNav = new();
        private List<HandbookSection> _sections = new();
        private int _selectedSection = 0;
        private int _selectedPage = 0;
        private readonly List<IDisposable> _subs = new();
        private HandbookBlockViewFactory _blockFactory;
        private SpriteCache _spriteCache;
        private string _pendingAnchorToScroll;
        private readonly Dictionary<string, PageItemView> _pageItems = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<int, SectionItemView> _sectionViews = new();
        private PageItemView _currentSelectedPage;
        
        public void Init(HandbookManager manager)
        {
            _manager = manager;
        }
        
        public override void OnOpen()
        {
            base.OnOpen();

            if (_manager == null)
            {
                View?.SetError("HandbookManager не передан в контроллер.");
                return;
            }

            _lifecycleCts?.Cancel();
            _lifecycleCts = new CancellationTokenSource();

            HookViewEvents();
            SubscribeToManager();
            EnsureRenderPipeline();
            InitializeAsync(_lifecycleCts.Token);
        }

        public override void OnClose()
        {
            base.OnClose();

            UnhookViewEvents();

            _lifecycleCts?.Cancel();
            _lifecycleCts?.Dispose();
            _lifecycleCts = null;

            _spriteCache?.Dispose();
            _spriteCache = null;

            for (int i = 0; i < _subs.Count; i++)
            {
                try { _subs[i]?.Dispose(); } catch { }
            }
            _subs.Clear();
        }

        private async void InitializeAsync(CancellationToken ct)
        {
            try
            {
                await _manager.EnsureInitializedAsync(ct);
                BuildNavigation();
                await _manager.OpenDefaultPageAsync(ct);
            }
            catch (Exception e)
            {
                View?.SetError(e.Message);
            }
        }
        
        private void HookViewEvents()
        {
            if (View == null) return;
            View.SectionChanged += OnSectionSelected;
            View.PageChanged += OnPageSelected;
        }

        private void UnhookViewEvents()
        {
            if (View == null) return;
            View.SectionChanged -= OnSectionSelected;
            View.PageChanged -= OnPageSelected;
        }

        private void SubscribeToManager()
        {
            // очищаем старые подписки на всякий случай
            for (int i = 0; i < _subs.Count; i++)
            {
                try { _subs[i]?.Dispose(); } catch { }
            }
            _subs.Clear();

            _subs.Add(_manager.OnLoadingChanged.Subscribe(isLoading =>
            {
                View?.SetLoading(isLoading);
            }));

            _subs.Add(_manager.OnPageChanged.Subscribe(page =>
            {
                if (page == null) return;

                SyncDropdownSelectionToPage(page.id);
                View?.SetTitle(page.title);
                RenderPage(page);
            }));

            _subs.Add(_manager.OnError.Subscribe(err =>
            {
                View?.SetError(err);
            }));
        }

        private void EnsureRenderPipeline()
        {
            if (_blockFactory != null)
                return;

            _spriteCache = new SpriteCache();

            var ctxProvider = new Func<HandbookBlockRenderContext>(() => new HandbookBlockRenderContext
            {
                View = View,
                Anchors = _anchors,
                BuildMediaPath = _manager.BuildMediaPath,
                HandleLinkAsync = HandleLinkAsync,
                Factory = _blockFactory,
                SpriteCache = _spriteCache
            });

            _blockFactory = new HandbookBlockViewFactory(ctxProvider);

            _blockFactory.Register<HeadingBlock>(View.HeadingPrefab);
            _blockFactory.Register<ParagraphBlock>(View.ParagraphPrefab);
            _blockFactory.Register<ImageBlock>(View.ImagePrefab);
            _blockFactory.Register<ListBlock>(View.ListPrefab);
            _blockFactory.Register<QuoteBlock>(View.QuotePrefab);
            _blockFactory.Register<HorizontalRuleBlock>(View.HrPrefab);
        }

        private void BuildNavigation()
        {
            _sections = _manager.EnumerateSections()?.ToList() ?? new List<HandbookSection>();
            _pageItems.Clear();
            _sectionViews.Clear();

            View.ClearSections();

            for (int s = 0; s < _sections.Count; s++)
            {
                var section = _sections[s];
                var sectionTitle = string.IsNullOrWhiteSpace(section.title) ? section.id : section.title;

                var sectionView = View.CreateSectionItem();
                if (sectionView == null) continue;

                sectionView.Setup(sectionTitle, View.PageItemPrefab);
                sectionView.PageClicked += OnPageClicked;

                _sectionViews[s] = sectionView;

                if (section.pages == null) continue;

                for (int p = 0; p < section.pages.Count; p++)
                {
                    var pr = section.pages[p];
                    if (pr == null || string.IsNullOrWhiteSpace(pr.id)) continue;

                    var pageTitle = string.IsNullOrWhiteSpace(pr.title) ? pr.id : pr.title;
                    var pageItem = sectionView.AddPage(pr.id, pageTitle);
                    if (pageItem != null)
                        _pageItems[pr.id.Trim().ToLowerInvariant()] = pageItem;
                }
            }
        }
        
        private void OnPageClicked(string pageId)
        {
            if (string.IsNullOrWhiteSpace(pageId)) return;
            _ = _manager.OpenPageAsync(pageId);
        }

        private List<string> GetPagesTitles(int sectionIndex)
        {
            if (sectionIndex < 0 || sectionIndex >= _sections.Count)
                return new List<string>();

            var sec = _sections[sectionIndex];
            if (sec.pages == null || sec.pages.Count == 0)
                return new List<string>();

            return sec.pages.Select(p => string.IsNullOrWhiteSpace(p.title) ? p.id : p.title).ToList();
        }

        private void OnSectionSelected(int sectionIndex)
        {
            if (_sectionViews == null || _sectionViews.Count == 0)
                return;

            if (!_sectionViews.TryGetValue(sectionIndex, out var selected))
                return;

            foreach (var kv in _sectionViews)
            {
                if (kv.Key == sectionIndex)
                    kv.Value.Expand();
                else
                    kv.Value.Collapse();
            }
        }

        private void OnPageSelected(int pageIndex)
        {
            _selectedPage = Mathf.Clamp(pageIndex, 0, GetPagesCount(_selectedSection) - 1);
            var pageId = TryGetPageId(_selectedSection, _selectedPage);
            if (pageId != null)
                _ = _manager.OpenPageAsync(pageId);
        }

        private int GetPagesCount(int sectionIndex)
        {
            if (sectionIndex < 0 || sectionIndex >= _sections.Count) return 0;
            var sec = _sections[sectionIndex];
            return sec.pages?.Count ?? 0;
        }

        private string TryGetPageId(int sectionIndex, int pageIndex)
        {
            if (sectionIndex < 0 || sectionIndex >= _sections.Count) return null;
            var sec = _sections[sectionIndex];
            if (sec.pages == null || pageIndex < 0 || pageIndex >= sec.pages.Count) return null;
            return sec.pages[pageIndex].id;
        }

        private void SyncDropdownSelectionToPage(string pageId)
        {
            
            if (string.IsNullOrWhiteSpace(pageId)) return;

            var id = pageId.Trim().ToLowerInvariant();
            if (!_pageItems.TryGetValue(id, out var pageView))
                return;

            if (_currentSelectedPage != null)
                _currentSelectedPage.SetSelected(false);

            _currentSelectedPage = pageView;
            _currentSelectedPage.SetSelected(true);

            // Разворачиваем нужную секцию
            foreach (var kv in _sectionViews)
            {
                var sectionView = kv.Value;
                if (sectionView.TryGetPageItem(pageId, out var pv) && pv == pageView)
                    sectionView.Expand();
                else
                    sectionView.Collapse();
            }
        }

        private async Task HandleLinkAsync(string url)
        {
            var action = _manager.ResolveLink(url);
            if (action is OpenHandbookPageAction open)
            {
                _pendingAnchorToScroll = open.Anchor;
                SyncDropdownSelectionToPage(open.PageId);
                await _manager.OpenPageAsync(open.PageId);
            }
            // tutorial:// и внешние ссылки добавим позже
        }

        private void RenderPage(HandbookPage page)
        {
            _anchors.Clear();
            View.ClearContent();

            if (page.blocks == null || page.blocks.Count == 0)
                return;

            for (int i = 0; i < page.blocks.Count; i++)
                _blockFactory.Create(page.blocks[i], View.ContentRoot);

            if (!string.IsNullOrEmpty(_pendingAnchorToScroll) &&
                _anchors.TryGet(_pendingAnchorToScroll, out var target))
            {
                View.ScrollTo(target);
            }
            _pendingAnchorToScroll = null;
        }

        // Вспомогательные типы
    }
}