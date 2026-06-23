using System.Collections.Generic;
using UnityEngine;

namespace Data.Levels
{
    [System.Serializable]
    public class Level
    {
        public int number;

        public List<Vector2Int> splinePoints = new();

        public List<TruckData> trucks = new();

        public Vector2 GridCenter()
        {
            bool any = false;
            Vector2 min = Vector2.zero, max = Vector2.zero;

            void Acc(Vector2Int p)
            {
                if (!any) { min = max = p; any = true; return; }
                min = Vector2.Min(min, p);
                max = Vector2.Max(max, p);
            }

            foreach (var p in splinePoints) Acc(p);
            foreach (var t in trucks) Acc(t.gridPosition);

            return any ? (min + max) * 0.5f : Vector2.zero;
        }
    }

    [System.Serializable]
    public class TruckData
    {
        public string id;
        public Vector2Int gridPosition;
        public float rotationY;

        public List<CubeColor> cubes = new();
    }
}
