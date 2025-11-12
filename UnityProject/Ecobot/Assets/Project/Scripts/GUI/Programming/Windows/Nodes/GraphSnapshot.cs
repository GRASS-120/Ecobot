using System;
using System.Collections.Generic;
using UnityEngine;
using GUI.Programming.Windows.Nodes; // UINodeKind

namespace GUI.Programming.Graph
{
    public enum PortKind { Stream, Data }

    [Serializable]
    public class GraphNodeDTO
    {
        public int id;
        public NodeController.UINodeKind type;
        public Vector2 position;
        public string dropdownTech;   // технический ключ
        public string dropdownVisual; // визуальное имя
    }

    [Serializable]
    public class GraphEdgeDTO
    {
        public int    fromId;
        public string fromSlotId;
        public PortKind fromKind;

        public int    toId;
        public string toSlotId;
        public PortKind toKind;
    }

    [Serializable]
    public class GraphSnapshotDTO
    {
        public List<GraphNodeDTO> nodes = new();
        public List<GraphEdgeDTO> edges = new();
    }
}