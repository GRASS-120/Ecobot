using GUI.Gameplay.Windows.Controller;
using GUI.UIFramework;
using InteractionSystem;
using Inventory;
using Inventory.LootSystem;
using R3;
using UnityEngine;

namespace Player
{
    public class PlayerInventoryHolder : MonoBehaviour, IInventoryHolder, ILootReceiver
    {
        [Header("Main Inventory")]
        [Min(1)]
        [SerializeField] protected int mainInventorySize = 20;

        [Header("Hot Bar Inventory")]
        [Min(1)]
        [SerializeField] private int hotbarInventorySize = 10;

        public InventorySystem MainInventory => _mainInventorySystem;
        public InventorySystem HotbarInventorySystem => _hotbarInventorySystem;

        private PlayerManager _player;
        private InventorySystem _mainInventorySystem;
        private InventorySystem _hotbarInventorySystem;

        public void Init(PlayerManager player)
        {
            _hotbarInventorySystem = new InventorySystem(hotbarInventorySize);
            _mainInventorySystem = new InventorySystem(mainInventorySize);
            
            _player = player;
            
            _player.Input.OnOpenInventory += HandleInventory;
        }

        private void HandleInventory()
        {
            var inventoryUI = _player.WindowManager.GetController<InventoryWindowController>();
            if (inventoryUI.IsOpen)
            {
                _player.WindowManager.CloseWindow<InventoryWindowController>();
            }
            else
            {
                _player.WindowManager.OpenWindow<InventoryWindowController>();
            }
        }

        public bool TryAddToInventory(InventoryItemData data, int amount)
        {
            if (_hotbarInventorySystem.TryAddToInventory(data, amount)) return true;
            if (_mainInventorySystem.TryAddToInventory(data, amount)) return true;
            return false;
        }

        public bool TryReceive(LootQuery loot)
        {
            return TryAddToInventory(loot.Item, loot.Amount);
        }
    }
}