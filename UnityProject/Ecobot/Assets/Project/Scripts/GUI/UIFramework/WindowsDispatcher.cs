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
        // add cach
        [SerializeField] private WindowManager windowManager;
        
        [Header("Containers")]
        [SerializeField] private Transform overlayContainer;
        [SerializeField] private Transform popupsContainer;

        private readonly Dictionary<IWindowController, WindowView> _openedPopups = new();
        private WindowView _openedOverlay;

        public void OpenPopup(IWindowController controller)
        {
            if (popupsContainer == null)
            {
                Debug.LogError("Popups container is null.");
                return;
            }
            
            var prefabPath = windowManager.GetPrefabPath(controller);
            var view = CreateWindow(prefabPath, popupsContainer);
            
            if (view != null)
            {
                controller.Bind(view);
                _openedPopups.Add(controller, view);
            }
        }
        
        public void ClosePopup(IWindowController controller)
        {
            if (_openedPopups.TryGetValue(controller, out var view))
            {
                view?.Close();
                _openedPopups.Remove(controller);
            }
        }
        
        public void OpenOverlay(IWindowController controller)
        {
            if (controller == null) return;
            
            if (overlayContainer == null)
            {
                Debug.LogError("Overlay container is null.");
                return;
            }
            
            var prefabPath = windowManager.GetPrefabPath(controller);
            var view = CreateWindow(prefabPath, overlayContainer);
            
            if (view != null)
            {
                _openedOverlay?.Close();
                controller.Bind(view);
                _openedOverlay = view;
            }
        }
        
        private WindowView CreateWindow(string prefabPath, Transform container)
        {
            var prefab = Resources.Load<GameObject>($"UI/{prefabPath}");
            if (prefab == null)
            {
                Debug.LogError($"Failed to load prefab at path: UI/{prefabPath}");
                return null;
            }
            
            var instance = Instantiate(prefab, container);
            var view = instance.GetComponent<WindowView>();
            
            if (view == null)
            {
                Debug.LogError($"Prefab {prefabPath} doesn't have WindowView component");
                Destroy(instance);
                return null;
            }
            
            return view;
        }
    }
}