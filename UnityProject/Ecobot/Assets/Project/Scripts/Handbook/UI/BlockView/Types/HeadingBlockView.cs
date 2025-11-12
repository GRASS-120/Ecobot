using System.Collections.Generic;
using Handbook.Parser;
using Handbook.Parser.BlockTypes;
using Handbook.Parser.InlineTypes;
using Handbook.UI.BlockView.Base;
using TMPro;
using UnityEngine;

namespace Handbook.UI.BlockView.Types
{
    public class HeadingBlockView : HandbookBlockView
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private float _baseSize = 24f;
        [SerializeField] private float _h1 = 1.30f;
        [SerializeField] private float _h2 = 1.20f;
        [SerializeField] private float _h3 = 1.10f;
        [SerializeField] private float _h4 = 1.00f;
        [SerializeField] private float _h5 = 0.95f;
        [SerializeField] private float _h6 = 0.90f;

        public override void Setup(HandbookBlockBase model, HandbookBlockRenderContext ctx)
        {
            var h = model as HeadingBlock;
            if (h == null || _text == null) return;

            _text.text = BuildPlainText(h.inlines);
            _text.fontSize = _baseSize * Mult(h.level);

            if (!string.IsNullOrWhiteSpace(h.anchorId))
                ctx.Anchors.Register(h.anchorId, transform as RectTransform);
        }

        private float Mult(int level)
        {
            switch (Mathf.Clamp(level, 1, 6))
            {
                case 1: return _h1;
                case 2: return _h2;
                case 3: return _h3;
                case 4: return _h4;
                case 5: return _h5;
                default: return _h6;
            }
        }

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