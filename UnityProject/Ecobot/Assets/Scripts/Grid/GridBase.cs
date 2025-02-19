using System;
using UnityEngine;

namespace Grid
{
    public class GridBase<T> {
        public event Action<Vector2Int> OnGridObjectChanged;
        // public class OnGridObjectChangedEventArgs : EventArgs {
        //     public Vector2Int cell;
        // }

        public int width { get; private set; }
        public int height { get; private set; }
        public float cellSize { get; private set; }

        private Vector3 _originPosition;
        private T[,] gridArray; 

        // createGridObject нужен чтобы при инициализации сетки можно было задать значение по умолчанию для кастомного типа
        public GridBase(int weight, int height, float _cellSize, Vector3 originPosition, Func<GridBase<T>, Vector2Int, T> createGridObject) {
            this.width = weight;
            this.height = height;
            this.cellSize = _cellSize;
            this._originPosition = originPosition;

            gridArray = new T[weight, height];

            for (int x = 0; x < gridArray.GetLength(0); x++) {
                for (int y = 0; y < gridArray.GetLength(1); y++) {
                    gridArray[x, y] = createGridObject(this, new Vector2Int(x, y));
                }
            }
        }

        public Vector3 GetWorldPosition(Vector2Int cell) {
            // + _originPosition чтобы учитывал смещение если оно есть
            return new Vector3(cell.x, 0, cell.y) * cellSize + _originPosition;
        }

        public Vector2Int GetGridPosition(Vector3 worldPosition) {
            // - _originPosition чтобы не было смещения в расчетах
            int xGrid = Mathf.FloorToInt((worldPosition - _originPosition).x / cellSize);
            int yGrid =  Mathf.FloorToInt((worldPosition - _originPosition).z / cellSize);
            return new Vector2Int(xGrid, yGrid);
        }

        public void SetGridObject(Vector2Int cell, T obj) {
            var (x, y) = cell;
            if (x >= 0 && y >= 0 && x < width && y < height) {
                gridArray[x, y] = obj;
            }
        }

        public void SetGridObject(Vector3 worldPosition, T obj) {
            Vector2Int gridPosition = GetGridPosition(worldPosition);
            SetGridObject(gridPosition, obj);
        }

        public T GetGridObject(Vector2Int cell) {
            var (x, y) = cell;
            if (x >= 0 && y >= 0 && x < width && y < height) {
                return gridArray[x, y];
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
        
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    gridString.Append(gridArray[x, y]); // Добавляем значение ячейки
                    if (x < width - 1) {
                        gridString.Append(" "); // Добавляем пробел между ячейками
                    }
                }
                gridString.AppendLine(); // Переход на новую строку после каждой строки сетки
            }
            gridString.Append($"{width} : {height}; cell size = {cellSize}");

            return gridString.ToString(); // Возвращаем строку сетки
        }
    }
}
