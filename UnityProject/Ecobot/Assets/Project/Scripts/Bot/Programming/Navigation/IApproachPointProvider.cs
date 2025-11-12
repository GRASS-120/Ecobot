// Assets/Project/Scripts/Bot/Programming/Navigation/IApproachPointProvider.cs
using UnityEngine;

namespace Bot.Programming.Navigation
{
    /// <summary>
    /// Провайдер точки подъезда: висит на ЦЕЛИ (здании, руде и т.д.)
    /// и возвращает мировую точку, куда боту подъехать.
    /// Никаких ссылок на BotBase — только позиция бота.
    /// </summary>
    public interface IApproachPointProvider
    {
        /// <param name="botWorldPosition">текущая мировая позиция бота (для выбора ближайшей клетки)</param>
        /// <param name="approachWorldPosition">результирующая мировая точка для подъезда</param>
        /// <returns>true, если удалось вычислить точку</returns>
        bool TryGetApproachPoint(Vector3 botWorldPosition, out Vector3 approachWorldPosition);
    }
}