using System.Collections;
using Bot.Programming.Nodes.Base;
using Bot.Programming.Nodes.Concrete;
using Bot.Programming.Nodes.Slots;
using Grid.BuildingSystem.Buildings.Base;
using UnityEngine;

namespace Bot.Programming
{
    public class BotProgrammingController : MonoBehaviour
    {
        private BotBase bot;
        private BotProgramExecutor executor;
        private ProgNodeBase rootNode;
        private bool programCreated;
        private Vector3 startPosition; // точка возврата

        // Храним корутину, чтобы можно было корректно остановить
        private Coroutine programRoutine;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
                CreateTestProgram();

            if (Input.GetKeyDown(KeyCode.O))
                RunProgram();

            if (Input.GetKeyDown(KeyCode.P))
                StopProgram();
        }

        public void Init(BotBase bot)
        {
            this.bot = bot;
            executor = new BotProgramExecutor(bot);
            Debug.Log("[Controller] Init called. Executor created.");
        }

        private void OnDestroy()
        {
            executor?.Cleanup();
            Debug.Log("[Controller] OnDestroy - cleaned up executor data.");
        }

        private void CreateTestProgram()
        {
            if (programCreated)
            {
                Debug.LogWarning("[Controller] CreateTestProgram called but program already created.");
                return;
            }

            startPosition = bot != null ? bot.transform.position : Vector3.zero;
            Debug.Log($"[Controller] Creating test program. Bot start position: {startPosition}");

            // Ноды
            var idleNode = new ProgNodeStateIdle();
            var findA = new ProgNodeFindBuilding("Storage_A");
            var moveToA = new ProgNodeMoveTo();
            var findB = new ProgNodeFindBuilding("Storage_B");
            var moveToB = new ProgNodeMoveTo();

            // Поток выполнения (stream slots)
            Debug.Log("[Controller] Connecting stream slots:");
            Debug.Log($"  idle -> findA");
            idleNode.Slots[0].Connect(findA);

            Debug.Log($"  findA -> moveToA");
            findA.Slots[0].Connect(moveToA);

            Debug.Log($"  moveToA -> findB");
            moveToA.Slots[0].Connect(findB);

            Debug.Log($"  findB -> moveToB");
            findB.Slots[0].Connect(moveToB);

            Debug.Log($"  moveToB -> idle (loop)");
            moveToB.Slots[0].Connect(idleNode); // зацикливаем

            // Соединяем данные (data slots)
            ProgNodeDataSlot<BuildingBase> slotA = null;
            ProgNodeDataSlot<BuildingBase> slotB = null;
            ProgNodeDataSlot<object> moveToATarget = null;
            ProgNodeDataSlot<object> moveToBTarget = null;

            // Находим слот "Found Building" у findA
            foreach (var slot in findA.Slots)
            {
                Debug.Log($"[Controller] findA slot: {slot.SlotName} ({slot.GetType().Name})");
                if (slot is ProgNodeDataSlot<BuildingBase> s && s.SlotName == "Found Building")
                {
                    slotA = s;
                    Debug.Log("[Controller] Found data slotA in findA: " + s.SlotName);
                }
            }

            // Находим слот "Found Building" у findB
            foreach (var slot in findB.Slots)
            {
                Debug.Log($"[Controller] findB slot: {slot.SlotName} ({slot.GetType().Name})");
                if (slot is ProgNodeDataSlot<BuildingBase> s && s.SlotName == "Found Building")
                {
                    slotB = s;
                    Debug.Log("[Controller] Found data slotB in findB: " + s.SlotName);
                }
            }

            // Находим слот "Target" у moveToA
            foreach (var slot in moveToA.Slots)
            {
                Debug.Log($"[Controller] moveToA slot: {slot.SlotName} ({slot.GetType().Name})");
                if (slot is ProgNodeDataSlot<object> s && s.SlotName == "Target")
                {
                    moveToATarget = s;
                    Debug.Log("[Controller] Found target slot in moveToA: " + s.SlotName);
                }
            }

            // Находим слот "Target" у moveToB
            foreach (var slot in moveToB.Slots)
            {
                Debug.Log($"[Controller] moveToB slot: {slot.SlotName} ({slot.GetType().Name})");
                if (slot is ProgNodeDataSlot<object> s && s.SlotName == "Target")
                {
                    moveToBTarget = s;
                    Debug.Log("[Controller] Found target slot in moveToB: " + s.SlotName);
                }
            }

            // Подключаем данные и логируем
            if (slotA != null && moveToATarget != null)
            {
                moveToATarget.ConnectToDataSlot(slotA);
                Debug.Log("[Controller] Connected moveToA.Target -> findA.Found Building");
            }
            else
            {
                Debug.LogWarning("[Controller] Failed to connect moveToA target - slotA or moveToATarget is null");
            }

            if (slotB != null && moveToBTarget != null)
            {
                moveToBTarget.ConnectToDataSlot(slotB);
                Debug.Log("[Controller] Connected moveToB.Target -> findB.Found Building");
            }
            else
            {
                Debug.LogWarning("[Controller] Failed to connect moveToB target - slotB or moveToBTarget is null");
            }

            rootNode = idleNode;
            programCreated = true;

            Debug.Log("✅ Program created: bot will move between Storage_A and Storage_B");
            Debug.Log($"[Controller] Root node: {rootNode.NodeName}");
        }

        // запуск (вариант 2: запускаем корутину один раз, ноды сами переключаются)
        public void RunProgram()
        {
            if (rootNode == null)
            {
                Debug.LogWarning("[Controller] No program to run (rootNode is null)");
                return;
            }

            if (executor == null)
            {
                Debug.LogWarning("[Controller] Executor is null - did you call Init()?");
                return;
            }

            if (programRoutine != null)
            {
                Debug.LogWarning("[Controller] Program already running");
                return;
            }

            Debug.Log("[Controller] ▶️ Starting program coroutine from root node: " + rootNode.NodeName);
            programRoutine = StartCoroutine(RunRoutine());
        }

        // Вспомогательная корутина, чтобы логировать start/finish
        private IEnumerator RunRoutine()
        {
            Debug.Log("[Controller] RunRoutine: started");
            yield return executor.ExecuteNode(rootNode);
            Debug.Log("[Controller] RunRoutine: finished (executor.ExecuteNode returned). If program is cyclic, nodes should have re-invoked each other.");
            programRoutine = null;
        }

        public void StopProgram()
        {
            if (programRoutine != null)
            {
                StopCoroutine(programRoutine);
                programRoutine = null;
                Debug.Log("🛑 Program stopped (coroutine stopped)");
            }
            else
            {
                Debug.Log("[Controller] StopProgram called but program was not running");
            }
        }
    }
}
