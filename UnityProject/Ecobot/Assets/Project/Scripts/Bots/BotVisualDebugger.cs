using System.Collections.Generic;
using UnityEngine;

namespace Bots
{
    public class BotVisualDebugger : MonoBehaviour
    {
        public void DrawPath(List<Vector3> path) {
            if (path != null) {
                for (int i = 0; i < path.Count - 1; i ++) {
                    Vector3 point1 = new Vector3(path[i].x, 0.1f, path[i].z);
                    Vector3 point2 = new Vector3(path[i+1].x, 0.1f, path[i+1].z);
                    Debug.DrawLine(point1, point2, Color.red);
                }
            }
        }
    }
}