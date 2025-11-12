using System.Collections.Generic;
using Handbook.Parser;
using Handbook.Parser.BlockTypes;
using Handbook.Parser.InlineTypes;
using Handbook.UI.BlockView.Base;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Handbook.UI.BlockView.Types
{
    public class ParagraphBlockView : HandbookBlockView
    {
        [SerializeField] private RectTransform _contentRoot;
        [SerializeField] private TMP_Text _textPrefab;
        [SerializeField] private Button _linkButtonPrefab;

        public override void Setup(HandbookBlockBase model, HandbookBlockRenderContext ctx)
        {
            var p = model as ParagraphBlock;
            if (p == null) return;

            if (p.inlines == null || p.inlines.Count == 0)
            {
                if (_textPrefab != null)
                {
                    var t = Instantiate(_textPrefab, GetRoot());
                    t.text = string.Empty;
                }
                return;
            }

            for (int i = 0; i < p.inlines.Count; i++)
            {
                var inline = p.inlines[i];

                if (inline is TextRunInline tr)
                {
                    var t = Instantiate(_textPrefab, GetRoot());
                    t.text = tr.text ?? string.Empty;
                }
                else if (inline is CodeSpanInline cs)
                {
                    var t = Instantiate(_textPrefab, GetRoot());
                    t.fontStyle = FontStyles.Bold;
                    t.text = cs.text ?? string.Empty;
                }
                else if (inline is LinkInline li)
                {
                    var btn = Instantiate(_linkButtonPrefab, GetRoot());
                    var label = btn.GetComponentInChildren<TMP_Text>();
                    if (label != null)
                        label.text = BuildPlainText(li.children);

                    var url = li.url;
                    btn.onClick.AddListener(() =>
                    {
                        _ = ctx.HandleLinkAsync?.Invoke(url);
                    });
                }
            }
        }

        private Transform GetRoot() => _contentRoot != null ? _contentRoot : transform;

        private string BuildPlainText(List<HandbookInlineBase> inlines)
        {
            if (inlines == null || inlines.Count == 0) return string.Empty;
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < inlines.Count; i++)
            {
                if (inlines[i] is TextRunInline t) sb.Append(t.text);
                else if (inlines[i] is CodeSpanInline c) sb.Append(c.text);
                else if (inlines[i] is LinkInline l) sb.Append(BuildPlainText(l.children));
            }
            return sb.ToString();
        }
    }
}