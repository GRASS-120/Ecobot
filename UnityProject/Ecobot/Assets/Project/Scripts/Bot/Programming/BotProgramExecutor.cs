using System.Collections;
using System.Collections.Generic;
using Bot.Programming.Nodes.Base;
using Grid.BuildingSystem;
using Grid.BuildingSystem.Buildings;
using Grid.BuildingSystem.Buildings.Base;
using Inventory;
using UnityEngine;

namespace Bot.Programming
{
    public class BotProgramExecutor
    {
        private BotBase bot;
        private Dictionary<string, InventoryItemData> simulatedItems = new Dictionary<string, InventoryItemData>();
        private Dictionary<string, BuildingBase> simulatedBuildings = new Dictionary<string, BuildingBase>();
        private Dictionary<InventoryItemData, Vector3> itemPositions = new Dictionary<InventoryItemData, Vector3>();
        // Добавим список созданных объектов для последующего удаления
        private List<Object> createdObjects = new List<Object>();
        public BotProgramExecutor(BotBase bot)
        {
            this.bot = bot;
            
            // Инициализация симуляционных данных для тестирования
            InitializeSimulationData();
        }
        
        private void InitializeSimulationData()
        {
            // Создаем тестовые данные для симуляции
            var testItem = ScriptableObject.CreateInstance<InventoryItemData>();
            testItem.displayName = "Test Item";
            testItem.maxStackValue = 10;
            simulatedItems["Test Item"] = testItem;
            itemPositions[testItem] = new Vector3(5, 0, 5);
            createdObjects.Add(testItem);
            
            var testBuildingObject = new GameObject("Test Building");
            var testBuilding = testBuildingObject.AddComponent<BuildingBase>();
            simulatedBuildings["Storage"] = testBuilding;
            createdObjects.Add(testBuildingObject);
        }
        
        // Метод для очистки созданных объектов
        public void Cleanup()
        {
            createdObjects.Clear();
            simulatedItems.Clear();
            simulatedBuildings.Clear();
            itemPositions.Clear();
        }
        
        public IEnumerator ExecuteNode(ProgNodeBase node)
        {
            if (node == null)
            {
                Debug.LogWarning("Attempted to execute null node");
                yield break;
            }
            
            yield return node.Execute(bot, this);
        }
        
        // Симуляционные методы для тестирования
        public bool SimulateFindItem(string itemName, out InventoryItemData item)
        {
            if (simulatedItems.TryGetValue(itemName, out item))
            {
                return true;
            }
            
            item = null;
            return false;
        }
        
        public bool SimulateFindBuilding(string buildingType, out BuildingBase building)
        {
            if (simulatedBuildings.TryGetValue(buildingType, out building))
            {
                return true;
            }
            
            building = null;
            return false;
        }
        
        public bool SimulatePutItem(InventoryItemData item, BuildingBase building)
        {
            // Простая симуляция успешного размещения
            return true;
        }
        
        public Vector3 GetItemPosition(InventoryItemData item)
        {
            if (itemPositions.TryGetValue(item, out Vector3 position))
            {
                return position;
            }
            
            return Vector3.zero;
        }
    }
}