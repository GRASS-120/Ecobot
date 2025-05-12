using System;
using Bot.Programming.Nodes.Base;
using UnityEngine;

namespace Bot.Programming.Nodes.Slots
{
    public class ProgNodeDataSlot<T> : ProgNodeSlotBase
    {
        private T value;
        private ProgNodeDataSlot<T> connectedDataSlot; // Слот, к которому подключен этот слот
        private object adapter; // Адаптер для преобразования типов
    
        public T Value 
        { 
            get 
            {
                // Если есть адаптер, используем его
                if (adapter != null)
                {
                    // Используем рефлексию для вызова GetValue()
                    var method = adapter.GetType().GetMethod("GetValue");
                    if (method != null)
                    {
                        return (T)method.Invoke(adapter, null);
                    }
                }
            
                // Если слот подключен к другому слоту данных, получаем значение из него
                if (connectedDataSlot != null)
                {
                    return connectedDataSlot.value;
                }
            
                return value;
            }
            set 
            { 
                this.value = value; 
            }
        }
    
        public ProgNodeDataSlot(string slotName, ProgNodeBase owner) : base(slotName, owner) { }
    
        // Соединяет этот слот с другим слотом данных
        public void ConnectToDataSlot(ProgNodeDataSlot<T> otherSlot)
        {
            if (otherSlot != null && otherSlot != this)
            {
                connectedDataSlot = otherSlot;
                Debug.Log($"Connected data slot {SlotName} to {otherSlot.SlotName}");
            }
        }
    
        // Устанавливает адаптер для преобразования типов
        public void SetAdapter(object adapter)
        {
            this.adapter = adapter;
            Debug.Log($"Set adapter for data slot {SlotName}");
        }
    
        public override bool CanConnect(ProgNodeBase node)
        {
            // Этот метод используется для соединения потоковых слотов, а не слотов данных
            return false;
        }
    }
}