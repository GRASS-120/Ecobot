using System;
using UnityEngine;

namespace Inventory
{
    public class StorageInventory : InventoryHolder, IInteractable
    {
        public event Action<IInteractable> OnInteractionComplete;
        
        public void Interact(Interactor interactor, out bool success)
        {
            // эм... если делать через eventhandler, то не работает (что логично, так как ивент статичный)
            // но блять если unityaction, то все норм... как?
            OnDynamicInventoryDisplayRequested?.Invoke(primaryInventorySystem);
            success = true;
        }

        public void EndInteraction()
        {
            throw new NotImplementedException();
        }
    }
}
