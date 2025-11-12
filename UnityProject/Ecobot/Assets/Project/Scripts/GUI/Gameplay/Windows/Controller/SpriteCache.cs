using System;
using System.Collections.Generic;
using Handbook.UI;
using UnityEngine;

namespace GUI.Gameplay.Windows.Controller
{
    public class SpriteCache : ISpriteCache
    {
        private readonly Dictionary<string, Sprite> _cache = new(StringComparer.OrdinalIgnoreCase);

        public bool TryGet(string path, out Sprite sprite) => _cache.TryGetValue(path, out sprite);

        public void Set(string path, Sprite sprite)
        {
            if (string.IsNullOrEmpty(path) || sprite == null) return;
            _cache[path] = sprite;
        }

        public void Clear() => _cache.Clear();

        public void Dispose()
        {
            foreach (var kv in _cache)
            {
                var s = kv.Value;
                if (s != null && s.texture != null)
                    UnityEngine.Object.Destroy(s.texture);
            }
            _cache.Clear();
        }
    }
}