using System.Collections;
using System.Collections.Generic;
using R3;
using UnityEngine;

namespace Grid.BuildingSystem.PowerSystem
{
    public class PowerWireViewManager : MonoBehaviour
    {
        [SerializeField] private PowerGridService powerGridService;
        [SerializeField] private Transform wiresContainer;
        [SerializeField] private Material installedMaterial;
        [SerializeField] private float lineWidth = 0.05f;
        [SerializeField] private float appearDuration = 0.2f;

        private readonly Dictionary<(IPowerNode, IPowerNode), LineRenderer> _lines = new();
        private CompositeDisposable _subs = new();

        private void OnEnable()
        {
            _subs?.Dispose();
            _subs = new CompositeDisposable();

            if (powerGridService != null)
            {
                powerGridService.Changed.Subscribe(_ => Rebuild()).AddTo(_subs);
                Rebuild();
            }
        }

        private void OnDisable()
        {
            _subs?.Dispose();
            ClearAll();
        }

        private void Rebuild()
        {
            var current = BuildCurrentEdges();

            var toRemove = new List<(IPowerNode, IPowerNode)>();
            foreach (var key in _lines.Keys)
            {
                if (!current.Contains(key))
                {
                    toRemove.Add(key);
                }
            }
            foreach (var key in toRemove)
            {
                if (_lines.TryGetValue(key, out var lr) && lr != null)
                {
                    Destroy(lr.gameObject);
                }
                _lines.Remove(key);
            }

            foreach (var key in _lines.Keys)
            {
                UpdateLinePositions(key, _lines[key]);
            }

            foreach (var key in current)
            {
                if (_lines.ContainsKey(key)) continue;
                CreateLine(key.Item1, key.Item2);
            }
        }

        private void ClearAll()
        {
            foreach (var lr in _lines.Values)
            {
                if (lr != null) Destroy(lr.gameObject);
            }
            _lines.Clear();
        }

        private void CreateLine(IPowerNode from, IPowerNode to)
        {
            var go = new GameObject($"Wire_{(from as Buildings.BuildingBase)?.name}_{(to as Buildings.BuildingBase)?.name}");
            if (wiresContainer != null) go.transform.SetParent(wiresContainer, false);

            var lr = go.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;
            lr.material = installedMaterial;
            lr.useWorldSpace = true;

            var p1 = GetAnchorPosition(from);
            var p2 = GetAnchorPosition(to);

            // стартуем "из точки" и плавно тянем к таргету
            lr.SetPosition(0, p1);
            lr.SetPosition(1, p1);

            _lines[(from, to)] = lr;

            StartCoroutine(AnimateLine(lr, p1, p2, appearDuration));
        }

        private IEnumerator AnimateLine(LineRenderer lr, Vector3 from, Vector3 to, float duration)
        {
            if (duration <= 0f)
            {
                lr.SetPosition(0, from);
                lr.SetPosition(1, to);
                yield break;
            }

            float t = 0f;
            while (t < duration && lr != null)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / duration);
                lr.SetPosition(0, from);
                lr.SetPosition(1, Vector3.Lerp(from, to, k));
                yield return null;
            }

            if (lr != null)
            {
                lr.SetPosition(0, from);
                lr.SetPosition(1, to);
            }
        }
        
        private Vector3 GetAnchorPosition(IPowerNode node)
        {
            var bb = node as Buildings.BuildingBase;
            var anchor = (node as IPowerAnchorProvider)?.WireAnchor;
            return (anchor != null ? anchor.position : bb.transform.position) + Vector3.up * 0.0f;
        }
        
        private HashSet<(IPowerNode, IPowerNode)> BuildCurrentEdges()
        {
            var set = new HashSet<(IPowerNode, IPowerNode)>();
            foreach (var from in powerGridService.Nodes)
            {
                foreach (var to in from.Outputs)
                {
                    set.Add((from, to));
                }
            }
            return set;
        }

        private void UpdateLinePositions((IPowerNode, IPowerNode) key, LineRenderer lr)
        {
            var p1 = GetAnchorPosition(key.Item1);
            var p2 = GetAnchorPosition(key.Item2);
            lr.positionCount = 2;
            lr.SetPosition(0, p1);
            lr.SetPosition(1, p2);
        }
    }
}