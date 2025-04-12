using System;
using InteractionSystem;
using Player;
using UnityEngine;

namespace Inventory
{
    // убрал IInretacrable, оэтому не работает. нужно у постройки его сделать, а не у storage блять
    public class StorageInventory : InventoryHolder
    {
        public event Action<IInteractable> OnInteractionComplete;
        
        public void Interact(PlayerInteractor playerInteractor, out bool success)
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
