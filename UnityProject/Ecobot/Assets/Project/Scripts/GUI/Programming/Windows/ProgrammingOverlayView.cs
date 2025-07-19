using System;
using GUI.UIFramework;
using R3;
using UnityEngine;
using UnityEngine.UI;
using Observable = UnityEngine.InputSystem.Utilities.Observable;

namespace GUI.Programming.Windows
{
    public class ProgrammingOverlayView : OverlayView
    {
        [Header("UI Elements")]
        [SerializeField] private Button btnClose;
        
        public Button BtnClose => btnClose;
    }
}