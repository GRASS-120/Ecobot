using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;
using Bot.Programming.Nodes.Base;
using Bot.Programming.Nodes.Concrete;
using Bot.Programming.Nodes.Slots;
using Grid.BuildingSystem.Buildings.Base;
using UnityEngine;
using GUI.Programming.Graph;

namespace Bot.Programming
{
    public class BotProgrammingController : MonoBehaviour
    {
        private enum ProgramSource { None, Snapshot, Test }

        [Header("Loop")]
        [Tooltip("Зацикливать выполнение программы (по достижении конца снова стартовать с корня).")]
        [SerializeField] private bool loopProgram = true;

        [Header("Persistence")]
        [Tooltip("Устойчивый идентификатор бота для сохранения графа. Если пусто — возьмём InstanceID.")]
        [SerializeField] private string persistenceId;

        public string PersistenceId
        {
            get
            {
                if (string.IsNullOrEmpty(persistenceId))
                    persistenceId = $"Bot#{gameObject.GetInstanceID()}";
                return persistenceId;
            }
        }

        private BotBase bot;
        private BotProgramExecutor executor;
        private ProgNodeBase rootNode;
        private bool programCreated;
        private ProgramSource programSource = ProgramSource.None;
        private Vector3 startPosition;

        private Coroutine programRoutine;
        private bool cancelRequested; // ставится при StopProgram()

        // Храним последний полученный снапшот (для отладки/инспекции)
        private GraphSnapshotDTO lastSnapshot;
        public GraphSnapshotDTO LastSnapshot => lastSnapshot;

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

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                Debug.Log("[Controller] 'I' pressed -> CreateTestProgram()");
                CreateTestProgram();
            }

