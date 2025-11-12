using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using GUI.Programming.Windows.Nodes;
using GUI.Programming.Windows.Slots;
using Bot.Programming;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace GUI.Programming.Graph
{
    public class NodeGraphController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private RectTransform nodesContainer;
        [SerializeField] private RectTransform connectionsContainer;

        [Header("Prefabs")]
        [Tooltip("Пары: тип UI-ноды → её префаб. Заполнить в инспекторе.")]
        [SerializeField] private List<KindPrefab> kindPrefabs = new();

        [Tooltip("Префаб Bezier-линии для восстановления соединений из снапшота")]
        [SerializeField] private UIBezierConnection connectionPrefab;

        [Header("Persistence (choose one)")]
        [Tooltip("Использовать сценовое хранилище вместо файла")]
        [SerializeField] private bool useSceneStorage = true;

        [Tooltip("Сценовое хранилище (если не задано — будет найдено автоматически; автосоздания больше нет)")]
        [SerializeField] private NodeGraphSceneStorage sceneStorage;

        [Tooltip("Если не найдено ни одного хранилища и это включено — создать НОВЫЙ объект В КОРНЕ СЦЕНЫ (без родителя у оверлея). По умолчанию выключено.")]
        [SerializeField] private bool autoCreateStorageIfMissing = false;

        [Tooltip("Путь к JSON (если useSceneStorage=false). Относительный → Application.persistentDataPath.")]
        [SerializeField] private string jsonPath = "programs/bot.json";

        [SerializeField] private bool loadOnBind = true;
        [SerializeField] private bool saveOnSend = true;

        [Serializable]
        private struct KindPrefab
        {
            public NodeController.UINodeKind kind;
            public GameObject prefab;
        }

        private Dictionary<NodeController.UINodeKind, GameObject> _prefabByKind;
        private readonly List<NodeController> _nodes = new();
        private readonly List<ConnectionRecord> _edges = new();

        private int _graphId;
        private BotProgrammingController botProgramming;

        private string Pfx => $"[NodeGraph#{_graphId}]";
        public RectTransform ConnectionsContainer => connectionsContainer;

        private void Awake()
        {
            _graphId = GetInstanceID();
            EnsurePrefabMapBuilt();
            EnsureContainers();
            if (useSceneStorage) EnsureSceneStorage();
        }

        // === Контейнеры ===
        private void EnsureContainers()
        {
            if (nodesContainer != null && connectionsContainer != null) return;

            RectTransform[] rts = GetComponentsInChildren<RectTransform>(true);

            if (nodesContainer == null)
                nodesContainer = FindByName(rts, "nodes", "nodescontainer", "nodesroot");

            if (connectionsContainer == null)
                connectionsContainer = FindByName(rts, "connections", "connectionscontainer", "edges", "links");

            if (nodesContainer == null)
            {
                var go = new GameObject("NodesContainer", typeof(RectTransform));
                nodesContainer = go.GetComponent<RectTransform>();
                nodesContainer.SetParent(transform as RectTransform, false);
            }

            if (connectionsContainer == null)
            {
                var go = new GameObject("ConnectionsContainer", typeof(RectTransform));
                connectionsContainer = go.GetComponent<RectTransform>();
                connectionsContainer.SetParent(transform as RectTransform, false);
            }
        }

        private RectTransform FindByName(RectTransform[] list, params string[] keys)
        {
            if (list == null) return null;
            foreach (var rt in list)
            {
                string n = rt.gameObject.name.Trim().ToLowerInvariant();
                for (int i = 0; i < keys.Length; i++)
                {
                    if (n.Contains(keys[i])) return rt;
                }
            }
            return null;
        }

        // ====== Public API ======
        public void SetBotProgramming(BotProgrammingController prog)
        {
            botProgramming = prog;
            EnsurePrefabMapBuilt();
            EnsureContainers();
            if (useSceneStorage) EnsureSceneStorage();

            Debug.Log($"{Pfx} Bound BotProgrammingController = {(prog ? prog.name : "NULL")}");

            if (loadOnBind)
            {
                if (useSceneStorage)
                    TryLoadFromScene();
                else
                    TryLoadFromJson();
            }
        }

        public void RegisterNode(NodeController node)
        {
            if (node != null && !_nodes.Contains(node))
            {
                _nodes.Add(node);
                Debug.Log($"{Pfx} Registered node '{node.name}' ({node.NodeType})");
            }
        }

        public void UnregisterNode(NodeController node)
        {
            if (node == null) return;
            RemoveAllConnectionsOfNode(node);
            _nodes.Remove(node);
            Debug.Log($"{Pfx} Unregistered node '{node.name}'");
        }

        public void RemoveAllConnectionsOfNode(NodeController node)
        {
            if (node == null) return;
            var toRemove = _edges.Where(e => e.FromNode == node || e.ToNode == node).ToList();
            if (toRemove.Count == 0) return;

            foreach (var rec in toRemove)
            {
                rec.Line?.MarkRemovedByGraph();
                rec.FromSlot?.DecrementConnected();
                rec.ToSlot?.DecrementConnected();
                if (rec.Line != null) Destroy(rec.Line.gameObject);
                _edges.Remove(rec);
            }
            Debug.Log($"{Pfx} Removed {toRemove.Count} connections touching '{node.name}'");
        }

        public bool RegisterConnection(SlotController from, SlotController to, UIBezierConnection line)
        {
            if (_edges.Any(e => e.Line == line)) return false;

            _edges.Add(new ConnectionRecord
            {
                FromNode = from.Owner,
                ToNode = to.Owner,
                FromSlot = from,
                ToSlot = to,
                Line = line
            });

            from.IncrementConnected();
            to.IncrementConnected();

            Debug.Log($"{Pfx} Connection registered: {from.Owner.name}.{from.SlotId} -> {to.Owner.name}.{to.SlotId}");
            return true;
        }

        public void RequestRemoveConnection(UIBezierConnection line)
        {
            if (line == null) return;
            var rec = _edges.FirstOrDefault(e => e.Line == line);
            if (rec == null)
            {
                Debug.Log($"{Pfx} RequestRemoveConnection: line={line?.name} not found");
                return;
            }

            rec.Line.MarkRemovedByGraph();
            rec.FromSlot?.DecrementConnected();
            rec.ToSlot?.DecrementConnected();
            _edges.Remove(rec);
            if (rec.Line != null) Destroy(rec.Line.gameObject);

            Debug.Log($"{Pfx} Connection removed: {rec.FromNode.name}->{rec.ToNode.name}");
        }

        // ====== Snapshot ======
        public GraphSnapshotDTO BuildSnapshot()
        {
            var snap = new GraphSnapshotDTO();
            var idByNode = new Dictionary<NodeController, int>(_nodes.Count);

            for (int i = 0; i < _nodes.Count; i++)
            {
                var node = _nodes[i];
                idByNode[node] = i;

                string tech = null, vis = null;
                if (node.TryGetDropdownTechnical(out var t)) tech = t;
                if (node.TryGetDropdownVisual(out var v)) vis = v;

                snap.nodes.Add(new GraphNodeDTO
                {
                    id = i,
                    type = node.NodeType,
                    position = node.GetUIPosition(),
                    dropdownTech = tech,
                    dropdownVisual = vis
                });
            }

            foreach (var e in _edges)
            {
                if (!idByNode.TryGetValue(e.FromNode, out var fromId)) continue;
                if (!idByNode.TryGetValue(e.ToNode, out var toId)) continue;

                snap.edges.Add(new GraphEdgeDTO
                {
                    fromId = fromId,
                    fromSlotId = e.FromSlot.SlotId,
                    fromKind = e.FromSlot.ContentType == SlotController.SlotContentType.Data ? PortKind.Data : PortKind.Stream,
                    toId = toId,
                    toSlotId = e.ToSlot.SlotId,
                    toKind = e.ToSlot.ContentType == SlotController.SlotContentType.Data ? PortKind.Data : PortKind.Stream
                });
            }

            Debug.Log($"{Pfx} BuildSnapshot: nodes={snap.nodes.Count}, edges={snap.edges.Count}");
            return snap;
        }

        public void SendSnapshotToBot()
        {
            // защищаем закрытие оверлея от любых исключений
            try
            {
                var snap = BuildSnapshot();

                try
                {
                    if (botProgramming != null)
                    {
                        Debug.Log($"{Pfx} Sending snapshot to '{botProgramming.name}' ... nodes={snap.nodes.Count}, edges={snap.edges.Count}");
                        botProgramming.LoadGraph(snap);
                    }
                    else
                    {
                        Debug.LogWarning($"{Pfx} BotProgrammingController is not assigned. Call SetBotProgramming first.");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"{Pfx} LoadGraph threw: {e}");
                }

                if (saveOnSend)
                {
                    try
                    {
                        if (useSceneStorage) SaveToScene(snap);
                        else SaveToJson(snap);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"{Pfx} Save on send failed: {e}");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"{Pfx} SendSnapshotToBot failed: {e}");
            }
        }

        // ====== Rebuild UI from snapshot ======
        public void ClearGraph()
        {
            foreach (var e in new List<ConnectionRecord>(_edges)) RequestRemoveConnection(e.Line);
            _edges.Clear();

            foreach (var n in new List<NodeController>(_nodes)) if (n) Destroy(n.gameObject);
            _nodes.Clear();
        }

        public void RebuildFromSnapshot(GraphSnapshotDTO snap)
        {
            ClearGraph();
            EnsurePrefabMapBuilt();
            EnsureContainers();

            if (snap == null)
            {
                Debug.LogWarning($"{Pfx} RebuildFromSnapshot: snap is null");
                return;
            }

            if (_prefabByKind == null || _prefabByKind.Count == 0)
            {
                Debug.LogWarning($"{Pfx} RebuildFromSnapshot: kind→prefab map is empty.");
                return;
            }

            if (connectionPrefab == null)
            {
                Debug.LogWarning($"{Pfx} RebuildFromSnapshot: missing connectionPrefab");
                return;
            }

            var id2node = new Dictionary<int, NodeController>(snap.nodes.Count);

            // 1) ноды
            foreach (var nd in snap.nodes)
            {
                if (!_prefabByKind.TryGetValue(nd.type, out var prefab) || prefab == null)
                {
                    Debug.LogWarning($"{Pfx} No prefab assigned for node type {nd.type}");
                    continue;
                }

                var go = Instantiate(prefab, nodesContainer);
                go.name = $"{nd.type}_{nd.id}";

                var node = go.GetComponent<NodeController>();
                if (node == null)
                {
                    Debug.LogWarning($"{Pfx} Prefab for {nd.type} has no NodeController");
                    Destroy(go);
                    continue;
                }

                // безопасная инъекция графа
                try { node.SendMessage("InjectGraph", this, SendMessageOptions.DontRequireReceiver); }
                catch { /* no-op */ }

                var rt = node.GetComponent<RectTransform>();
                if (rt != null) rt.anchoredPosition = nd.position;

                var binding = node.GetComponentInChildren<NodeDropdownBinding>(true);
                if (binding != null)
                {
                    bool ok = false;
                    if (!string.IsNullOrEmpty(nd.dropdownTech)) ok = binding.TrySetByTechnical(nd.dropdownTech);
                    if (!ok && !string.IsNullOrEmpty(nd.dropdownVisual)) binding.TrySetByVisual(nd.dropdownVisual);
                }

                id2node[nd.id] = node;
            }

            // 2) связи
            foreach (var e in snap.edges)
            {
                if (!id2node.TryGetValue(e.fromId, out var fromNode)) continue;
                if (!id2node.TryGetValue(e.toId, out var toNode)) continue;

                var fromSlot = FindSlotById(fromNode?.OutputSlots, e.fromSlotId);
                var toSlot   = FindSlotById(toNode?.InputSlots,   e.toSlotId);
                if (fromSlot == null || toSlot == null)
                {
                    Debug.LogWarning($"{Pfx} Edge skipped: slot not found {e.fromId}.{e.fromSlotId} -> {e.toId}.{e.toSlotId}");
                    continue;
                }

                var line = Instantiate(connectionPrefab, connectionsContainer);
                line.name = $"Conn_{fromNode.name}.{fromSlot.SlotId}__{toNode.name}.{toSlot.SlotId}";
                line.SetContainer(connectionsContainer);
                line.SetStartSlot(fromSlot.ConnectionPoint);
                line.SetEndSlot(toSlot.ConnectionPoint);
                line.SetGraph(this);
                line.SetInteractable(true);
                line.AssociatedOutput = fromSlot;
                line.AssociatedInput  = toSlot;

                RegisterConnection(fromSlot, toSlot, line);

                fromNode?.AddLocalConnectionIfMissing(fromSlot, toSlot, line);
                toNode?.AddLocalConnectionIfMissing(fromSlot, toSlot, line);
            }

            Debug.Log($"{Pfx} RebuildFromSnapshot done: nodes={_nodes.Count}, edges={_edges.Count}");
        }

        // ====== Persistence: Scene ======
        private void EnsureSceneStorage()
        {
            if (sceneStorage != null)
            {
                Debug.Log($"{Pfx} Scene storage already assigned: {GetHierarchyPath(sceneStorage.transform)}");
                return;
            }

            // Ищем ВСЕ хранилища на сцене (включая Inactive и спрятанные)
            var all = Resources.FindObjectsOfTypeAll<NodeGraphSceneStorage>();
            NodeGraphSceneStorage best = null;

            foreach (var s in all)
            {
                // отфильтровываем те, что принадлежат префабам/невалидным сценам
                if (!s.gameObject.scene.IsValid() || !s.gameObject.scene.isLoaded) continue;

                // исключаем хранилища, которые являются дочерними нашего оверлея
                if (transform != null && s.transform.IsChildOf(transform)) continue;

#if UNITY_EDITOR
                // исключаем ассетные префабы (Prefab Stage/asset)
                if (EditorUtility.IsPersistent(s)) continue;
#endif
                best = s; // берём первый подходящий
                break;
            }

            if (best != null)
            {
                sceneStorage = best;
                Debug.Log($"{Pfx} Scene storage resolved: {GetHierarchyPath(sceneStorage.transform)}");
                return;
            }

            // Ничего не нашли
            if (autoCreateStorageIfMissing)
            {
                var go = new GameObject("NodeGraphStorage");
                // В КОРНЕ СЦЕНЫ, без родителя у оверлея
                go.transform.SetParent(null, false);
                sceneStorage = go.AddComponent<NodeGraphSceneStorage>();
                Debug.Log($"{Pfx} Scene storage AUTO-CREATED at scene root: {GetHierarchyPath(sceneStorage.transform)}");
            }
            else
            {
                Debug.LogWarning($"{Pfx} Scene storage NOT FOUND. Saving to scene will be skipped. " +
                                 "Assign 'sceneStorage' in inspector or enable 'autoCreateStorageIfMissing'.");
            }
        }

        private void SaveToScene(GraphSnapshotDTO snap)
        {
            if (sceneStorage == null)
            {
                Debug.LogWarning($"{Pfx} SaveToScene skipped: storage is NULL.");
                return;
            }

            try
            {
                sceneStorage.snapshot = snap;
                sceneStorage.snapshotJson = JsonUtility.ToJson(new Wrapper { snap = snap }, true);

                Debug.Log($"{Pfx} Saved snapshot to scene storage on '{GetHierarchyPath(sceneStorage.transform)}'.");

#if UNITY_EDITOR
                try
                {
                    EditorUtility.SetDirty(sceneStorage);
                    var scene = sceneStorage.gameObject.scene;
                    if (scene.IsValid())
                        EditorSceneManager.MarkSceneDirty(scene);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"{Pfx} MarkSceneDirty failed (editor-only): {e}");
                }
#endif
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{Pfx} SaveToScene failed: {e}");
            }
        }

        private bool TryLoadFromScene()
        {
            EnsureSceneStorage();
            if (sceneStorage == null)
            {
                Debug.Log($"{Pfx} Scene storage absent — nothing to load.");
                return false;
            }

            GraphSnapshotDTO snap = null;

            if (sceneStorage.snapshot != null && sceneStorage.snapshot.nodes != null && sceneStorage.snapshot.nodes.Count > 0)
            {
                snap = sceneStorage.snapshot;
                Debug.Log($"{Pfx} Loaded snapshot from scene storage (object) at {GetHierarchyPath(sceneStorage.transform)}.");
            }
            else if (!string.IsNullOrEmpty(sceneStorage.snapshotJson))
            {
                try
                {
                    var wrapper = JsonUtility.FromJson<Wrapper>(sceneStorage.snapshotJson);
                    if (wrapper?.snap != null) snap = wrapper.snap;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"{Pfx} Failed to parse snapshotJson from storage: {e}");
                }
            }

            if (snap == null)
            {
                Debug.Log($"{Pfx} Scene storage empty — nothing to load.");
                return false;
            }

            RebuildFromSnapshot(snap);
            return true;
        }

        // ====== Persistence: File (опция) ======
        private string ResolveJsonPath()
        {
            var path = string.IsNullOrWhiteSpace(jsonPath) ? "programs/bot.json" : jsonPath.Trim();
            if (Path.IsPathRooted(path)) return path;
            return Path.Combine(Application.persistentDataPath, path);
        }

        private void SaveToJson(GraphSnapshotDTO snap)
        {
            try
            {
                var path = ResolveJsonPath();
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var json = JsonUtility.ToJson(new Wrapper { snap = snap }, false);
                File.WriteAllText(path, json);
                Debug.Log($"{Pfx} Saved JSON → {path}");
            }
            catch (Exception e)
            {
                Debug.LogError($"{Pfx} SaveToJson failed: {e}");
            }
        }

        private bool TryLoadFromJson()
        {
            try
            {
                var path = ResolveJsonPath();
                if (!File.Exists(path))
                {
                    Debug.Log($"{Pfx} JSON not found, skip load: {path}");
                    return false;
                }

                var json = File.ReadAllText(path);
                var wrapper = JsonUtility.FromJson<Wrapper>(json);
                if (wrapper?.snap == null)
                {
                    Debug.LogWarning($"{Pfx} Loaded file but snapshot is NULL: {path}");
                    return false;
                }

                RebuildFromSnapshot(wrapper.snap);
                Debug.Log($"{Pfx} Loaded JSON ← {path}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"{Pfx} TryLoadFromJson failed: {e}");
                return false;
            }
        }

        [Serializable] private class Wrapper { public GraphSnapshotDTO snap; }

        // ====== helpers ======
        private void EnsurePrefabMapBuilt()
        {
            if (_prefabByKind != null) return;
            _prefabByKind = new Dictionary<NodeController.UINodeKind, GameObject>();
            foreach (var kp in kindPrefabs)
            {
                if (kp.prefab != null)
                    _prefabByKind[kp.kind] = kp.prefab;
            }
        }

        private static string GetHierarchyPath(Transform t)
        {
            if (t == null) return "<null>";
            var path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }
            return path;
        }

        private SlotController FindSlotById(IReadOnlyList<SlotController> slots, string slotId)
        {
            if (slots == null || string.IsNullOrEmpty(slotId)) return null;
            for (int i = 0; i < slots.Count; i++)
            {
                var s = slots[i];
                if (s != null && string.Equals(s.SlotId, slotId, StringComparison.Ordinal))
                    return s;
            }
            return null;
        }

        private class ConnectionRecord
        {
            public NodeController FromNode;
            public NodeController ToNode;
            public SlotController FromSlot;
            public SlotController ToSlot;
            public UIBezierConnection Line;
        }
    }
}
