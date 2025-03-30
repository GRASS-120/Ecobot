using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Inventory.UI
{
    public class MouseInventoryItem : MonoBehaviour
    {
        public Image image;
        public TextMeshProUGUI count;
        public InventorySlot assignedSlot;

        private void Awake()
        {
            image.color = Color.clear;
            count.text = null;
        }

        private void Update()
        {
            if (assignedSlot.ItemData == null) return;
            
            // Mouse - это из new input system, так что не зазорно использовать!
            transform.position = Mouse.current.position.ReadValue();  // ! refactor на GetMousePosition

            if (Mouse.current.leftButton.wasPressedThisFrame && !IsPointerOverUIObject())
            {
                // ! remake что б не удалялся объект, а выпадал
                ClearSlot();
            }
        }

        public void ClearSlot()
        {
            assignedSlot.ClearSlot();
            count.text = "";
            image.color = Color.clear;
            image.sprite = null;
        }

        public void UpdateMouseSlot(InventorySlot slot)
        {
            this.assignedSlot.ReassignItem(slot);
            
            image.sprite = slot.ItemData.icon;
            count.text = slot.StackSize.ToString();
            image.color = Color.white;
        }

        // чтобы нормально работало, нужно сделать RaycastTarget -> false у Image и Count, а то скрипт их тоже считает
        public static bool IsPointerOverUIObject()
        {
            // EventSystem.current - текущая система событий ui
            PointerEventData currentPosition = new PointerEventData(EventSystem.current);
            currentPosition.position = Mouse.current.position.ReadValue();
            
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(currentPosition, results);
            return results.Count > 0;
        }
    }
}