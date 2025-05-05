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

        private readonly Dictionary<WindowViewModel, IWindowView> _openedPopups = new();
        private IWindowView _openedOverlay;

        public void OpenPopup(WindowViewModel viewModel)
        {
            if (popupsContainer == null)
                Debug.LogError("Popups container is null.");
            
            var prefabPath = GetPrefabPath(viewModel);
            var prefab = Resources.Load<GameObject>(prefabPath);
            var createdPopup = Instantiate(prefab, popupsContainer);
            var view = createdPopup.GetComponent<IWindowView>();
            
            view.Bind(viewModel);
            _openedPopups.Add(viewModel, view);
        }

        public void ClosePopup(WindowViewModel viewModel)
        {
            var view = _openedPopups[viewModel];
            
            view?.Close();
            _openedPopups.Remove(viewModel);
        }
        
        public void OpenOverlay(WindowViewModel viewModel)
        {
            if (viewModel == null) return;
            
            if (overlayContainer == null)
                Debug.LogError("Overlay container is null.");
            
            var prefabPath = GetPrefabPath(viewModel);
            var prefab = Resources.Load<GameObject>(prefabPath);
            var createdOverlay = Instantiate(prefab, overlayContainer);
            var view = createdOverlay.GetComponent<IWindowView>();

            _openedOverlay?.Close();
            
            view.Bind(viewModel);
            _openedOverlay = view;
        }

        private static string GetPrefabPath(WindowViewModel viewModel) => $"UI/{viewModel.Id}";
    }
}