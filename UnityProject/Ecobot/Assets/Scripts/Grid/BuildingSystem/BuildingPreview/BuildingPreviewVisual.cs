using System.Collections.Generic;
using R3;
using Unity.VisualScripting;
using UnityEngine;
using Utils;

namespace Grid.BuildingSystem.BuildingPreview
{
    public class BuildingPreviewVisual : MonoBehaviour
    {
        [Header("Visual")]
        [SerializeField] private Material defaultHologram;
        [SerializeField] private Material cancelHologram;
        [SerializeField] private Material dissolve;

        private BuildingPreview _buildingPreview;
        private Transform _visual;
        private Transform _plane;
        private BuildingItem _buildingItem;
        private Vector3 _targetPosition = Vector3.zero;
        private Vector3 _planeSize = Vector3.zero;

        public Transform Plane => _plane;

        private void Awake()
        {
            _buildingPreview = GetComponent<BuildingPreview>();
        }

        private void Start()
        {
            _buildingPreview.CanBuildByCollision.Subscribe(HandleVisual).AddTo(this);
            _buildingPreview.BuildingItem.Subscribe((item => _buildingItem = item)).AddTo(this);
        }

        public void HandleVisual(bool canBuildByCollision)
        {
            if (_visual == null) return;

            if (canBuildByCollision)
            {
                Helper.Render.ChangeMaterialFull(_visual, defaultHologram);
                Helper.Render.ChangeMaterialFull(_plane, defaultHologram);
            }
            else
            {
                Helper.Render.ChangeMaterialFull(new List<Transform> {_visual, _plane}, cancelHologram);
            }
        }

        public void HandleModeChanging(GameManager.Mode currentMode)
        {
            if (currentMode == GameManager.Mode.Default)
            {
                if (_visual == null) return;
                Destroy(_visual.gameObject);
                Destroy(_plane.gameObject);
            }
            else if (currentMode == GameManager.Mode.Building)
            {
                if (_buildingItem == null) return;
                RefreshVisual(_targetPosition, _planeSize);
            }
        }

        public void RefreshVisual(Vector3 targetPosition, Vector3 planeSize)
        {
            _targetPosition = targetPosition;
            _planeSize = planeSize;
            
            if (_visual != null)
            {
                Destroy(_visual.gameObject);
                Destroy(_plane.gameObject);
                _visual = null;
                _plane = null;
            };

            if (_buildingItem == null) return;

            _visual = Instantiate(_buildingItem.prefab, _targetPosition, Quaternion.identity);
            _visual.GetComponent<Collider>().enabled = false;
            _visual.parent = transform;
            _visual.localPosition = Vector3.zero;
            _visual.localEulerAngles = Vector3.zero;
            if (_visual.TryGetComponent<Animator>(out var animator)) animator.enabled = false;

            _plane = GameObject.CreatePrimitive(PrimitiveType.Plane).transform;
            _plane.AddComponent<BuildingPreviewPlane>();
            _plane.parent = transform;
            _plane.localPosition = new Vector3(0f, -0.995f, 0f);
            _plane.localScale = _planeSize;
            _plane.localEulerAngles = Vector3.zero;
        }
    }
}