using Game;
using Game.Mods.Core;
using GUI.Gameplay;
using GUI.UIFramework;

namespace GUI.Core
{
    public interface IUIManager
    {
        public void Init(GameUIRootViewModel rootViewModel, GameMode mode, GameManager gameManager);
        public void OpenOverlay();
    }
}