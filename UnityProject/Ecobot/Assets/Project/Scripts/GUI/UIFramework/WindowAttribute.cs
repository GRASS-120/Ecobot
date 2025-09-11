using System;

namespace GUI.UIFramework
{
    [AttributeUsage(AttributeTargets.Class)]
    public class WindowAttribute : Attribute
    {
        public WindowType WindowType { get; }
        public string PrefabPath { get; }
        
        public WindowAttribute(WindowType windowType, string prefabPath)
        {
            WindowType = windowType;
            PrefabPath = prefabPath;
        }
    }
}