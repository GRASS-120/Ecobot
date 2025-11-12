using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GUI.Programming.Graph
{
    /// <summary>
    /// Хранит снапшоты графов всех ботов в одном JSON-файле.
    /// Формат:
    /// {
    ///   "entries": [
    ///     { "botId": "Bot#123", "snap": { ...GraphSnapshotDTO... } },
    ///     ...
    ///   ]
    /// }
    /// </summary>
    public static class GraphMultiBotStorage
    {
        private const string DefaultFileName = "programming_graphs.json";

        [Serializable]
        private class Entry
        {
            public string botId;
            public GraphSnapshotDTO snap;
        }

        [Serializable]
        private class FileData
        {
            public List<Entry> entries = new();
        }

        private static string _cachedPath;
        private static string FilePath
        {
            get
            {
                if (string.IsNullOrEmpty(_cachedPath))
                {
                    var dir = Application.persistentDataPath;
                    _cachedPath = Path.Combine(dir, DefaultFileName);
                }
                return _cachedPath;
            }
        }

        private static FileData LoadFile()
        {
            try
            {
                var path = FilePath;
                if (!File.Exists(path))
                    return new FileData();

                var json = File.ReadAllText(path);
                var data = JsonUtility.FromJson<FileData>(json);
                return data ?? new FileData();
            }
            catch (Exception e)
            {
                Debug.LogError($"[GraphStorage] Load failed: {e}");
                return new FileData();
            }
        }

        private static void SaveFile(FileData data)
        {
            try
            {
                var json = JsonUtility.ToJson(data, false);
                File.WriteAllText(FilePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[GraphStorage] Save failed: {e}");
            }
        }

        public static void SaveForBot(string botId, GraphSnapshotDTO snap)
        {
            if (string.IsNullOrEmpty(botId) || snap == null) return;

            var data = LoadFile();
            var idx = data.entries.FindIndex(e => e.botId == botId);
            if (idx >= 0) data.entries[idx].snap = snap;
            else data.entries.Add(new Entry { botId = botId, snap = snap });

            SaveFile(data);
            Debug.Log($"[GraphStorage] Saved snapshot for botId='{botId}' (nodes={snap.nodes?.Count ?? 0}, edges={snap.edges?.Count ?? 0}) to {FilePath}");
        }

        public static bool TryLoadForBot(string botId, out GraphSnapshotDTO snap)
        {
            snap = null;
            if (string.IsNullOrEmpty(botId)) return false;

            var data = LoadFile();
            var entry = data.entries.Find(e => e.botId == botId);
            if (entry?.snap == null) return false;

            snap = entry.snap;
            Debug.Log($"[GraphStorage] Loaded snapshot for botId='{botId}' (nodes={snap.nodes?.Count ?? 0}, edges={snap.edges?.Count ?? 0}) from {FilePath}");
            return true;
        }
    }
}