            if (Input.GetKeyDown(KeyCode.O))
            {
                Debug.Log("[Controller] 'O' pressed -> RunProgram()");
                RunProgram();
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                Debug.Log("[Controller] 'P' pressed -> StopProgram()");
                StopProgram();
            }
        }

        // === Загрузка снапшота от графа ===
        public void LoadGraph(GraphSnapshotDTO snap)
        {
            if (executor == null)
            {
                Debug.LogWarning("[Controller] Executor is null - did you call Init()?");
                return;
            }

            lastSnapshot = snap;

            int nodeCount = snap?.nodes != null ? snap.nodes.Count : 0;
            int edgeCount = snap?.edges != null ? snap.edges.Count : 0;
            Debug.Log($"[Controller] ⏳ LoadGraph: snapshot nodes={nodeCount}, edges={edgeCount} (botId={PersistenceId})");

            try
            {
                string snapshotStructure = DumpSnapshotStructure(snap);
                if (!string.IsNullOrEmpty(snapshotStructure))
                    Debug.Log($"[Controller] 📦 Snapshot structure:\n{snapshotStructure}");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[Controller] DumpSnapshotStructure failed: {ex}");
            }

            var program = GraphSnapshotToProgram.Build(snap, out var errors);

            if (errors != null && errors.Count > 0)
            {
                Debug.LogWarning("[Controller] Graph build issues:");
                foreach (var e in errors)
                    Debug.LogWarning(" - " + e);
            }

            if (program == null)
            {
                Debug.LogWarning("[Controller] ❌ Failed to build program from snapshot.");
                return;
            }

            rootNode = program;
            programCreated = true;
            programSource = ProgramSource.Snapshot;

            try
            {
                string structure = DumpProgramStructure(rootNode);
                Debug.Log($"[Controller] ✅ Program successfully created from SNAPSHOT.\nRoot: {rootNode.NodeName}\nProgram structure:\n{structure}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Controller] DumpProgramStructure failed: {ex}");
            }
        }

        // === Рекурсивный вывод программы (stream + data), защищённый от циклов ===
        private string DumpProgramStructure(ProgNodeBase root, int depth = 0)
            => DumpProgramStructureInternal(root, depth, new System.Collections.Generic.HashSet<ProgNodeBase>());

        private string DumpProgramStructureInternal(ProgNodeBase root, int depth, System.Collections.Generic.HashSet<ProgNodeBase> path)
        {
            if (root == null) return string.Empty;

            var sb = new StringBuilder();
            string indent = new string(' ', depth * 2);
            sb.AppendLine($"{indent}- {root.NodeName}");

            if (path.Contains(root))
            {
                sb.AppendLine($"{indent}  ↩ cycle detected, stop here");
                return sb.ToString();
            }

            path.Add(root);

            foreach (var slot in root.Slots)
            {
                // STREAM переходы
                if (slot.ConnectedNode != null)
                {
                    sb.Append(DumpProgramStructureInternal(slot.ConnectedNode, depth + 1, path));
                    continue;
                }

                // DATA (ProgNodeDataSlot<>)
                if (IsDataSlot(slot))
                {
                    var src = TryGetConnectedDataSlot(slot);
                    if (src != null)
                        sb.AppendLine($"{indent}  [data] {slot.SlotName} ← {src.Owner.NodeName}.{src.SlotName}");
                    else
                        sb.AppendLine($"{indent}  [data] {slot.SlotName} (no source)");
                }
            }

            path.Remove(root);
            return sb.ToString();
        }

        private static bool IsDataSlot(ProgNodeSlotBase slot)
        {
            var t = slot.GetType();
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ProgNodeDataSlot<>);
        }

        private static ProgNodeSlotBase TryGetConnectedDataSlot(ProgNodeSlotBase dataSlot)
        {
            var field = dataSlot.GetType().GetField("connectedSlot", BindingFlags.NonPublic | BindingFlags.Instance);
            return field?.GetValue(dataSlot) as ProgNodeSlotBase;
        }

        // === Вывод «структуры снапшота» (DTO) ===
        private string DumpSnapshotStructure(GraphSnapshotDTO snap)
        {
            if (snap == null || snap.nodes == null || snap.nodes.Count == 0) return string.Empty;

            var sb = new StringBuilder();

            var nodesById = snap.nodes.ToDictionary(n => n.id, n => n);
            var streamByFrom = new Dictionary<int, GraphEdgeDTO>();
            var dataByTo     = new Dictionary<int, List<GraphEdgeDTO>>();

            foreach (var e in snap.edges ?? new List<GraphEdgeDTO>())
            {
                if (e.fromKind == PortKind.Stream && e.toKind == PortKind.Stream)
                {
                    // один к одному или несколько — для печати хватит последнего
                    streamByFrom[e.fromId] = e;
                }
                else if (e.fromKind == PortKind.Data && e.toKind == PortKind.Data)
                {
                    if (!dataByTo.TryGetValue(e.toId, out var list))
                        dataByTo[e.toId] = list = new List<GraphEdgeDTO>();
                    list.Add(e);
                }
            }

            int rootId = snap.nodes.FirstOrDefault(n => n.type == GUI.Programming.Windows.Nodes.NodeController.UINodeKind.IdleStart)?.id
                         ?? snap.nodes[0].id;

            var path = new System.Collections.Generic.HashSet<int>();
            DumpNodeRec(rootId, 0);
            return sb.ToString();

            void DumpNodeRec(int nodeId, int depth)
            {
                if (!nodesById.TryGetValue(nodeId, out var node)) return;

                string indent = new string(' ', depth * 2);
                sb.AppendLine($"{indent}- {node.type}");

                if (dataByTo.TryGetValue(nodeId, out var dataIns))
                {
                    foreach (var de in dataIns)
                    {
                        string fromName = nodesById.TryGetValue(de.fromId, out var fromN) ? fromN.type.ToString() : $"Node#{de.fromId}";
                        sb.AppendLine($"{indent}  [data] {de.toSlotId} ← {fromName}.{de.fromSlotId}");
                    }
                }

                if (path.Contains(nodeId))
                {
                    sb.AppendLine($"{indent}  ↩ cycle detected, stop here");
                    return;
                }

                path.Add(nodeId);

                if (streamByFrom.TryGetValue(nodeId, out var se))
                {
                    DumpNodeRec(se.toId, depth + 1);
                }

                path.Remove(nodeId);
            }
        }

        // === Тестовая программа (для отладки) ===
        private void CreateTestProgram()
        {
            if (programSource == ProgramSource.Snapshot)
            {
                Debug.LogWarning("[Controller] CreateTestProgram: snapshot program already loaded — skip creating test to avoid confusion. Use O to run snapshot.");
                return;
            }

            if (programCreated)
            {
                Debug.LogWarning("[Controller] CreateTestProgram called but program already created (source=" + programSource + ").");
                return;
            }

            startPosition = bot != null ? bot.transform.position : Vector3.zero;
            Debug.Log($"[Controller] Creating TEST program. Bot start position: {startPosition}");

            // Простая тест-цепочка
            var idle            = new ProgNodeStateIdle();

            var findCoal        = new ProgNodeFindOre("CoalData");
            var moveToCoal      = new ProgNodeMoveTo();
            var mineCoal        = new ProgNodeMineOre("5");

            var findIron        = new ProgNodeFindOre("IronData");
            var moveToIron      = new ProgNodeMoveTo();
            var mineIron        = new ProgNodeMineOre("5");

            var findFurnace     = new ProgNodeFindBuilding("Building_FurnanceData");
            var moveToFurnace   = new ProgNodeMoveTo();
            var putAll          = new ProgNodePut();

            idle.Slots[0].Connect(findCoal);
            findCoal.Slots[0].Connect(moveToCoal);
            moveToCoal.Slots[0].Connect(mineCoal);

            mineCoal.Slots[0].Connect(findIron);
            findIron.Slots[0].Connect(moveToIron);
            moveToIron.Slots[0].Connect(mineIron);

            mineIron.Slots[0].Connect(findFurnace);
            findFurnace.Slots[0].Connect(moveToFurnace);
            moveToFurnace.Slots[0].Connect(putAll);
            putAll.Slots[0].Connect(idle);

            rootNode = idle;
            programCreated = true;
            programSource = ProgramSource.Test;

            string structure = DumpProgramStructure(rootNode);
            Debug.Log("✅ TEST program created.\nProgram structure:\n" + structure);
        }

        // === Запуск/стоп ===
        public void RunProgram()
        {
            if (rootNode == null)
            {
                Debug.LogWarning("[Controller] No program to run (rootNode is null). Press T in the overlay to send a snapshot.");
                return;
            }

            if (executor == null)
            {
                Debug.LogWarning("[Controller] Executor is null - did you call Init() on BotProgrammingController?");
                return;
            }

            if (programRoutine != null)
            {
                Debug.LogWarning("[Controller] Program already running");
                return;
            }

            cancelRequested = false;
            Debug.Log($"[Controller] ▶️ Starting program: source={programSource}, root={rootNode.NodeName}");
            programRoutine = StartCoroutine(RunRoutine());
        }

        private IEnumerator RunRoutine()
        {
            Debug.Log("[Controller] RunRoutine: started");

            while (!cancelRequested)
            {
                yield return executor.ExecuteNode(rootNode);

                if (cancelRequested) break;

                if (!loopProgram)
                {
                    Debug.Log("[Controller] RunRoutine: finished (loop disabled)");
                    break;
                }

                Debug.Log("[Controller] ♻ Program finished — restarting from root (loopProgram=true).");
                yield return null;
            }

            Debug.Log("[Controller] RunRoutine: finished");
            programRoutine = null;
        }

        public void StopProgram()
        {
            cancelRequested = true;

            if (programRoutine != null)
            {
                Debug.Log("[Controller] StopProgram: stopping coroutine...");
                StopCoroutine(programRoutine);
                programRoutine = null;
                Debug.Log("🛑 Program stopped");
            }
            else
            {
                Debug.Log("[Controller] StopProgram called but program was not running");
            }
        }
    }
}
