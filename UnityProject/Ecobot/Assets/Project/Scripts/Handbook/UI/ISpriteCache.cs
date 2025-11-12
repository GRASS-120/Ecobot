using UnityEngine;

namespace Handbook.UI
{
    public interface ISpriteCache
    {
        bool TryGet(string path, out Sprite sprite);
        void Set(string path, Sprite sprite);
        void Clear();
    }
}