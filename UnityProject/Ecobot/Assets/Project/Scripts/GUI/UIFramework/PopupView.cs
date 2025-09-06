using R3;
using UnityEngine;
using UnityEngine.UI;

namespace GUI.UIFramework
{
    /// <summary>
    /// Абстрактный класс для всплывающих окон, отображающих небольшие элементы интерфейса поверх основного экрана.
    /// </summary>
    /// <typeparam name="T">Конкретный ViewModel (унаследован от <see cref="WindowController"/>).</typeparam>
    public abstract class PopupView : WindowView
    {
        
    }
}