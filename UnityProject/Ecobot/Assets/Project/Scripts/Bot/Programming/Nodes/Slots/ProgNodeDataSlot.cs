using System;
using Bot.Programming.Nodes.Base;
using UnityEngine;

namespace Bot.Programming.Nodes.Slots
{
    public class ProgNodeDataSlot<T> : ProgNodeSlotBase
    {
        private T value;
        private ProgNodeSlotBase connectedSlot; // Может быть слот любого типа
        private Func<object, T> converter; // Функция конвертации значения из подключенного слота

        public T Value 
        { 
            get 
            {
                // Если слот подключен к другому слоту данных
                if (connectedSlot != null)
                {
                    // Получаем значение из подключенного слота
                    var method = connectedSlot.GetType().GetMethod("GetValue");
                    if (method != null)
                    {
                        object sourceValue = method.Invoke(connectedSlot, null);
                        
                        // Если есть функция конвертации, используем её
                        if (converter != null)
                        {
                            return converter(sourceValue);
                        }
                        
                        // Если типы совместимы, просто приводим
                        if (sourceValue is T typedValue)
                        {
                            return typedValue;
                        }
                    }
                }
                
                // Возвращаем собственное значение, если нет подключения или конвертация не удалась
                return value;
            }
            set 
            { 
                this.value = value; 
            }
        }

        public ProgNodeDataSlot(string slotName, ProgNodeBase owner) : base(slotName, owner) { }

        // Метод для получения значения (используется при подключении)
        public T GetValue()
        {
            return Value;
        }

        // Соединяет этот слот с другим слотом данных
        public void ConnectToDataSlot<TSource>(ProgNodeDataSlot<TSource> sourceSlot, Func<TSource, T> conversionFunc = null)
        {
            if (sourceSlot != null)
            {
                connectedSlot = sourceSlot;
                
                // Если предоставлена функция конвертации, используем её
                if (conversionFunc != null)
                {
                    converter = (obj) => conversionFunc((TSource)obj);
                }
                // Иначе пытаемся использовать прямое приведение типов
                else if (typeof(TSource).IsAssignableFrom(typeof(T)) || typeof(T).IsAssignableFrom(typeof(TSource)))
                {
                    converter = (obj) => (T)obj;
                }
                
                Debug.Log($"Connected data slot {SlotName} to {sourceSlot.SlotName}");
            }
        }

        public override bool CanConnect(ProgNodeBase node)
        {
            // Этот метод используется для соединения потоковых слотов, а не слотов данных
            return false;
        }
    }
}