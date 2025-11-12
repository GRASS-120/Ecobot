using UnityEngine;

namespace GUI.Programming.Graph
{
    /// <summary>
    /// Простое сценовое хранилище графа: сериализуем и как объект, и как JSON (для наглядности).
    /// Добавь этот компонент на любой GO в сцене, либо NodeGraphController сам его создаст.
    /// </summary>
    public sealed class NodeGraphSceneStorage : MonoBehaviour
    {
        [Header("Human-readable JSON copy (optional)")]
        [TextArea(4, 32)]
        public string snapshotJson;

        [Header("Live snapshot (serialized by Unity)")]
        public GraphSnapshotDTO snapshot; // Unity умеет сериализовать, если DTO помечены [Serializable]
    }
}