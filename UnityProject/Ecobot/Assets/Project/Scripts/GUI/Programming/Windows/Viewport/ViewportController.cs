using GUI.Programming.Windows.Nodes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GUI.Programming.Windows.Viewport
{
    public class ViewportController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IScrollHandler
    {
        [Header("Components")]
        [SerializeField] private RectTransform canvasRectTransform;
        [SerializeField] private RectTransform viewportRectTransform; 
        
        [Header("Zoom Settings")]
        [SerializeField] private float zoomSpeed = 0.1f;
        [SerializeField] private float minZoom = 0.5f;
        [SerializeField] private float maxZoom = 3f;
    
        [Header("Drag Settings")]
        [SerializeField] private bool dragWithMiddleMouse = true;
        [SerializeField] private bool dragWithRightMouse;
        [SerializeField] private bool clampToBounds = true; // Ограничить перемещение границами видимой области
        
        private Vector2 _lastMousePosition;
        
        public void OnPointerUp(PointerEventData eventData)
        {
            // Debug.Log("OnPointerUp");
            if ((dragWithMiddleMouse && eventData.button == PointerEventData.InputButton.Middle) ||
                (dragWithRightMouse && eventData.button == PointerEventData.InputButton.Right))
            {
                // Предотвращаем обработку события другими элементами
                eventData.Use();
            }
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            // Debug.Log("OnPointerDown");
            // Проверяем, что нажата нужная кнопка мыши
            if ((dragWithMiddleMouse && eventData.button == PointerEventData.InputButton.Middle) ||
                (dragWithRightMouse && eventData.button == PointerEventData.InputButton.Right))
            {
                _lastMousePosition = eventData.position;
            
                // Предотвращаем обработку события другими элементами
                eventData.Use();
            }
            
            //при нажатии на viewport удаляем все активные соединения
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                CancelAllActiveConnections();
            }
            
            
        }
        
        private void CancelAllActiveConnections()
        {
            // Находим все ноды и отменяем их активные соединения
            NodeController[] allNodes = FindObjectsByType<NodeController>(FindObjectsSortMode.None);
            foreach (var node in allNodes)
            {
                if (node.HasActiveConnection)
                {
                    node.CancelActiveConnection();
                    Debug.Log("Активное соединение отменено по клику на пустую область");
                }
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            // Debug.Log("OnDrag");
            // Проверяем, что перетаскивание происходит нужной кнопкой мыши
            if ((dragWithMiddleMouse && eventData.button == PointerEventData.InputButton.Middle) ||
                (dragWithRightMouse && eventData.button == PointerEventData.InputButton.Right))
            {
                Vector2 currentMousePosition = eventData.position;
                Vector2 mouseDelta = currentMousePosition - _lastMousePosition;
            
                // Перемещаем полотно
                Vector2 newPosition = canvasRectTransform.anchoredPosition + mouseDelta;
            
                // Если включено ограничение границами
                if (clampToBounds && viewportRectTransform != null)
                {
                    newPosition = ClampPositionToBounds(newPosition);
                }
            
                canvasRectTransform.anchoredPosition = newPosition;
            
                _lastMousePosition = currentMousePosition;
            
                eventData.Use();
            }
        }

        public void OnScroll(PointerEventData eventData)
        {
            var scrollDelta = eventData.scrollDelta.y;
            var currentScale = canvasRectTransform.localScale;
        
            var newScale = Mathf.Clamp(currentScale.x + scrollDelta * zoomSpeed, minZoom, maxZoom);
        
            // Получаем позицию курсора относительно полотна
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRectTransform, 
                eventData.position, 
                eventData.pressEventCamera, 
                out var mousePositionOnCanvas);
        
            // Рассчитываем новую позицию полотна, чтобы масштабирование происходило относительно курсора
            Vector2 pivotDelta = mousePositionOnCanvas * (newScale / currentScale.x - 1);
        
            // Применяем новый масштаб
            canvasRectTransform.localScale = new Vector3(newScale, newScale, 1f);
        
            // Корректируем позицию полотна
            Vector2 newPosition = canvasRectTransform.anchoredPosition - pivotDelta;
        
            if (clampToBounds)
                newPosition = ClampPositionToBounds(newPosition);
        
            canvasRectTransform.anchoredPosition = newPosition;
            
        }

        // Ограничение позиции полотна границами видимой области
        private Vector2 ClampPositionToBounds(Vector2 position)
        {
            if (viewportRectTransform == null) return position;
        
            Rect contentRect = canvasRectTransform.rect;
            Rect viewportRect = viewportRectTransform.rect;
        
            var contentWidth = contentRect.width * canvasRectTransform.localScale.x;
            var contentHeight = contentRect.height * canvasRectTransform.localScale.y;
        
            // Рассчитываем минимальные и максимальные значения для позиции
            var minX = viewportRect.width / 2 - contentWidth / 2;
            var maxX = -minX;
            var minY = viewportRect.height / 2 - contentHeight / 2;
            var maxY = -minY;
        
            // Если контент меньше вьюпорта, центрируем его
            if (contentWidth <= viewportRect.width)
            {
                position.x = 0;
            }
            else
            {
                position.x = Mathf.Clamp(position.x, minX, maxX);
            }
        
            if (contentHeight <= viewportRect.height)
            {
                position.y = 0;
            }
            else
            {
                position.y = Mathf.Clamp(position.y, minY, maxY);
            }
        
            return position;
        }
    }
}