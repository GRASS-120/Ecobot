using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GUI.Programming.Windows.Slots;
using GUI.Programming.Graph;

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
    public float curveIntensity = 50f;
    public int segmentCount = 20;

    [Header("Materials")]
    [Tooltip("Обычный материал линии/квадратика")]
    [SerializeField] private Material normalMaterial;
    [Tooltip("Материал предпросмотра при наведении на валидный слот")]
    [SerializeField] private Material hoverValidMaterial;
    [Tooltip("Материал предпросмотра при наведении на невалидный слот")]
    [SerializeField] private Material hoverInvalidMaterial;
    [Tooltip("Материал выбранной линии/квадратика")]
    [SerializeField] private Material selectedMaterial;

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

    private NodeGraphController _graph;
    private bool _removedByGraph = false;

    protected override void Awake()
    {
        base.Awake();
        _rt = GetComponent<RectTransform>();
        useLegacyMeshGeneration = false;
        raycastTarget = false;
        maskable = false;

        ApplyMaterial(normalMaterial);
    }

    private void Start()
    {
        CreateSelectionCircle();
    }

    public void SetGraph(NodeGraphController graph)
    {
        _graph = graph;
        if (_graph == null)
            Debug.LogWarning($"[Line] SetGraph: graph is NULL on {name}@{GetInstanceID()}");
        else
            Debug.Log($"[Line] SetGraph: bound graph to {name}@{GetInstanceID()}");
    }

    private void EnsureGraph()
    {
        if (_graph != null) return;
        _graph = GetComponentInParent<NodeGraphController>();
        if (_graph == null)
            _graph = FindFirstObjectByType<NodeGraphController>();
        if (_graph != null)
            Debug.Log($"[Line] EnsureGraph: late bound graph for {name}@{GetInstanceID()}");
    }

    private void CreateSelectionCircle()
    {
        var circleObj = new GameObject("SelectionCircle",
            typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));

        _circle = circleObj.GetComponent<RectTransform>();
        _circle.SetParent(transform, false);
        _circle.sizeDelta = new Vector2(16f, 16f);

        _circleImage = circleObj.GetComponent<Image>();
        _circleImage.color = new Color(1f, 1f, 1f, 0.5f); // полупрозрачный, реальный «цвет» задаёт материал/свет
        _circleImage.raycastTarget = true;

        var btn = circleObj.GetComponent<Button>();
        btn.onClick.AddListener(() => SetSelected(!IsSelected));

        _circle.SetAsLastSibling();

        // синхронизируем стартовый материал кружка с линией
        SyncHandleMaterialToCurrent();
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

    // ---------- Публичный API подсветки материалами ----------

    public void SetHoverPreview(bool isValid)
    {
        IsPreview = true;
        IsInteractable = false;
        ApplyMaterial(isValid ? hoverValidMaterial : hoverInvalidMaterial);
    }

    public void ClearHoverPreview()
    {
        IsPreview = false;
        if (IsSelected && selectedMaterial != null)
            ApplyMaterial(selectedMaterial);
        else
            ApplyMaterial(normalMaterial);
    }

    public void SetSelected(bool selected)
    {
        IsSelected = selected;

        if (_circleImage != null)
            _circleImage.color = IsSelected
                ? new Color(1f, 1f, 0.3f, 0.8f)
                : new Color(1f, 1f, 1f, 0.5f);

        if (!IsPreview)
            ApplyMaterial(IsSelected && selectedMaterial != null ? selectedMaterial : normalMaterial);

        SetAllDirty();
        Debug.Log($"[Line] SetSelected: line={name}@{GetInstanceID()} IsSelected={IsSelected}");
    }

    public void SetInteractable(bool state)
    {
        IsInteractable = state;
        IsPreview = false;
        ApplyMaterial(IsSelected && selectedMaterial != null ? selectedMaterial : normalMaterial);
        SetAllDirty();
    }

    private void ApplyMaterial(Material mat)
    {
        // Линия
        material = mat; // допускаем null → дефолт
        // Квадратик
        if (_circleImage != null)
            _circleImage.material = mat;

        SetAllDirty();
    }

    private void SyncHandleMaterialToCurrent()
    {
        // На старте (или при создании круга) синхронизируем материал квадратика с текущим материалом линии
        if (_circleImage != null)
            _circleImage.material = material;
    }

    // ---------- Рендер ----------

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
    private static Vector2 CalculateBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        float u   = 1f - t;
        float tt  = t * t;
        float uu  = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector2 p = uuu * p0;                 // (1 - t)^3 * P0
        p += 3f * uu * t * p1;                // 3(1 - t)^2 t * P1
        p += 3f * u * tt * p2;                // 3(1 - t) t^2 * P2
        p += ttt * p3;                        // t^3 * P3
        return p;
    }
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (_container == null || startSlot == null || endSlot == null)
            return;

        Vector2 startLocal = _rt.InverseTransformPoint(startSlot.position);
        Vector2 endLocal   = _rt.InverseTransformPoint(endSlot.position);
        Vector2 control1   = startLocal + Vector2.right * curveIntensity;
        Vector2 control2   = endLocal   + Vector2.left  * curveIntensity;

        // цвет вершин оставляем белым — материал рулит внешним видом
        Color col = Color.white;

        for (int i = 0; i < segmentCount; i++)
        {
            float t1 = i / (float)segmentCount;
            float t2 = (i + 1) / (float)segmentCount;

            Vector2 p1 = CalculateBezierPoint(t1, startLocal, control1, control2, endLocal);
            Vector2 p2 = CalculateBezierPoint(t2, startLocal, control1, control2, endLocal);

            Vector2 tangent = (p2 - p1);
            Vector2 n = new Vector2(-tangent.y, tangent.x);
            if (n == Vector2.zero) n = Vector2.up;
            n = n.normalized * (thickness * 0.5f);

            int idx = vh.currentVertCount;

            // v=0 — нижняя кромка, v=1 — верхняя кромка (относительно нормали)
            // u идёт по длине: t1 -> t2
            vh.AddVert(p1 - n, col, new Vector2(t1, 0f)); // 0
            vh.AddVert(p1 + n, col, new Vector2(t1, 1f)); // 1
            vh.AddVert(p2 + n, col, new Vector2(t2, 1f)); // 2
            vh.AddVert(p2 - n, col, new Vector2(t2, 0f)); // 3

            vh.AddTriangle(idx + 0, idx + 1, idx + 2);
            vh.AddTriangle(idx + 2, idx + 3, idx + 0);
        }
    }

    private static Vector2 Bezier(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        float u = 1f - t;
        return u*u*u*p0 + 3f*u*u*t*p1 + 3f*u*t*t*p2 + t*t*t*p3;
    }

    private void LateUpdate()
    {
        if (_container == null || startSlot == null) return;

        Vector3 startPos = startSlot.position;
        Vector3 endPos   = endSlot != null ? endSlot.position : startPos;

        bool changed =
            _lastStartPos != startPos ||
            _lastEndPos != endPos ||
            _lastContainerPos   != (_container != null ? _container.position   : Vector3.zero) ||
            _lastContainerScale != (_container != null ? _container.lossyScale : Vector3.zero);

        if (changed)
        {
            _lastStartPos = startPos;
            _lastEndPos   = endPos;
            _lastContainerPos   = _container != null ? _container.position   : Vector3.zero;
            _lastContainerScale = _container != null ? _container.lossyScale : Vector3.zero;

            UpdateRectTransform();
            UpdateCirclePosition();
            SetAllDirty();
        }

        if (IsSelected && (Input.GetKeyDown(KeyCode.Delete) || Input.GetKeyDown(KeyCode.X)))
        {
            Debug.Log($"[Line] LateUpdate: Delete/X on {name}@{GetInstanceID()} → RemoveSelf()");
            RemoveSelf();
        }
    }

    private void UpdateCirclePosition()
    {
        if (_circle == null || startSlot == null || endSlot == null) return;

        Vector2 startLocal = _rt.InverseTransformPoint(startSlot.position);
        Vector2 endLocal   = _rt.InverseTransformPoint(endSlot.position);
        Vector2 c1         = startLocal + Vector2.right * curveIntensity;
        Vector2 c2         = endLocal   + Vector2.left  * curveIntensity;

        Vector2 mid = Bezier(0.5f, startLocal, c1, c2, endLocal);
        _circle.anchoredPosition = mid;
    }

    public void MarkRemovedByGraph()
    {
        _removedByGraph = true;
        Debug.Log($"[Line] MarkRemovedByGraph: {name}@{GetInstanceID()}");
    }

    public void RemoveSelf()
    {
        EnsureGraph();
        if (_graph != null)
        {
            Debug.Log($"[Line] RemoveSelf: request remove {name}@{GetInstanceID()}");
            _graph.RequestRemoveConnection(this);
        }
        else
        {
            Debug.LogWarning($"[Line] RemoveSelf: graph not found, destroying {name}@{GetInstanceID()}");
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (!_removedByGraph)
        {
            EnsureGraph();
            if (_graph != null)
            {
                Debug.Log($"[Line] OnDestroy: not marked by graph. Request remove {name}@{GetInstanceID()}");
                _graph.RequestRemoveConnection(this);
            }
            else
            {
                Debug.Log($"[Line] OnDestroy: no graph found for {name}@{GetInstanceID()}");
            }
        }
        else
        {
            Debug.Log($"[Line] OnDestroy: clean exit {name}@{GetInstanceID()}");
        }
    }
}
