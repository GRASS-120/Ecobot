using Handbook.Parser;
using Handbook.UI.BlockView;
using Handbook.UI.BlockView.Base;
using UnityEngine;

namespace Handbook.UI
{
    public interface IHandbookBlockViewFactory
    {
        HandbookBlockView Create(HandbookBlockBase model, Transform parent);
    }
}