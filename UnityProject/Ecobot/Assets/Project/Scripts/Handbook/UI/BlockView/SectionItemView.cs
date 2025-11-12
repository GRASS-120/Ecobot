using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Handbook.UI.BlockView
{
    public class SectionItemView : MonoBehaviour
    {
        [SerializeField] private Button _headerButton;
        [SerializeField] private TMP_Text _headerText;
        [SerializeField] private RectTransform _pagesRoot;
        [SerializeField] private PageItemView _pageItemPrefab;
        [SerializeField] private GameObject _arrowExpanded;   // опционально, иконка раскрытия
        [SerializeField] private GameObject _arrowCollapsed;  // опционально, иконка свёрнута
        
        private readonly List<PageItemView> _pages = new();
        private bool _expanded;

        public event Action<string> PageClicked;

        public void Setup(string title, PageItemView pageItemPrefabOverride = null)
        {
            if (_headerText != null)
                _headerText.text = title ?? string.Empty;

            if (pageItemPrefabOverride != null)
                _pageItemPrefab = pageItemPrefabOverride;

            SetExpanded(false);

            if (_headerButton != null)
            {
                _headerButton.onClick.RemoveAllListeners();
                _headerButton.onClick.AddListener(() => SetExpanded(!_expanded));
            }
        }

        public PageItemView AddPage(string pageId, string pageTitle)
        {
            if (_pagesRoot == null || _pageItemPrefab == null) return null;
            var item = Instantiate(_pageItemPrefab, _pagesRoot);
            item.Setup(pageId, pageTitle, OnPageClickInternal);
            _pages.Add(item);
            return item;
        }

        public void SetExpanded(bool expanded)
        {
            _expanded = expanded;
            if (_pagesRoot != null)
                _pagesRoot.gameObject.SetActive(_expanded);

            if (_arrowExpanded != null) _arrowExpanded.SetActive(_expanded);
            if (_arrowCollapsed != null) _arrowCollapsed.SetActive(!_expanded);
        }

        public void Collapse() => SetExpanded(false);
        public void Expand() => SetExpanded(true);

        public void ClearSelection()
        {
            for (int i = 0; i < _pages.Count; i++)
                _pages[i].SetSelected(false);
        }

        public bool TryGetPageItem(string pageId, out PageItemView view)
        {
            for (int i = 0; i < _pages.Count; i++)
            {
                if (string.Equals(_pages[i].PageId, pageId, StringComparison.OrdinalIgnoreCase))
                {
                    view = _pages[i];
                    return true;
                }
            }
            view = null;
            return false;
        }

        private void OnPageClickInternal(string pageId)
        {
            PageClicked?.Invoke(pageId);
        }
    }
}