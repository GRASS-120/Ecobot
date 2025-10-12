
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GUI.Programming.Windows.Nodes;
using GUI.Programming.Windows.Slots;

[RequireComponent(typeof(CanvasRenderer))]
public class UIBezierConnection : MaskableGraphic
{
    [Header("Connection Points")]
    public RectTransform startSlot;
    public RectTransform endSlot;

    [HideInInspector] public SlotController AssociatedOutput;
    [HideInInspector] public SlotController AssociatedInput;

    [Header("Line Settings")]
    public float thickness = 3f;
    public Color lineColor = Color.white;
    public Color selectedColor = Color.yellow;
    public float curveIntensity = 50f;
    public int segmentCount = 20;

    [Header("Materials")]
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material validMaterial;
    [SerializeField] private Material invalidMaterial;

    public bool IsInteractable { get; private set; }
    public bool IsPreview { get; private set; }
    public bool IsSelected { get; private set; }

    private RectTransform _container;
    private RectTransform _rt;
    private RectTransform _circle;
    private Image _circleImage;

    private Vector3 _lastStartPos;
    private Vector3 _lastEndPos;
    private Vector3 _lastContainerPos;
    private Vector3 _lastContainerScale;

    protected override void Awake()
    {
        base.Awake();
        _rt = GetComponent<RectTransform>();
        useLegacyMeshGeneration = false;
        raycastTarget = false;
        maskable = false;

        if (normalMaterial != null)
            material = normalMaterial;
    }

    private void Start()
    {
        CreateSelectionCircle();
    }

    private void CreateSelectionCircle()
    {
        GameObject circleObj = new GameObject("SelectionCircle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        _circle = circleObj.GetComponent<RectTransform>();
        _circle.SetParent(transform, false);
        _circle.sizeDelta = new Vector2(16f, 16f);

        _circleImage = circleObj.GetComponent<Image>();
        _circleImage.color = new Color(1f, 1f, 1f, 0.5f);
        _circleImage.raycastTarget = true;

        Button btn = circleObj.GetComponent<Button>();
        btn.onClick.AddListener(ToggleSelection);

        _circle.SetAsLastSibling();
    }

    public void SetContainer(RectTransform container)
    {
        _container = container;
        if (_rt != null && _container != null)
        {
            _rt.SetParent(_container, false);
            UpdateRectTransform();
            SetAllDirty();
        }
    }

    public void SetStartSlot(RectTransform start)
    {
        startSlot = start;
        UpdateRectTransform();
        SetAllDirty();
    }

    public void SetEndSlot(RectTransform end)
    {
        endSlot = end;
        UpdateRectTransform();
        SetAllDirty();
    }

    public void SetInteractable(bool state)
    {
        IsInteractable = state;
        IsPreview = false;
        if (normalMaterial != null)
            material = normalMaterial;
        SetAllDirty();
    }

    public void SetPreviewState(bool isValid)
    {
        IsPreview = true;
        IsInteractable = false;

        if (isValid && validMaterial != null)
            material = validMaterial;
        else if (!isValid && invalidMaterial != null)
            material = invalidMaterial;

        SetAllDirty();
    }

    public void ClearPreview()
    {
        if (!IsPreview) return;
        IsPreview = false;
        if (normalMaterial != null)
            material = normalMaterial;
        SetAllDirty();
    }

    private void UpdateRectTransform()
    {
        if (_container == null || _rt == null || startSlot == null)
            return;

        Vector2 p0 = _container.InverseTransformPoint(startSlot.position);
        Vector2 p3 = endSlot != null ? _container.InverseTransformPoint(endSlot.position) : p0;
        Vector2 cp1 = p0 + Vector2.right * curveIntensity;
        Vector2 cp2 = p3 + Vector2.left * curveIntensity;

        Vector2 min = Vector2.Min(Vector2.Min(p0, p3), Vector2.Min(cp1, cp2));
        Vector2 max = Vector2.Max(Vector2.Max(p0, p3), Vector2.Max(cp1, cp2));

        float pad = Mathf.Max(thickness, 10f);
        min -= new Vector2(pad, pad);
        max += new Vector2(pad, pad);

        Vector2 size = max - min;
        Vector2 center = (min + max) * 0.5f;

        _rt.anchorMin = _rt.anchorMax = new Vector2(0.5f, 0.5f);
        _rt.pivot = new Vector2(0.5f, 0.5f);
        _rt.anchoredPosition = center;
        _rt.sizeDelta = size;
        _rt.localScale = Vector3.one;
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (_container == null || startSlot == null || endSlot == null)
            return;

        Vector2 startLocal = _rt.InverseTransformPoint(startSlot.position);
        Vector2 endLocal = _rt.InverseTransformPoint(endSlot.position);
        Vector2 control1 = startLocal + Vector2.right * curveIntensity;
        Vector2 control2 = endLocal + Vector2.left * curveIntensity;

        var col = IsSelected ? selectedColor : lineColor;

        for (int i = 0; i < segmentCount; i++)
        {
            float t1 = i / (float)segmentCount;
            float t2 = (i + 1) / (float)segmentCount;

            Vector2 p1 = CalculateBezierPoint(t1, startLocal, control1, control2, endLocal);
            Vector2 p2 = CalculateBezierPoint(t2, startLocal, control1, control2, endLocal);

            Vector2 n = new Vector2(-(p2 - p1).y, (p2 - p1).x);
            if (n == Vector2.zero) n = Vector2.up;
            n = n.normalized * (thickness * 0.5f);

            int idx = vh.currentVertCount;
            vh.AddVert(p1 - n, col, Vector2.zero);
            vh.AddVert(p1 + n, col, Vector2.zero);
            vh.AddVert(p2 + n, col, Vector2.zero);
            vh.AddVert(p2 - n, col, Vector2.zero);

            vh.AddTriangle(idx, idx + 1, idx + 2);
            vh.AddTriangle(idx + 2, idx + 3, idx);
        }
    }

    private Vector2 CalculateBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        float u = 1 - t;
        return u * u * u * p0 +
               3f * u * u * t * p1 +
               3f * u * t * t * p2 +
               t * t * t * p3;
    }

