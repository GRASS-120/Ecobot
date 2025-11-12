using System;
using System.Threading.Tasks;
using GUI.Gameplay.Windows.View;

namespace Handbook.UI
{
    public struct HandbookBlockRenderContext
    {
        public HandbookPopupView View;
        public AnchorIndex Anchors;
        public Func<string, string> BuildMediaPath;
        public Func<string, Task> HandleLinkAsync;
        public IHandbookBlockViewFactory Factory;
        public ISpriteCache SpriteCache;
    }
}