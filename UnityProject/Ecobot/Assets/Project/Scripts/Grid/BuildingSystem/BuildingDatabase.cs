using System;
using System.Collections.Generic;
using System.Linq;
using Grid.BuildingSystem.Buildings;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Grid.BuildingSystem
{
    public class BuildingDatabase
    {
        public Dictionary<BuildingAssetData, List<BuildingBase>> BuildingsData; 
        
        public BuildingDatabase (BuildingListConfig buildingListConfig)
        {
            var buildingTypes = buildingListConfig.buildings;
            
            BuildingsData = new Dictionary<BuildingAssetData, List<BuildingBase>>();

            foreach (var buildingType in buildingTypes)
            {
                BuildingsData[buildingType] = new List<BuildingBase>();
            }
        }

        public void Append(BuildingBase building)
        {
            if (!BuildingsData.ContainsKey(building.BuildingAssetData))
            {
                BuildingsData.Add(building.BuildingAssetData, new List<BuildingBase>());
            }
            
            BuildingsData[building.BuildingAssetData].Add(building);
        }

        public void Remove(BuildingBase building)
        {
            if (!BuildingsData.ContainsKey(building.BuildingAssetData)) return;
            
            BuildingsData[building.BuildingAssetData].Remove(building);

            if (BuildingsData[building.BuildingAssetData].Count == 0)
            {
                BuildingsData.Remove(building.BuildingAssetData);
            }
        }
    }
}