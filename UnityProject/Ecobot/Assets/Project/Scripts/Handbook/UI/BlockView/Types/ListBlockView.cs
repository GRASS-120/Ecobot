using Handbook.Parser;
using Handbook.Parser.BlockTypes;
using Handbook.UI.BlockView.Base;
using TMPro;
using UnityEngine;

namespace Handbook.UI.BlockView.Types
{
    public class ListBlockView : HandbookBlockView
    {
        [SerializeField] private RectTransform _contentRoot;
        [SerializeField] private TMP_Text _markerTextPrefab;

        public override void Setup(HandbookBlockBase model, HandbookBlockRenderContext ctx)
        {
            var list = model as ListBlock;
            if (list == null) return;

            var root = _contentRoot != null ? _contentRoot : (RectTransform)transform;

            if (list.items == null || list.items.Count == 0)
                return;

            for (int i = 0; i < list.items.Count; i++)
            {
                var marker = Instantiate(_markerTextPrefab, root);
                marker.text = list.ordered ? $"{i + 1}. " : "• ";

                var item = list.items[i];
                if (item?.children == null) continue;

                for (int c = 0; c < item.children.Count; c++)
                {
                    var child = item.children[c];
                    ctx.Factory.Create(child, root);
                }
            }
        }
    }
}