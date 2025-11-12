using System;
using System.IO;
using UnityEngine;
using GUI.Programming.Graph;

public static class GraphSnapshotPersistence
{
    public static string BaseDirectory { get; set; } = Application.persistentDataPath;
    public static string DefaultFileName { get; set; } = "programming_overlay_snapshot.json";

    public static string MakePath(string subdir = null, string fileName = null)
    {
        var dir = string.IsNullOrEmpty(subdir) ? BaseDirectory : Path.Combine(BaseDirectory, subdir);
        try
        {
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
        }
        catch (Exception e)
        {
            Debug.LogError($"[Snapshot] Cannot ensure directory '{dir}': {e}");
        }
        return Path.Combine(dir, string.IsNullOrEmpty(fileName) ? DefaultFileName : fileName);
    }

    public static void Save(GraphSnapshotDTO snapshot, string customPath = null)
    {
        if (snapshot == null)
        {
            Debug.LogWarning("[Snapshot] Save called with NULL snapshot.");
            return;
        }

        try
        {
            var json = JsonUtility.ToJson(new Wrapper { snap = snapshot }, false);
            var path = string.IsNullOrEmpty(customPath) ? MakePath() : customPath;

            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(path, json);
            Debug.Log($"[Snapshot] Saved to: {path}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Snapshot] Save failed: {e}");
        }
    }

    public static bool TryLoad(out GraphSnapshotDTO snapshot, string customPath = null)
    {
        snapshot = null;
        try
        {
            var path = string.IsNullOrEmpty(customPath) ? MakePath() : customPath;
            if (!File.Exists(path))
            {
                Debug.Log($"[Snapshot] File not found: {path}");
                return false;
            }

            var json = File.ReadAllText(path);
            var w = JsonUtility.FromJson<Wrapper>(json);
            snapshot = w?.snap;

            if (snapshot == null)
            {
                Debug.LogWarning($"[Snapshot] Loaded file but snapshot is NULL: {path}");
                return false;
            }

            Debug.Log($"[Snapshot] Loaded from: {path}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Snapshot] Load failed: {e}");
            snapshot = null;
            return false;
        }
    }

    [Serializable]
    private class Wrapper { public GraphSnapshotDTO snap; }
}
