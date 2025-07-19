using UnityEngine;

namespace GUI.UIFramework
{
    /// <summary>
    /// Базовый абстрактный класс для всех оконных представлений UI.
    /// Обеспечивает механизм привязки модели представления к окну и стандартное закрытие окна.
    /// </summary>
    /// <typeparam name="T">Тип модели представления, наследуемой от <see cref="WindowController"/>.</typeparam>
    public abstract class WindowView : MonoBehaviour
    {
        public void Open()
        {
            OnOpen();
        }

        public void Close()
        {
            // Сначала уничтожаем, затем делаем анимации на закрытие
            OnClose();
            
            Destroy(gameObject);
        }
        
        protected virtual void OnOpen() { }
        protected virtual void OnClose() { }
    }
}