using Grid.BuildingSystem.Buildings.Base;
using Grid.BuildingSystem.PowerSystem.WireSystem;
using UnityEngine;

namespace Grid.BuildingSystem.PowerSystem
{
    public class PowerNodeHighlighter : MonoBehaviour
    {
        [SerializeField] private Material previewValidMaterial;
        [SerializeField] private Material previewInvalidMaterial;
        [SerializeField] private float lineWidth = 0.05f;

        private PowerWireToolService _tool;
        private Player.PlayerManager _player;
        private LineRenderer _lr;
        private bool _active;

        public void StartPreview(PowerWireToolService tool, Player.PlayerManager player)
        {
            _tool = tool;
            _player = player;
            EnsureLine();
            _active = true;
            _lr.enabled = true;
            RefreshPreview();
        }

        public void StopPreview()
        {
            _active = false;
            if (_lr != null) _lr.enabled = false;
            _tool = null;
            _player = null;
        }

        public void RefreshPreview()
        {
            if (!_active || _tool == null || _player == null)
            {
                if (_lr != null) _lr.enabled = false;
                return;
            }

            var source = _tool.Source;
            if (source == null)
            {
                _lr.enabled = false;
                return;
            }

            var fromBB = source as BuildingBase;
            var fromAnchor = (source as IPowerAnchorProvider)?.WireAnchor;
            var p1 = (fromAnchor != null ? fromAnchor.position : fromBB.transform.position);

            Vector3 p2;
            if (_tool.CurrentHover != null)
            {
                var to = _tool.CurrentHover;
                var toBB = to as BuildingBase;
                var toAnchor = (to as IPowerAnchorProvider)?.WireAnchor;
                p2 = (toAnchor != null ? toAnchor.position : toBB.transform.position);
            }
            else
            {
                p2 = _player.transform.position + new Vector3(0, 1.2f, 0);
            }

            // Материал: валидное соединение — один, невалидное — другой
            _lr.material = _tool.HoverIsValid ? previewValidMaterial : previewInvalidMaterial;

            _lr.enabled = true;
            _lr.SetPosition(0, p1);
            _lr.SetPosition(1, p2);
        }

        private void EnsureLine()
        {
            if (_lr != null) return;

            _lr = gameObject.AddComponent<LineRenderer>();
            _lr.positionCount = 2;
            _lr.startWidth = lineWidth;
            _lr.endWidth = lineWidth;
            _lr.material = previewValidMaterial;
            _lr.useWorldSpace = true;
            _lr.enabled = false;
        }
    }
}