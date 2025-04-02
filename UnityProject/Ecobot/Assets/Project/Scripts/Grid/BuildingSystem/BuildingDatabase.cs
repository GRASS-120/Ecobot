using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Grid.BuildingSystem
{
    public class BuildingDatabase
    {
        // + reactive? + odin? serializable
        private Dictionary<BuildingSO, List<Building>> _buildingsData;  // Building знает о всех занимаемых клетках
        
        public BuildingDatabase (BuildingListSO buildingListSO)
        {
            var buildingTypes = buildingListSO.buildings;
            
            _buildingsData = new Dictionary<BuildingSO, List<Building>>();

            foreach (var buildingType in buildingTypes)
            {
                _buildingsData[buildingType] = new List<Building>();
            }
        }
        

        public void Append(Building building)
        {
            if (!_buildingsData.ContainsKey(building.BuildingSO))
            {
                _buildingsData.Add(building.BuildingSO, new List<Building>());
            }
            
            _buildingsData[building.BuildingSO].Add(building);
        }

        // public void Remove()
        // {
        //     
        // }
    }
}