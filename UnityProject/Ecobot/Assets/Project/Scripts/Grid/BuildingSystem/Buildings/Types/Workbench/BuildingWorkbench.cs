using System.Collections;
using Grid.BuildingSystem.Buildings.Base;
using GUI.Gameplay.Windows.Controller;
using InteractionSystem;
using UnityEngine;

namespace Grid.BuildingSystem.Buildings
{
    public class BuildingWorkbench : BuildingBase, IInteractable
    {
        public void Interact(IInteractor interactor)
        {
            if (_context?.PowerWireToolService?.IsActive == true)
            {
                return;
            }
            
            HandleCraftingWindow();
        }

        private void HandleCraftingWindow()
        {
            if (_context?.PlayerManager?.Inventory == null)
            {
                return;
            }

            var craftingWindow = _windowManager.GetController<CraftingWindowController>();
            
            if (craftingWindow.IsOpen)
            {
                _windowManager.CloseWindow<CraftingWindowController>();
            }
            else
            {
                var playerInventory = _context.PlayerManager.Inventory;
                
                _windowManager.OpenWindow<CraftingWindowController>(controller =>
                {
                    controller.Init(
                        playerInventory.CraftingSystem,
                        playerInventory.MainInventory,
                        playerInventory.HotbarInventorySystem
                    );
                });
            }
        }
    }
}