    private void LateUpdate()
    {
        if (_container == null || startSlot == null) return;

        Vector3 startPos = startSlot.position;
        Vector3 endPos = endSlot != null ? endSlot.position : startPos;

        bool changed =
            _lastStartPos != startPos ||
            _lastEndPos != endPos ||
            _lastContainerPos != (_container != null ? _container.position : Vector3.zero) ||
            _lastContainerScale != (_container != null ? _container.lossyScale : Vector3.zero);

        if (changed)
        {
            _lastStartPos = startPos;
            _lastEndPos = endPos;
            _lastContainerPos = _container != null ? _container.position : Vector3.zero;
            _lastContainerScale = _container != null ? _container.lossyScale : Vector3.zero;

            UpdateRectTransform();
            UpdateCirclePosition();
            SetAllDirty();
        }

        // Удаление по клавише Delete или X
        if (IsSelected && (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.X)))
        {
            if (AssociatedOutput != null) AssociatedOutput.SetConnected(false);
            if (AssociatedInput != null) AssociatedInput.SetConnected(false);
            NodeController.RemoveGlobalConnection(this);
            Destroy(gameObject);
        }
    }

    private void UpdateCirclePosition()
    {
        if (_circle == null || startSlot == null || endSlot == null) return;

        Vector2 startLocal = _rt.InverseTransformPoint(startSlot.position);
        Vector2 endLocal = _rt.InverseTransformPoint(endSlot.position);
        Vector2 control1 = startLocal + Vector2.right * curveIntensity;
        Vector2 control2 = endLocal + Vector2.left * curveIntensity;

        Vector2 mid = CalculateBezierPoint(0.5f, startLocal, control1, control2, endLocal);
        _circle.anchoredPosition = mid;
    }

    private void ToggleSelection()
    {
        IsSelected = !IsSelected;
        _circleImage.color = IsSelected
            ? new Color(1f, 1f, 0.3f, 0.8f)
            : new Color(1f, 1f, 1f, 0.5f);
        SetAllDirty();
    }

    public void ForceUpdate()
    {
        UpdateRectTransform();
        UpdateCirclePosition();
    }
}
