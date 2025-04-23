using System.Collections.Generic;
using UnityEngine;

namespace GUI.UIFramework.Base
{
    /// <summary>
    /// Класс, который предоставляет методы для загрузки окон в соответствующий Transform
    /// </summary>
    public class WindowsDispatcher : MonoBehaviour
    {
        [Header("Containers")]
        [SerializeField] private Transform screensContainer;
        [SerializeField] private Transform popupsContainer;

        private readonly Dictionary<WindowViewModel, IWindowView> _openedPopups = new();
        private IWindowView _openedScreen;

        public void OpenPopup(WindowViewModel viewModel)
        {
            var prefabPath = GetPrefabPath(viewModel);
            var prefab = Resources.Load<GameObject>(prefabPath);
            var createdPopup = Instantiate(prefab, popupsContainer);
            var view = createdPopup.GetComponent<IWindowView>();
            
            Debug.Log($"prefab: {createdPopup}");

            
            view.Bind(viewModel);
            _openedPopups.Add(viewModel, view);
        }

        public void ClosePopup(WindowViewModel viewModel)
        {
            var view = _openedPopups[viewModel];
            
            view?.Close();
            _openedPopups.Remove(viewModel);
        }
        
        public void OpenScreen(WindowViewModel viewModel)
        {
            if (viewModel == null)
            {
                return;
            }
            
            var prefabPath = GetPrefabPath(viewModel);
            var prefab = Resources.Load<GameObject>(prefabPath);
            var createdScreen = Instantiate(prefab, screensContainer);
            var view = createdScreen.GetComponent<IWindowView>();
            
            _openedScreen?.Close();
            Debug.Log($"prefab: {createdScreen}");

            
            view.Bind(viewModel);
            _openedScreen = view;
        }

        private static string GetPrefabPath(WindowViewModel viewModel)
        {
            Debug.Log($"UI/{viewModel.Id}");
            return $"UI/{viewModel.Id}";
        }
    }
}