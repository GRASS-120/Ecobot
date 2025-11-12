using System;
using System.Collections.Generic;
using UnityEngine;
using GUI.Programming.Windows.Nodes;

[CreateAssetMenu(menuName = "Programming/Node Prefab Registry")]
public class NodePrefabRegistry : ScriptableObject
{
    [Serializable]
    public struct Entry
    {
        public NodeController.UINodeKind kind;
        public GameObject prefab;
    }

    [SerializeField] private List<Entry> entries = new();

    public GameObject GetPrefab(NodeController.UINodeKind kind)
    {
        foreach (var e in entries)
            if (e.kind == kind) return e.prefab;
        return null;
    }
}