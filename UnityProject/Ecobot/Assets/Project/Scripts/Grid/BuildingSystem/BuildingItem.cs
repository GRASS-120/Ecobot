using Inventory;
using Unity.VisualScripting;
using UnityEngine;

namespace Grid.BuildingSystem
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Building Item")]
    public class BuildingItem : InventoryItemData {
        public Transform prefab;
        public int width;
        public int height;

        public void GetSizesDependsOnDir(Dir dir,out int w, out int h)
        {
            switch (dir) {
                default:
                case Dir.Down:
                case Dir.Up:
                    w = this.width;
                    h = this.height;
                    break;
                case Dir.Left:
                case Dir.Right:
                    w = this.height;
                    h = this.width;
                    break;
            }
        }
        
        public enum Dir {
            Down,
            Left,
            Up,
            Right
        }

        public static Dir GetNextDir(Dir dir) {
            switch (dir) {
                default:
                case Dir.Down: return Dir.Left;
                case Dir.Left: return Dir.Up;
                case Dir.Up: return Dir.Right;
                case Dir.Right: return Dir.Down;
            }
        }

        public int GetRotationAngle(Dir dir) {
            switch (dir) {
                default:
                case Dir.Down: return 0;
                case Dir.Left: return 90;
                case Dir.Up: return 180;
                case Dir.Right: return 270;
            }
        }

        // ! в общем, есть риск, что если одна сторона чет, а другая - нет, то криво будет псотройка стоять. нужно
        // ! чекнуть на других еще, а не только на насосе. как исправить ест варик:
        // ! if w нечет и h чет (или наоборот), то округлять в большую, если нет - в меньшую (как сейчас) - мб варик еше 1 + или -  

        // так же как варик: можно сделать так, чтобы размер определялся сам, а не задавался вручную - чекать границы и
        // за счет разницы вычислять. но пока и так норм вроде

        // ? объяснение в тетради как это работает - схему придумал сам! (важны + и -)
        public Vector2Int[,] GetAllGridPositions(Vector2Int position, Dir dir) {
            Vector2Int[,] matrix;
            Vector2Int corner;

            switch (dir) {
                default:
                case Dir.Down:
                    matrix = new Vector2Int[width, height];
                    corner = new Vector2Int(position.x + width / 2, position.y + height / 2);

                    for (int i = 0; i < width; i++) {
                        for (int j = 0; j < height; j++) {
                            matrix[i,j] = new Vector2Int(corner.x - i, corner.y - j);
                        }
                    }
                    break;

                case Dir.Up:
                    matrix = new Vector2Int[width, height];
                    corner = new Vector2Int(position.x - width / 2, position.y - height / 2);

                    for (int i = 0; i < width; i++) {
                        for (int j = 0; j < height; j++) {
                            matrix[i,j] = new Vector2Int(corner.x + i, corner.y + j);
                        }
                    }
                    break;

                case Dir.Left:
                    matrix = new Vector2Int[height, width];
                    corner = new Vector2Int(position.x + height / 2, position.y - width / 2);

                    for (int i = 0; i < height; i++) {
                        for (int j = 0; j < width; j++) {
                            matrix[i,j] = new Vector2Int(corner.x - i, corner.y + j);
                        }
                    }
                    break;

                case Dir.Right:
                    matrix = new Vector2Int[height, width];
                    corner = new Vector2Int(position.x - height / 2, position.y + width / 2);

                    for (int i = 0; i < height; i++) {
                        for (int j = 0; j < width; j++) {
                            matrix[i,j] = new Vector2Int(corner.x + i, corner.y - j);
                        }
                    }
                    break;
            }

            return matrix;
        }
    }
}
