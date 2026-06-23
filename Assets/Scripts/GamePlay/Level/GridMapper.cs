using UnityEngine;

namespace Game.Levels
{
    public static class GridMapper
    {
        public static Vector3 ToWorld(Vector2Int grid, Vector2 center, float cellSize)
        {
            float x = (grid.x - center.x) * cellSize;
            float z = (center.y - grid.y) * cellSize;
            return new Vector3(x, 0f, z);
        }
    }
}
