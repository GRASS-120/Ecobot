using GUI.UIFramework.Base;

namespace GUI.UIFramework
{
    /// <summary>
    /// Абстрактный базовый класс для экранных окон, занимающих большую часть экрана (например, HUD или меню).
    /// </summary>
    /// <typeparam name="T">Конкретный ViewModel (унаследован от <see cref="WindowViewModel"/>).</typeparam>
    public abstract class OverlayView<T> : WindowView<T> where T : WindowViewModel
    {
        
    }
}