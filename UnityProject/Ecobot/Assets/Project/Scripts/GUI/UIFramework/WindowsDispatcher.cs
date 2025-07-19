using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace GUI.UIFramework
{
    /// <summary>
    /// Компонент для управления загрузкой и отображением окон UI.
    /// Отвечает за динамическое создание экранов и всплывающих окон из префабов
    /// и их размещение в соответствующий контейнер.
    /// </summary>
    public class WindowsDispatcher : MonoBehaviour
    {
        [Header("Containers")]
        [SerializeField] private Transform overlayContainer;
        [SerializeField] private Transform popupsContainer;

        private readonly Dictionary<IWindowController, WindowView> _openedPopups = new();
        private WindowView _openedOverlay;

        public void OpenPopup(IWindowController controller)
        {
            if (popupsContainer == null)
                Debug.LogError("Popups container is null.");
            
            var prefabPath = GetPrefabPath(controller);
            var prefab = Resources.Load<GameObject>(prefabPath);
            var createdPopup = Instantiate(prefab, popupsContainer);
            var view = createdPopup.GetComponent<WindowView>();
            
            controller.Bind(view);
            
            _openedPopups.Add(controller, view);
        }

        public void ClosePopup(IWindowController controller)
        {
            var view = _openedPopups[controller];
            
            view?.Close();
            _openedPopups.Remove(controller);
        }
        
        public void OpenOverlay(IWindowController controller)
        {
            if (controller == null) return;
            
            if (overlayContainer == null)
                Debug.LogError("Overlay container is null.");
            
            var prefabPath = GetPrefabPath(controller);
            var prefab = Resources.Load<GameObject>(prefabPath);
            var createdOverlay = Instantiate(prefab, overlayContainer);
            var view = createdOverlay.GetComponent<WindowView>();

            _openedOverlay?.Close();
            
            controller.Bind(view);
            _openedOverlay = view;
        }

        private static string GetPrefabPath(IWindowController controller) => $"UI/{controller.Id}";
    }
}