using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace GUI.UIFramework
{
    public class WindowManager : MonoBehaviour
    {
        private readonly Dictionary<Type, IWindowController> _cachedControllers = new ();
        private readonly Dictionary<Type, WindowAttribute> _controllerAttributes = new ();
        private UIRootController _rootController;

        private void Awake()
        {
            CacheControllerTypes();
        }

        public void Init(UIRootController rootController)
        {
            _rootController = rootController;
        }

        private void CacheControllerTypes()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => typeof(IWindowController).IsAssignableFrom(type)
                    && !type.IsAbstract
                    && !type.IsInterface
                    && type.GetCustomAttribute<WindowAttribute>() != null);

            foreach (var type in types)
            {
                var attribute = type.GetCustomAttribute<WindowAttribute>();
                _controllerAttributes[type] = attribute;
            }
        }
        
        public T GetController<T>() where T : class, IWindowController
        {
            var type = typeof(T);
            
            if (_cachedControllers.TryGetValue(type, out var cachedController))
            {
                return cachedController as T;
            }
            
            // Создаем новый экземпляр через рефлексию
            var controller = CreateController<T>();
            _cachedControllers[type] = controller;
            
            return controller;
        }

        private T CreateController<T>() where T : class, IWindowController
        {
            var type = typeof(T);

            try
            {
                // почему здесь обязательно надо указывать t : class?
                var controller = Activator.CreateInstance(type) as T; // почему именно так создаем экз?
                return controller;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create controller {type.Name}: {e.Message}");
                return null;
            }
        }
        
        public T OpenWindow<T>() where T : class, IWindowController
        {
            var controller = GetController<T>();
            
            if (controller == null)
            {
                Debug.LogError($"Failed to get controller for type {typeof(T).Name}");
                return null;
            }
            
            switch (controller.WindowType)
            {
                case WindowType.Overlay:
                    _rootController.OpenOverlay(controller);
                    break;
                case WindowType.Popup:
                    _rootController.OpenPopup(controller);
                    break;
            }
            
            return controller;
        }
        
        public T OpenWindow<T>(Action<T> initAction) where T : class, IWindowController
        {
            var controller = GetController<T>();
    
            if (controller == null)
            {
                Debug.LogError($"Failed to get controller for type {typeof(T).Name}");
                return null;
            }
    
            initAction?.Invoke(controller);
    
            switch (controller.WindowType)
            {
                case WindowType.Overlay:
                    _rootController.OpenOverlay(controller);
                    break;
                case WindowType.Popup:
                    _rootController.OpenPopup(controller);
                    break;
            }
    
            return controller;
        }
        
        public void CloseWindow<T>() where T : class, IWindowController
        {
            var controller = GetController<T>();
            controller?.Close();
        }
        
        public string GetPrefabPath<T>() where T : class, IWindowController
        {
            var type = typeof(T);
            if (_controllerAttributes.TryGetValue(type, out var attribute))
            {
                return attribute.PrefabPath;
            }
            
            Debug.LogError($"No WindowAttribute found for controller {type.Name}");
            return null;
        }
        
        public string GetPrefabPath(IWindowController controller)
        {
            var type = controller.GetType();
            if (_controllerAttributes.TryGetValue(type, out var attribute))
            {
                return attribute.PrefabPath;
            }
            
            Debug.LogError($"No WindowAttribute found for controller {type.Name}");
            return controller.Id; // fallback
        }
    }
}