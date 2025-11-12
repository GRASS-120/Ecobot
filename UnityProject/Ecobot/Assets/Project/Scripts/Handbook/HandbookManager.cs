using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GUI.Gameplay.Windows.Controller;
using GUI.UIFramework;
using Handbook.ContentProvider;
using Handbook.Models;
using Handbook.Parser;
using Handbook.Routing;
using R3;
using UnityEngine;

namespace Handbook
{
    public class HandbookManager : MonoBehaviour
    {
        [SerializeField] private WindowManager _windowManager;
        [SerializeField] private HandbookRuntimeConfig _config;
        [SerializeField] private KeyCode _openKey = KeyCode.P;

        private IHandbookRepository _repo;
        private IHandbookContentProvider _provider;
        private IHandbookMarkdownParser _parser;
        private IHandbookLinkRouter _linkRouter;

        private readonly Subject<bool> _initialized = new();
        private readonly Subject<bool> _loading = new();
        private readonly Subject<string> _currentPageId = new();
        private readonly Subject<HandbookPage> _currentPage = new();
        private readonly Subject<string> _lastError = new();

        private CancellationTokenSource _loadCts;

        public bool IsInitialized { get; private set; }
        public Observable<bool> OnInitialized => _initialized;
        public Observable<bool> OnLoadingChanged => _loading;
        public Observable<string> OnPageIdChanged => _currentPageId;
        public Observable<HandbookPage> OnPageChanged => _currentPage;
        public Observable<string> OnError => _lastError;

        private void Update()
        {
            if (_windowManager == null)
                return;

            if (Input.GetKeyDown(_openKey))
                OpenPopup();
        }

        public void OpenPopup()
        {
            _windowManager.OpenWindow<HandbookPopupController>(controller =>
            {
                controller.Init(this);
            });
        }

        public async Task EnsureInitializedAsync(CancellationToken ct = default)
        {
            if (IsInitialized)
                return;

            if (_config == null || string.IsNullOrWhiteSpace(_config.RootFolder))
                throw new InvalidOperationException("HandbookManager: назначьте HandbookRuntimeConfig в инспекторе и укажите RootFolder.");

            var root = ResolveManifestRoot(_config.RootFolder);
            Debug.Log($"[Handbook] Using handbook root: {root}");

            _provider = new FileSystemHandbookContentProvider(root);
            _linkRouter = new HandbookLinkRouter();
            _parser = new HandbookMarkdownParser(_linkRouter);
            _repo = new HandbookRepository();

            await _repo.InitializeAsync(_provider, _parser, ct);
            IsInitialized = true;
            _initialized.OnNext(true);
        }
        
        private string ResolveManifestRoot(string configuredRoot)
        {
            var root = configuredRoot.Trim();
            if (string.IsNullOrWhiteSpace(root))
                throw new InvalidOperationException("HandbookManager: пустой RootFolder в конфиге.");

            root = root.Replace('\\', '/').TrimEnd('/');

            var direct = Path.Combine(root, "manifest.json");
            if (File.Exists(direct))
                return root;

            var nested = Path.Combine(root, "Handbook", "manifest.json");
            if (File.Exists(nested))
                return Path.Combine(root, "Handbook");

            throw new FileNotFoundException(
                $"Handbook manifest.json not found. Checked:\n  {direct}\n  {nested}\n" +
                "Проверь RootFolder в HandbookRuntimeConfig или путь сохранения манифеста в генераторе.");
        }

        public IReadOnlyList<HandbookSection> EnumerateSections()
        {
            EnsureRepo();
            return _repo.EnumerateSections();
        }

        public bool TryGetPageRef(string pageId, out HandbookPageRef pageRef)
        {
            EnsureRepo();
            return _repo.TryGetPageRef(pageId, out pageRef);
        }

        public bool PageExists(string pageId)
        {
            EnsureRepo();
            return _repo.PageExists(pageId);
        }

        public string BuildMediaPath(string relativePath)
        {
            EnsureRepo();
            return _repo.BuildMediaPath(relativePath);
        }

        public LinkActionBase ResolveLink(string url)
        {
            return _linkRouter.Resolve(url);
        }

        public async Task OpenDefaultPageAsync(CancellationToken ct = default)
        {
            EnsureRepo();

            var id = _repo.Manifest.defaultPageId;
            if (string.IsNullOrWhiteSpace(id) || !_repo.PageExists(id))
            {
                var all = _repo.EnumerateAllPages();
                id = all.Count > 0 ? all[0].id : null;
            }

            if (!string.IsNullOrEmpty(id))
                await OpenPageAsync(id, null, ct);
        }

        public async Task OpenPageAsync(string pageId, string anchorId = null, CancellationToken externalCt = default)
        {
            EnsureRepo();

            CancelActiveLoad();
            _loadCts = CancellationTokenSource.CreateLinkedTokenSource(externalCt);
            var ct = _loadCts.Token;

            try
            {
                _loading.OnNext(true);
                var page = await _repo.LoadPageAsync(pageId, ct);
                _currentPageId.OnNext(pageId);
                _currentPage.OnNext(page);
            }
            catch (Exception e)
            {
                _lastError.OnNext(e.Message);
            }
            finally
            {
                _loading.OnNext(false);
            }
        }

        public void DisposeRepo()
        {
            CancelActiveLoad();

            _repo?.Dispose();
            _repo = null;
            _parser = null;
            _linkRouter = null;
            _provider = null;

            if (IsInitialized)
            {
                IsInitialized = false;
                _initialized.OnNext(false);
            }
        }

        private void OnDestroy()
        {
            DisposeRepo();

            _initialized.Dispose();
            _loading.Dispose();
            _currentPageId.Dispose();
            _currentPage.Dispose();
            _lastError.Dispose();
        }

        private void EnsureRepo()
        {
            if (!IsInitialized)
                throw new InvalidOperationException("HandbookManager is not initialized. Call EnsureInitializedAsync first.");
        }

        private void CancelActiveLoad()
        {
            if (_loadCts == null)
                return;

            _loadCts.Cancel();
            _loadCts.Dispose();
            _loadCts = null;
        }
    }
}