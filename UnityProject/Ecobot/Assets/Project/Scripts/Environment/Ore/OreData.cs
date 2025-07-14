using InteractionSystem;
using Inventory;
using UnityEngine;
using UnityEngine.Serialization;

namespace environment.Ore
{
    [CreateAssetMenu(menuName = "Project/Ore")]
    public class OreData : ScriptableObject
    {
        [SerializeField] private InventoryItemData oreItem;
        [SerializeField] private float miningTime = 1f;
        [SerializeField] private float capacity = 30f;
        
        public InventoryItemData OreItem => oreItem;
        public float MiningTime => miningTime;
        public float Capacity => capacity;
    }
}