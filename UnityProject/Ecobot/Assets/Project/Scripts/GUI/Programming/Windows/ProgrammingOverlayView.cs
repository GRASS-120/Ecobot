using GUI.UIFramework;
using UnityEngine;
using UnityEngine.UI;

namespace GUI.Programming.Windows
{
    public class ProgrammingOverlayView : OverlayView
    {
        [SerializeField] private Button btnClose;
        public Button BtnClose => btnClose;

        [SerializeField] private GUI.Programming.Graph.NodeGraphController graph;
        public GUI.Programming.Graph.NodeGraphController Graph => graph;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (graph == null)
                graph = GetComponentInChildren<GUI.Programming.Graph.NodeGraphController>(true);
        }
#endif
    }
}