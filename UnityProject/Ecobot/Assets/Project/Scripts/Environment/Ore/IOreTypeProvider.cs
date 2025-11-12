// Assets/Project/Scripts/Environment/Ore/IOreTypeProvider.cs
namespace environment.Ore
{
    /// <summary>
    /// Сообщает тип руды ("Iron", "Coal", "Copper" и т.п.)
    /// для логики поиска.
    /// </summary>
    public interface IOreTypeProvider
    {
        string TypeId { get; }
    }
}