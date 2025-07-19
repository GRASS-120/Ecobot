namespace GUI.UIFramework
{
    /// <summary>
    /// Абстрактный базовый класс для экранных окон, занимающих большую часть экрана (например, HUD или меню).
    /// </summary>
    /// <typeparam name="T">Конкретный ViewModel (унаследован от <see cref="WindowController"/>).</typeparam>
    public abstract class OverlayView : WindowView
    {
        
    }
}