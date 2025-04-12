using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Grid.BuildingSystem
{
    public class BuildingDatabase
    {
        public Dictionary<BuildingSO, List<Building>> BuildingsData; 
        
        public BuildingDatabase (BuildingListSO buildingListSO)
        {
            var buildingTypes = buildingListSO.buildings;
            
            BuildingsData = new Dictionary<BuildingSO, List<Building>>();

            foreach (var buildingType in buildingTypes)
            {
                BuildingsData[buildingType] = new List<Building>();
            }
        }

        public void Append(Building building)
        {
            if (!BuildingsData.ContainsKey(building.BuildingSO))
            {
                BuildingsData.Add(building.BuildingSO, new List<Building>());
            }
            
            BuildingsData[building.BuildingSO].Add(building);
        }

        public void Remove(Building building)
        {
            if (!BuildingsData.ContainsKey(building.BuildingSO)) return;
            
            BuildingsData[building.BuildingSO].Remove(building);

            if (BuildingsData[building.BuildingSO].Count == 0)
            {
                BuildingsData.Remove(building.BuildingSO);
            }
        }
    }
}