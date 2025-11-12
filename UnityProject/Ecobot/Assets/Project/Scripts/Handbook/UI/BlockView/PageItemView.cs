using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Handbook.UI.BlockView
{
    public class PageItemView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private GameObject _selectedMarker; // опционально: выделение текущей страницы
        [SerializeField] private Image _background;          // опционально: подсветка фона при выборе
        [SerializeField] private Color _selectedColor = new Color(0.2f, 0.5f, 1f, 0.15f);
        [SerializeField] private Color _normalColor = new Color(0f, 0f, 0f, 0f);

        public string PageId { get; private set; }

        private Action<string> _onClick;

        public void Setup(string pageId, string title, Action<string> onClick)
        {
            PageId = pageId;
            _onClick = onClick;
            if (_label != null)
                _label.text = string.IsNullOrWhiteSpace(title) ? pageId : title;

            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
                _button.onClick.AddListener(() => _onClick?.Invoke(PageId));
            }

            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            if (_selectedMarker != null)
                _selectedMarker.SetActive(selected);

            if (_background != null)
                _background.color = selected ? _selectedColor : _normalColor;
        }
    }
}