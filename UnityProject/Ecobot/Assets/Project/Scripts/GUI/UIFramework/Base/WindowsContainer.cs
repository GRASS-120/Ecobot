using System.Collections.Generic;
using UnityEngine;

namespace GUI.UIFramework.Base
{
    public class WindowsContainer : MonoBehaviour
    {
        [Header("Containers")]
        [SerializeField] private Transform screensContainer;
        [SerializeField] private Transform popupsContainer;

        private readonly Dictionary<WindowViewModel, IWindowBinder> _openedPopups = new();
        private IWindowBinder _openedScreen;

        public void OpenPopup(WindowViewModel viewModel)
        {
            var prefabPath = GetPrefabPath(viewModel);
            var prefab = Resources.Load<GameObject>(prefabPath);
            var createdPopup = Instantiate(prefab, popupsContainer);
            var binder = createdPopup.GetComponent<IWindowBinder>();
            
            binder.Bind(viewModel);
            _openedPopups.Add(viewModel, binder);
        }

        public void ClosePopup(WindowViewModel viewModel)
        {
            var binder = _openedPopups[viewModel];
            
            binder?.Close();
            _openedPopups.Remove(viewModel);
        }
        
        public void OpenScreen(WindowViewModel viewModel)
        {
            var prefabPath = GetPrefabPath(viewModel);
            var prefab = Resources.Load<GameObject>(prefabPath);
            var createdScreen = Instantiate(prefab, screensContainer);
            var binder = createdScreen.GetComponent<IWindowBinder>();
            
            binder.Bind(viewModel);
            _openedScreen = binder;
        }

        private static string GetPrefabPath(WindowViewModel viewModel)
        {
            return $"UI/{viewModel.Id}";
        }
    }
}