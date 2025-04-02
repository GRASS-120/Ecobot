using System.Collections.Generic;
using UnityEngine;

namespace Grid.BuildingSystem
{
    [CreateAssetMenu(fileName = "FILENAME", menuName = "Scriptable Objects/Building System/BuildingList SO", order = 1)]
    public class BuildingListSO : ScriptableObject
    {
        public List<BuildingSO> buildings = new List<BuildingSO>();
    }
}