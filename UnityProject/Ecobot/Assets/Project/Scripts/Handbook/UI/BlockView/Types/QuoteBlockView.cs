using Handbook.Parser;
using Handbook.Parser.BlockTypes;
using Handbook.UI.BlockView.Base;
using UnityEngine;

namespace Handbook.UI.BlockView.Types
{
    public class QuoteBlockView : HandbookBlockView
    {
        [SerializeField] private RectTransform _contentRoot;

        public override void Setup(HandbookBlockBase model, HandbookBlockRenderContext ctx)
        {
            var q = model as QuoteBlock;
            if (q == null) return;

            var root = _contentRoot != null ? _contentRoot : (RectTransform)transform;

            if (q.children == null) return;
            for (int i = 0; i < q.children.Count; i++)
                ctx.Factory.Create(q.children[i], root);
        }
    }
}