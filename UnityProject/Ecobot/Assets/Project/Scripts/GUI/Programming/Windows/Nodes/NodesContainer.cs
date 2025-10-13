using UnityEngine;

namespace GUI.Programming.Windows.Nodes
{
    /// <summary>
    /// Контейнер, внутри которого размещаются ноды.
    /// Хранит ссылку на контейнер соединений, чтобы ноды могли её получить автоматически.
    /// </summary>
    public class NodesContainer : MonoBehaviour
    {
        [Header("Connections Container")]
        [SerializeField] private RectTransform connectionsContainer;

        public RectTransform GetConnectionsContainer() => connectionsContainer;
    }
}