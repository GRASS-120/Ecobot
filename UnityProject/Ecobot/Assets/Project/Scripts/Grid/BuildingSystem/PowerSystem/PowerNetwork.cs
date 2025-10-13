using System.Collections.Generic;

namespace Grid.BuildingSystem.PowerSystem
{
    public class PowerNetwork
    {
        public int Id { get; }
        public List<IPowerNode> Nodes { get; } = new List<IPowerNode>();
        public int TotalProduction { get; internal set; }
        public int TotalConsumption { get; internal set; }
        
        public PowerNetwork(int id)
        {
            Id = id;
        }
    }
}