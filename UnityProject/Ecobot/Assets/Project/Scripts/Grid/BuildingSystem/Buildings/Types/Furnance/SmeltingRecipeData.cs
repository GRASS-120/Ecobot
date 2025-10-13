using Inventory;
using UnityEngine;

namespace Grid.BuildingSystem.Buildings.Types.Furnance
{
    [CreateAssetMenu(menuName = "Project/Smelting Recipe")]
    public class SmeltingRecipeData : ScriptableObject
    {
        [Header("Input")]
        public InventoryItemData inputItem;         
        [Min(1)] public int inputAmountPerOutput = 1;

        [Header("Output")]
        public InventoryItemData resultItem;        
        [Min(1)] public int resultAmount = 1;

        [Header("Process")]
        [Min(0.1f)] public float smeltTimeSeconds = 2f;
        [Min(0)] public int fuelPerOutput = 1;      
    }
}