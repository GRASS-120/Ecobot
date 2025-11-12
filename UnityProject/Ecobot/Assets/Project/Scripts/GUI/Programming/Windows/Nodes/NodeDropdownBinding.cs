using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GUI.Programming.Windows.Nodes
{
    /// <summary>
    /// Универсальный биндинг для UGUI Dropdown и TMP_Dropdown.
    /// Если задан массив technicalKeys — сохраняем/восстанавливаем по тех.ключу.
    /// Если массива нет — хотя бы визуальный текст.
    /// </summary>
    public class NodeDropdownBinding : MonoBehaviour
    {
        [Header("UI (один из)")]
        [SerializeField] private Dropdown uguiDropdown;
        [SerializeField] private TMP_Dropdown tmpDropdown;

        [Header("Keys mapping (optional)")]
        [Tooltip("Массив технических ключей, по длине 1:1 с options. Если пусто — будет сохраняться только видимый текст.")]
        [SerializeField] private string[] technicalKeys;

        // ——— AUTO-RESOLVE ———
        private void Awake()
        {
            EnsureRefs();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            EnsureRefs();
        }
#endif

        private void EnsureRefs()
        {
            if (uguiDropdown == null)
                uguiDropdown = GetComponentInChildren<Dropdown>(true);
            if (tmpDropdown == null)
                tmpDropdown = GetComponentInChildren<TMP_Dropdown>(true);
        }

        private bool HasUGUI => uguiDropdown != null && uguiDropdown.options != null && uguiDropdown.options.Count > 0;
        private bool HasTMP  => tmpDropdown  != null && tmpDropdown.options  != null && tmpDropdown.options.Count  > 0;

        private int CurrentIndex
        {
            get
            {
                if (HasTMP)  return Mathf.Clamp(tmpDropdown.value,  0, tmpDropdown.options.Count  - 1);
                if (HasUGUI) return Mathf.Clamp(uguiDropdown.value, 0, uguiDropdown.options.Count - 1);
                return -1;
            }
        }

        private int OptionsCount
        {
            get
            {
                if (HasTMP)  return tmpDropdown.options.Count;
                if (HasUGUI) return uguiDropdown.options.Count;
                return 0;
            }
        }

        // ===== GET =====
        public bool TryGetTechnical(out string technical)
        {
            technical = null;
            if (!IsReady()) return false;

            if (technicalKeys == null || technicalKeys.Length != OptionsCount)
                return false;

            var idx = CurrentIndex;
            if (idx < 0) return false;

            technical = technicalKeys[idx];
            return !string.IsNullOrEmpty(technical);
        }

        public bool TryGetVisual(out string visual)
        {
            visual = null;
            if (!IsReady()) return false;

            var idx = CurrentIndex;
            if (idx < 0) return false;

            if (HasTMP)  visual = tmpDropdown.options[idx].text;
            else         visual = uguiDropdown.options[idx].text;

            return !string.IsNullOrEmpty(visual);
        }

        // ===== SET =====
        public bool TrySetByTechnical(string technical)
        {
            if (!IsReady() || string.IsNullOrEmpty(technical)) return false;
            if (technicalKeys == null || technicalKeys.Length != OptionsCount) return false;

            for (int i = 0; i < technicalKeys.Length; i++)
            {
                if (string.Equals(technicalKeys[i], technical, StringComparison.Ordinal))
                {
                    SetIndex(i);
                    return true;
                }
            }
            return false;
        }

        public bool TrySetByVisual(string visual)
        {
            if (!IsReady() || string.IsNullOrEmpty(visual)) return false;

            if (HasTMP)
            {
                for (int i = 0; i < tmpDropdown.options.Count; i++)
                    if (string.Equals(tmpDropdown.options[i].text, visual, StringComparison.Ordinal))
                    { SetIndex(i); return true; }
            }
            else if (HasUGUI)
            {
                for (int i = 0; i < uguiDropdown.options.Count; i++)
                    if (string.Equals(uguiDropdown.options[i].text, visual, StringComparison.Ordinal))
                    { SetIndex(i); return true; }
            }
            return false;
        }

        private void SetIndex(int i)
        {
            if (HasTMP)  { tmpDropdown.value = i;  tmpDropdown.RefreshShownValue(); }
            else         { uguiDropdown.value = i; uguiDropdown.RefreshShownValue(); }
        }

        private bool IsReady()
        {
            if (!HasTMP && !HasUGUI) return false;
            // если заданы тех.ключи — проверим длину
            if (technicalKeys != null && technicalKeys.Length > 0 && technicalKeys.Length != OptionsCount)
            {
                Debug.LogWarning($"[NodeDropdownBinding:{name}] technicalKeys.Length ({technicalKeys.Length}) != options.Count ({OptionsCount})");
                return false;
            }
            return true;
        }

        // отладка
        public void DebugDump()
        {
            var opts = OptionsCount;
            var keys = technicalKeys != null ? technicalKeys.Length : 0;
            var idx  = CurrentIndex;
            Debug.Log($"[NodeDropdownBinding:{name}] TMP={(HasTMP ? "yes" : "no")} UGUI={(HasUGUI ? "yes" : "no")} options={opts} technicalKeys={keys} idx={idx}");
        }
    }
}
