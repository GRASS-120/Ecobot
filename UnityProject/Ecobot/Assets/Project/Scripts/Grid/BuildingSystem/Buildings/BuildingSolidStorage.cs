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
            Debug.Log("Interacting with solid storage");
            var inventoryUI = _windowManager.GetController<InventoryWindowController>();
            // if (inventoryUI.IsOpen)
            // {
            //     _windowManager.CloseWindow<InventoryWindowController>();
            // }
            // else
            // {
            //     _windowManager.OpenWindow<InventoryWindowController>();
            // }
        }
        
        public bool TryAddToInventory(InventoryItemData data, int amount)
        {
            throw new System.NotImplementedException();
        }
    }
}