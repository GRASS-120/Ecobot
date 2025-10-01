using System.Collections.Generic;
using UnityEngine;

namespace Grid.BuildingSystem
{
    [CreateAssetMenu(fileName = "FILENAME", menuName = "Scriptable Objects/Building System/BuildingList SO", order = 1)]
    public class BuildingListConfig : ScriptableObject
    {
        public List<BuildingAssetData> buildings = new ();
    }
}