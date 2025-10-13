using System;
using System.Collections;
using System.Collections.Generic;
using Bot.Programming.Nodes;
using Bot.Programming.Nodes.Base;
using Bot.Programming.Nodes.Concrete;
using Bot.Programming.Nodes.Slots;
using Grid.BuildingSystem;
using Grid.BuildingSystem.Buildings;
using Grid.BuildingSystem.Buildings.Base;
using Inventory;
using UnityEngine;

namespace Bot.Programming
{
    public class BotProgrammingController : MonoBehaviour
    {
        private BotBase bot;
        private BotProgramExecutor executor;
        private ProgNodeBase rootNode;
        private bool programCreated;
        
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                CreateTestProgram();
            }
            if (Input.GetKeyDown(KeyCode.O))
            {
                RunProgram();
            }
            if (Input.GetKeyDown(KeyCode.P))
            {
                StopProgram();
            }
        }
        
        public void Init(BotBase bot)
        {
            this.bot = bot;
            executor = new BotProgramExecutor(bot);
        }
        
        private void OnDestroy()
        {
            executor?.Cleanup();
        }
        
        private void CreateTestProgram()
        {
            if (programCreated) return;
            
            var startIdleNode = new ProgNodeStateIdle();
            var endIdleNode = new ProgNodeStateIdle(); 
            
            var findItemNode = new ProgNodeFindAndPick("Test Item");
            var findBuildingNode = new ProgNodeFindBuilding("Storage");
            var moveToItemNode = new ProgNodeMoveTo();
            var moveToBuildingNode = new ProgNodeMoveTo();
            var putNode = new ProgNodePut();
            
            // Соединяем потоковые слоты (Stream Slots)
            startIdleNode.Slots[0].Connect(findItemNode); // Start Idle -> FindAndPick
            
            findItemNode.Slots[0].Connect(moveToItemNode); // FindAndPick Success -> MoveTo(item)
            findItemNode.Slots[1].Connect(endIdleNode); // FindAndPick Failure -> End Idle
            
            moveToItemNode.Slots[0].Connect(findBuildingNode); // MoveTo(item) Success -> FindBuilding
            moveToItemNode.Slots[1].Connect(endIdleNode); // MoveTo(item) Failure -> End Idle
            
            findBuildingNode.Slots[0].Connect(moveToBuildingNode); // FindBuilding Success -> MoveTo(building)
            findBuildingNode.Slots[1].Connect(endIdleNode); // FindBuilding Failure -> End Idle
            
            moveToBuildingNode.Slots[0].Connect(putNode); // MoveTo(building) Success -> Put
            moveToBuildingNode.Slots[1].Connect(endIdleNode); // MoveTo(building) Failure -> End Idle
            
            putNode.Slots[0].Connect(startIdleNode); // Put Success -> Start Idle (цикл)
            putNode.Slots[1].Connect(endIdleNode); // Put Failure -> End Idle
            
            // Находим слоты данных в нодах
            ProgNodeDataSlot<InventoryItemData> findItemOutputSlot = null;
            ProgNodeDataSlot<BuildingBase> findBuildingOutputSlot = null;
            ProgNodeDataSlot<object> moveToItemTargetSlot = null;
            ProgNodeDataSlot<object> moveToBuildingTargetSlot = null;
            ProgNodeDataSlot<InventoryItemData> putItemSlot = null;
            ProgNodeDataSlot<BuildingBase> putBuildingSlot = null;
            
            // Находим нужные слоты в нодах
            foreach (var slot in findItemNode.Slots)
            {
                if (slot is ProgNodeDataSlot<InventoryItemData> dataSlot && slot.SlotName == "Found Item")
                {
                    findItemOutputSlot = dataSlot;
                    break;
                }
            }
            
            foreach (var slot in findBuildingNode.Slots)
            {
                if (slot is ProgNodeDataSlot<BuildingBase> dataSlot && slot.SlotName == "Found Building")
                {
                    findBuildingOutputSlot = dataSlot;
                    break;
                }
            }
            
            foreach (var slot in moveToItemNode.Slots)
            {
                if (slot is ProgNodeDataSlot<object> dataSlot && slot.SlotName == "Target")
                {
                    moveToItemTargetSlot = dataSlot;
                    break;
                }
            }
            
            foreach (var slot in moveToBuildingNode.Slots)
            {
                if (slot is ProgNodeDataSlot<object> dataSlot && slot.SlotName == "Target")
                {
                    moveToBuildingTargetSlot = dataSlot;
                    break;
                }
            }
            
            foreach (var slot in putNode.Slots)
            {
                if (slot is ProgNodeDataSlot<InventoryItemData> dataSlotItem && slot.SlotName == "Item")
                {
                    putItemSlot = dataSlotItem;
                }
                else if (slot is ProgNodeDataSlot<BuildingBase> dataSlotBuilding && slot.SlotName == "Building")
                {
                    putBuildingSlot = dataSlotBuilding;
                }
            }
            
            // Соединяем слоты данных напрямую
            // Для MoveTo Item - прямое подключение с автоматическим преобразованием
            if (findItemOutputSlot != null && moveToItemTargetSlot != null)
            {
                moveToItemTargetSlot.ConnectToDataSlot(findItemOutputSlot);
                Debug.Log("Connected item output to MoveTo target slot");
            }

            // Для MoveTo Building - прямое подключение с автоматическим преобразованием
            if (findBuildingOutputSlot != null && moveToBuildingTargetSlot != null)
            {
                moveToBuildingTargetSlot.ConnectToDataSlot(findBuildingOutputSlot);
                Debug.Log("Connected building output to MoveTo target slot");
            }

            // Для Put - прямое подключение
            if (findItemOutputSlot != null && putItemSlot != null)
            {
                putItemSlot.ConnectToDataSlot(findItemOutputSlot);
                Debug.Log("Connected item output to Put item slot");
            }

            if (findBuildingOutputSlot != null && putBuildingSlot != null)
            {
                putBuildingSlot.ConnectToDataSlot(findBuildingOutputSlot);
                Debug.Log("Connected building output to Put building slot");
            }
            
            // Устанавливаем корневую ноду
            rootNode = startIdleNode;
            programCreated = true;
            
            Debug.Log("BIBA");
        }
        
        // Метод для запуска программы
        public void RunProgram()
        {
            if (rootNode == null)
            {
                Debug.LogWarning("No program to run");
                return;
            }
            
            StartCoroutine(executor.ExecuteNode(rootNode));
        }
        
        public void StopProgram()
        {
            StopAllCoroutines();
        }
    }
}