using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// по первому аргументу метода класс определяет, какой класс/структуру нужно расширить
public static class Vector2IntExtensions {
    public static void Deconstruct(this Vector2Int vector, out int x, out int y) {
        x = vector.x;
        y = vector.y;
    }
}
