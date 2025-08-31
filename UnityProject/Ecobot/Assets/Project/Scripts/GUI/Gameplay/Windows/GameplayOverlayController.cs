using GUI.UIFramework;
using Inventory;
using Player;
using R3;
using UnityEngine;

namespace GUI.Gameplay.Windows
{
    public class GameplayOverlayController : WindowController<GameplayOverlayView>
    {
        private readonly GameplayUIManager _uiManager;
        private readonly PlayerInventoryHolder _playerInventoryHolder; 

        public override string Id => "GameplayOverlay";

        public GameplayOverlayController(
            GameplayUIManager uiManager,
            PlayerInventoryHolder playerInventoryHolder)
        {
            _uiManager = uiManager;
            _playerInventoryHolder = playerInventoryHolder;
        }

        public override void OnOpen()
        {
            // var a = new CompositeDisposable();

            View.MouseInventoryItemUI.Init(Subs);


            // Хотбар (quick move -> основной инвентарь)
            View.PlayerHotbarUI.Init(
                _playerInventoryHolder.HotbarInventorySystem, 
                View.MouseInventoryItemUI, 
                Subs,
                quickMoveTarget: _playerInventoryHolder.MainInventory);

            // Основной инвентарь (quick move -> хотбар)
            View.PlayerInventoryUI.Init(
                _playerInventoryHolder.MainInventory, 
                View.MouseInventoryItemUI, 
                Subs,
                quickMoveTarget: _playerInventoryHolder.HotbarInventorySystem);
            // View.StorageInventoryUI.Init(_playerInventoryHolder.HotbarInventorySystem, Subs);
            
            Subs.Add(_playerInventoryHolder.MainInventory.OnInventorySlotChanged
                .Subscribe(s =>
                {
                    int idx = _playerInventoryHolder.MainInventory.IndexOf(s);
                    Debug.Log($"[MainInv] changed idx={idx} item={(s.ItemData ? s.ItemData.displayName : "null")} count={s.StackSize}");
                }));

            Subs.Add(_playerInventoryHolder.HotbarInventorySystem.OnInventorySlotChanged
                .Subscribe(s =>
                {
                    int idx = _playerInventoryHolder.HotbarInventorySystem.IndexOf(s);
                    Debug.Log($"[Hotbar] changed idx={idx} item={(s.ItemData ? s.ItemData.displayName : "null")} count={s.StackSize}");
                }));
            
            Debug.Log($"[UI Bind] hotbar={_playerInventoryHolder.HotbarInventorySystem.GetHashCode()} main={_playerInventoryHolder.MainInventory.GetHashCode()}");
        }
    }
}