using System;
using UnityEngine;

namespace Grid
{
    public class GridBase<T> {
        public event Action<Vector2Int> OnGridObjectChanged;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public float CellSize { get; private set; }

        private Vector3 _originPosition;
        private T[,] _gridArray; 

        // createGridObject нужен чтобы при инициализации сетки можно было задать значение по умолчанию для кастомного типа
        public GridBase(int weight, int height, float cellSize, Vector3 originPosition, Func<GridBase<T>, Vector2Int, T> CreateGridObject) {
            Width = weight;
            Height = height;
            CellSize = cellSize;
            _originPosition = originPosition;

            _gridArray = new T[weight, height];

            for (int x = 0; x < _gridArray.GetLength(0); x++) {
                for (int y = 0; y < _gridArray.GetLength(1); y++) {
                    _gridArray[x, y] = CreateGridObject(this, new Vector2Int(x, y));
                }
            }
        }

        public Vector3 GetWorldPosition(Vector2Int cell) {
            // + _originPosition чтобы учитывал смещение если оно есть
            return new Vector3(cell.x, 0, cell.y) * CellSize + _originPosition;
        }

        public Vector2Int GetGridPosition(Vector3 worldPosition) {
            // - _originPosition чтобы не было смещения в расчетах
            int xGrid = Mathf.FloorToInt((worldPosition - _originPosition).x / CellSize);
            int yGrid =  Mathf.FloorToInt((worldPosition - _originPosition).z / CellSize);
            return new Vector2Int(xGrid, yGrid);
        }

        public void SetGridObject(Vector2Int cell, T obj) {
            var (x, y) = cell;
            if (x >= 0 && y >= 0 && x < Width && y < Height) {
                _gridArray[x, y] = obj;
            }
        }

        public void SetGridObject(Vector3 worldPosition, T obj) {
            Vector2Int gridPosition = GetGridPosition(worldPosition);
            SetGridObject(gridPosition, obj);
        }

        public T GetGridObject(Vector2Int cell) {
            var (x, y) = cell;
            if (x >= 0 && y >= 0 && x < Width && y < Height) {
                return _gridArray[x, y];
            } else {
                // default возвращает тип по умолчанию для указанного типа (для ссылочных данных - null, для int - 0...)
                return default;
            }
        }

        public T GetGridObject(Vector3 worldPosition) {
            Vector2Int gridPosition = GetGridPosition(worldPosition);
            return GetGridObject(gridPosition);
        }

        public void TriggerGridObjectChanged(Vector2Int cell) {
            OnGridObjectChanged?.Invoke(cell);
        }

        public override string ToString() {
            System.Text.StringBuilder gridString = new System.Text.StringBuilder();
        
            for (int y = 0; y < Height; y++) {
                for (int x = 0; x < Width; x++) {
                    gridString.Append(_gridArray[x, y]); // Добавляем значение ячейки
                    if (x < Width - 1) {
                        gridString.Append(" "); // Добавляем пробел между ячейками
                    }
                }
                gridString.AppendLine(); // Переход на новую строку после каждой строки сетки
            }
            gridString.Append($"{Width} : {Height}; cell size = {CellSize}");

            return gridString.ToString(); // Возвращаем строку сетки
        }
    }
}
