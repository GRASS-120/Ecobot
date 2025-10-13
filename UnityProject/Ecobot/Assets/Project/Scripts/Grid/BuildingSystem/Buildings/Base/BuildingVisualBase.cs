using UnityEngine;

namespace Grid.BuildingSystem.Buildings.Base
{
    public abstract class BuildingVisualBase : MonoBehaviour
    {
        protected BuildingBase _building;
        protected BuildingContext _context;

        public virtual void Init(BuildingBase building, BuildingContext context)
        {
            _building = building;
            _context = context;
        }

        public virtual void SetPowered(bool isPowered) { }
        public virtual void OnBroken() { }
        public virtual void OnRepaired() { }
    }
}