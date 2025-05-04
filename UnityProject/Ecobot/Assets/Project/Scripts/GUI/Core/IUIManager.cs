using Game.Mods.Core;
using GUI.UIFramework;

namespace GUI.Core
{
    public interface IUIManager
    {
        public void Init(UIRootView rootView);
        public void OpenOverlay();
    }
}