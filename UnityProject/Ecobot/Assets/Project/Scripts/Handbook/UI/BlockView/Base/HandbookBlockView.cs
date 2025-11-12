using Handbook.Parser;
using UnityEngine;

namespace Handbook.UI.BlockView.Base
{
    public abstract class HandbookBlockView : MonoBehaviour
    {
        public abstract void Setup(HandbookBlockBase model, HandbookBlockRenderContext ctx);
        public virtual void Dispose() { }
    }
}