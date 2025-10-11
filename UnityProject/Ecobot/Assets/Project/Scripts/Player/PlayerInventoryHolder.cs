using System.Collections.Generic;
using GUI.Gameplay.Windows.Controller;
using GUI.UIFramework;
using InteractionSystem;
using Inventory;
using Inventory.CraftingSystem;
using Inventory.LootSystem;
using Inventory.Services;
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

        [Header("Crafting")]
        [SerializeField] private List<CraftingRecipeData> availableCraftingRecipes = new List<CraftingRecipeData>();
        
        public InventorySystem MainInventory => _mainInventorySystem;
        public InventorySystem HotbarInventorySystem => _hotbarInventorySystem;
        public InventorySelectionService InventorySelectionService => _inventorySelectionService;
        public InventoryResourceCounterService ResourceCounterService => _resourceCounterService;
        public CraftingSystem CraftingSystem => _craftingSystem; 

        private PlayerManager _player;
        private InventorySystem _mainInventorySystem;
        private InventorySystem _hotbarInventorySystem;
        private InventorySelectionService _inventorySelectionService; 
        private InventoryResourceCounterService _resourceCounterService;
        private CraftingSystem _craftingSystem; 

        public void Init(PlayerManager player)
        {
            _hotbarInventorySystem = new InventorySystem(hotbarInventorySize);
            _mainInventorySystem = new InventorySystem(mainInventorySize);
            _inventorySelectionService = new InventorySelectionService();
            _resourceCounterService = new InventoryResourceCounterService();
            
            _player = player;
            
            _resourceCounterService.SubscribeToInventory(_hotbarInventorySystem);
            _resourceCounterService.SubscribeToInventory(_mainInventorySystem);
            
            _craftingSystem = new CraftingSystem(_resourceCounterService, availableCraftingRecipes, this);
            
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