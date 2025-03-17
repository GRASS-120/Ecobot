using System;
using Grid.BuildingSystem;
using UnityEngine;

namespace Grid
{
    public class GridBuildingSystemVisual : MonoBehaviour
    {
        [SerializeField] private GridBuildingSystem gridBuildingSystem;
        [SerializeField] private Color gridBuildingAreaColor;
        
        private GridBase<GridNode> _grid;
        private MeshRenderer _meshRenderer;
        private Material _material;
        private Texture2D _texture;

        // private void Awake()
        // {
        //     _grid = gridBuildingSystem.GetGrid();
        //     _meshRenderer = GetComponent<MeshRenderer>();
        //     _material = _meshRenderer.material;
        //     
        //     _texture = new Texture2D(_grid.width + 1, _grid.height + 1);
        //     _texture.wrapMode = TextureWrapMode.Clamp;
        //     _texture.filterMode = FilterMode.Bilinear;
        //     _material.SetTexture("_BaseMap", _texture);
        //     // _material.SetTexture("_BaseMap", _texture);
        //     // CreateTexture();
        // }
        //
        // private void Start()
        // {
        //     gridBuildingSystem.OnBuildingPlaced += UpdateGridVisual;
        // }
        //
        // private void UpdateGridVisual(Vector2[,] gridPositionMatrix)
        // {
        //     foreach (var gridPosition in gridPositionMatrix)
        //     {
        //         int x = _grid.width -  gridPosition.x; // Инвертируем X-координату
        //         int y = _grid.height - gridPosition.y; // Инвертируем Y-координату
        //         gridBuildingAreaColor.a = 1;
        //         _texture.SetPixel(x, y, gridBuildingAreaColor);
        //     }
        //     _texture.Apply();
        // }
    }
}