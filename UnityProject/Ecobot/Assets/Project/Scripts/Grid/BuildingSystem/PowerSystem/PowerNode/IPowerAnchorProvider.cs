using UnityEngine;

namespace Grid.BuildingSystem.PowerSystem
{
    public interface IPowerAnchorProvider
    {
        Transform WireAnchor { get; }
    }
}