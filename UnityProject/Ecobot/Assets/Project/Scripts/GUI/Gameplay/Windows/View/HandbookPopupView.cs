using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GUI.UIFramework;
using Handbook.UI;
using Handbook.UI.BlockView;
using Handbook.UI.BlockView.Base;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GUI.Gameplay.Windows.View
{
    public class HandbookPopupView : PopupView
    {
        [SerializeField] private RectTransform _sectionsRoot;
        [SerializeField] private SectionItemView _sectionItemPrefab;
        [SerializeField] private PageItemView _pageItemPrefab;
        [SerializeField] private ScrollRect _scroll;
        [SerializeField] private RectTransform _contentRoot;
        [SerializeField] private GameObject _loadingRoot;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private GameObject _errorRoot;
        [SerializeField] private TMP_Text _errorText;

        
        [SerializeField] private HandbookBlockView _headingPrefab;
        [SerializeField] private HandbookBlockView _paragraphPrefab;
        [SerializeField] private HandbookBlockView _imagePrefab;
        [SerializeField] private HandbookBlockView _listPrefab;
        [SerializeField] private HandbookBlockView _quotePrefab;
        [SerializeField] private HandbookBlockView _hrPrefab;
        
        public event Action<int> SectionChanged;
        public event Action<int> PageChanged;
        public SectionItemView SectionItemPrefab => _sectionItemPrefab;
        public PageItemView PageItemPrefab => _pageItemPrefab;
        public RectTransform ContentRoot => _contentRoot;
        public ScrollRect Scroll => _scroll;
        public RectTransform SectionsRoot => _sectionsRoot;
        public HandbookBlockView HeadingPrefab => _headingPrefab;
        public HandbookBlockView ParagraphPrefab => _paragraphPrefab;
        public HandbookBlockView ImagePrefab => _imagePrefab;
        public HandbookBlockView ListPrefab => _listPrefab;
        public HandbookBlockView QuotePrefab => _quotePrefab;
        public HandbookBlockView HrPrefab => _hrPrefab;
        
        private void Awake()
        {
            SetLoading(false);
            SetError(null);
        }

        private void OnSectionDropdownChanged(int index)
        {
            SectionChanged?.Invoke(index);
        }

        private void OnPageDropdownChanged(int index)
        {
            PageChanged?.Invoke(index);
        }

        // ДОБАВИТЬ методы состояния/заголовка
        public void SetLoading(bool isLoading)
        {
            if (_loadingRoot != null)
                _loadingRoot.SetActive(isLoading);
        }

        public void SetTitle(string title)
        {
            if (_titleText != null)
                _titleText.text = title ?? string.Empty;
        }

        public void SetError(string message)
        {
            if (_errorRoot != null)
                _errorRoot.SetActive(!string.IsNullOrEmpty(message));
            if (_errorText != null)
                _errorText.text = message ?? string.Empty;
        }
        
        public void ClearSections()
        {
            if (_sectionsRoot == null) return;
            for (int i = _sectionsRoot.childCount - 1; i >= 0; i--)
                Destroy(_sectionsRoot.GetChild(i).gameObject);
        }

        public SectionItemView CreateSectionItem()
        {
            if (_sectionsRoot == null || _sectionItemPrefab == null) return null;
            var item = Instantiate(_sectionItemPrefab, _sectionsRoot);
            return item;
        }

        // ДОБАВИТЬ методы контента
        public void ClearContent()
        {
            if (_contentRoot == null) return;
            var blocks = _contentRoot.GetComponentsInChildren<HandbookBlockView>(true);
            for (int i = 0; i < blocks.Length; i++)
            {
                try { blocks[i].Dispose(); } catch { }
            }
            for (int i = _contentRoot.childCount - 1; i >= 0; i--)
                Destroy(_contentRoot.GetChild(i).gameObject);
        }

        public void AddBlock(RectTransform block)
        {
            if (_contentRoot == null || block == null) return;
            block.SetParent(_contentRoot, false);
        }

        // ДОБАВИТЬ скролл к элементу (DOTween)
        public void ScrollTo(RectTransform target, float duration = 0.25f)
        {
            if (_scroll == null || target == null || _contentRoot == null) return;
            if (_scroll.verticalNormalizedPosition < 0 || _scroll.verticalNormalizedPosition > 1)
                _scroll.verticalNormalizedPosition = Mathf.Clamp01(_scroll.verticalNormalizedPosition);

            var vp = _scroll.viewport != null ? _scroll.viewport : _scroll.GetComponent<RectTransform>();
            if (vp == null) return;

            var contentBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(_contentRoot, _contentRoot);
            var targetBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(_contentRoot, target);

            var contentHeight = contentBounds.size.y;
            var viewportHeight = vp.rect.height;
            var scrollable = Mathf.Max(1e-3f, contentHeight - viewportHeight);

            var contentTop = contentBounds.max.y;
            var targetTop = targetBounds.max.y;
            var offsetFromTop = Mathf.Clamp(contentTop - targetTop, 0f, scrollable);

            var normalized = 1f - Mathf.Clamp01(offsetFromTop / scrollable);

            DOTween.Kill(_scroll); // на всякий
            DOTween.To(
                () => _scroll.verticalNormalizedPosition,
                v => _scroll.verticalNormalizedPosition = v,
                normalized,
                duration
            );
        }
    }
}