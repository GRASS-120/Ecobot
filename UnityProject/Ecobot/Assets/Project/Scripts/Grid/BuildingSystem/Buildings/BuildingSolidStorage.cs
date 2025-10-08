using GUI.Gameplay.Windows.Controller;
using GUI.UIFramework;
using InteractionSystem;
using Inventory;
using UnityEngine;

namespace Grid.BuildingSystem.Buildings
{
    public class BuildingSolidStorage : BuildingBase, IInventoryHolder, IInteractable
    {
        [Header("Storage Capacity")]
        [Min(1)]
        [SerializeField] protected int inventorySize = 12;
        
        private InventorySystem _inventorySystem;
        
        public override void Init(
            BuildingAssetData data,
            Vector2Int origin,
            WindowManager windowManager,
            BuildingAssetData.Dir dir = BuildingAssetData.Dir.Down)
        {
            base.Init(data, origin, windowManager, dir);
            _inventorySystem = new InventorySystem(inventorySize);
        }

        public void Interact(IInteractor interactor)
        {
            HandleInventory();
        }

        private void HandleInventory()
        {
            Debug.Log("Opening solid storage inventory");
            
            var storageWindow = _windowManager.GetController<StorageInventoryWindowController>();
            
            if (storageWindow.IsOpen)
            {
                _windowManager.CloseWindow<StorageInventoryWindowController>();
            }
            else
            {
                storageWindow.SetStorage(_inventorySystem);
                _windowManager.OpenWindow<StorageInventoryWindowController>();
            }
        }

        public bool TryAddToInventory(InventoryItemData data, int amount)
        {
            return _inventorySystem.TryAddToInventory(data, amount);
        }
    }
}