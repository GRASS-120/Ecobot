using System;
using UnityEditor;
using UnityEngine;

namespace Inventory
{
    [CreateAssetMenu(menuName = "Project/Inventory Item")]
    public class InventoryItemData : ScriptableObject
    {
        [SerializeField] private string guid;
        public string displayName;
        [TextArea(4, 4)]
        public string description;
        public Sprite icon;
        public int maxStackValue;
        
        public string ID => guid;
        
        private void OnValidate()
        {
#if UNITY_EDITOR

            if (string.IsNullOrEmpty(guid))
            {
                GenerateGuid();
            }
#endif
        }

#if UNITY_EDITOR
        [ContextMenu("Generate New GUID")]
        private void GenerateGuid()
        {
            guid = Guid.NewGuid().ToString();
            EditorUtility.SetDirty(this);
        }
#endif
    }
}
