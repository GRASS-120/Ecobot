// Assets/Project/Scripts/Environment/Ore/OreApproachProvider.cs
using Bot.Programming.Navigation;
using UnityEngine;

namespace environment.Ore
{
    /// <summary>
    /// Провайдер точки подъезда и типа руды.
    /// Отвечает и за подъезд, и за идентификацию "что это за руда".
    /// </summary>
    [RequireComponent(typeof(Ore))]
    [DisallowMultipleComponent]
    public class OreApproachProvider : MonoBehaviour, IApproachPointProvider, IOreTypeProvider
    {
        [Header("Ore Type (for AI search)")]
        [SerializeField] private string oreType = "Iron"; // ← здесь задаёшь "Iron", "Coal", "Copper" и т.д.

        private OreGridOccupant _occupant;

        private void Awake()
        {
            _occupant = GetComponent<OreGridOccupant>();
            if (_occupant == null)
                _occupant = gameObject.AddComponent<OreGridOccupant>(); // дефолт 1x1
        }

        // --- IApproachPointProvider ---
        public bool TryGetApproachPoint(Vector3 botWorldPosition, out Vector3 worldPos)
        {
            worldPos = default;
            if (_occupant == null) return false;

            // GridApproach сам найдёт ближайшую свободную клетку вокруг всех занятых клеток
            return GridApproach.TryFindApproach(botWorldPosition, _occupant.EnumerateCells(), out worldPos);
        }

        // --- IOreTypeProvider ---
        public string TypeId => oreType;
    }
}