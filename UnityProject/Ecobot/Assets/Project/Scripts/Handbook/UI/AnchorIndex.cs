using System;
using System.Collections.Generic;
using UnityEngine;

namespace Handbook.UI
{
    public class AnchorIndex
    {
        private readonly Dictionary<string, RectTransform> _map = new(StringComparer.OrdinalIgnoreCase);

        public void Register(string anchorId, RectTransform target)
        {
            if (string.IsNullOrWhiteSpace(anchorId) || target == null) return;
            var key = anchorId.Trim().ToLowerInvariant();
            _map[key] = target;
        }

        public bool TryGet(string anchorId, out RectTransform target)
        {
            if (string.IsNullOrWhiteSpace(anchorId))
            {
                target = null;
                return false;
            }
            return _map.TryGetValue(anchorId.Trim().ToLowerInvariant(), out target);
        }

        public void Clear() => _map.Clear();
    }
}