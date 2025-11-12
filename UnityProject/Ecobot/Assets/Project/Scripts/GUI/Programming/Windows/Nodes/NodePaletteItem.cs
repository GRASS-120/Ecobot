using UnityEngine;
using UnityEngine.EventSystems;

public class NodePaletteItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Node Setup")]
    [SerializeField] private GameObject nodePrefab; // Префаб ноды
    [SerializeField] private RectTransform nodesParent; // Сюда нода будет спавниться

    private GameObject _previewNode;

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Создаём временный "призрак" ноды во время перетаскивания
        _previewNode = Instantiate(nodePrefab, nodesParent);
        _previewNode.transform.SetAsLastSibling();

        var rect = _previewNode.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            nodesParent,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint);
        rect.anchoredPosition = localPoint;

        CanvasGroup cg = _previewNode.AddComponent<CanvasGroup>();
        cg.alpha = 0.6f; // полупрозрачная нода во время перетаскивания
        cg.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_previewNode == null) return;

        var rect = _previewNode.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            nodesParent,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint);
        rect.anchoredPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_previewNode == null) return;

        var rect = _previewNode.GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            nodesParent,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint);
        rect.anchoredPosition = localPoint;

        // Теперь делаем её полноценной нодой
        Destroy(_previewNode.GetComponent<CanvasGroup>());

        _previewNode = null;
    }
}
