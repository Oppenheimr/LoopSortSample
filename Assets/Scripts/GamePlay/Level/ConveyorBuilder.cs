using System.Collections.Generic;
using Dreamteck.Splines;
using Game.Levels;
using UnityEngine;

namespace GamePlay.Level
{
    public class ConveyorBuilder : MonoBehaviour
    {
        [SerializeField] private MeshFilter _conveyorBlock;
        [SerializeField] private float _speed = 3f;
        [SerializeField] private float _acceleration = 15f;
        [SerializeField] private Spline.Direction _direction = Spline.Direction.Forward;
        [SerializeField] private float _beltHalfWidth = 1.5f;
        [SerializeField] private float _beltMaxHeight = 2f;

        private const Spline.Type SplineKind = Spline.Type.Linear;
        private const int SplineSampleRate = 20;
        private const float CornerRadius = 4f;
        private const int CornerSegments = 6;

        public ConveyorBelt Build(IReadOnlyList<Vector2Int> splinePoints, Vector2 center, float cellSize, Transform parent)
        {
            if (splinePoints == null || splinePoints.Count < 2) return null;

            var go = new GameObject("Conveyor");
            go.transform.SetParent(parent, false);

            var spline = go.AddComponent<SplineComputer>();
            spline.space = SplineComputer.Space.Local;
            spline.type = SplineKind;
            spline.sampleRate = SplineSampleRate;

            var positions = BuildPath(splinePoints, center, cellSize);
            var points = new SplinePoint[positions.Count];
            for (int i = 0; i < positions.Count; i++)
                points[i] = new SplinePoint(positions[i]);

            spline.SetPoints(points, SplineComputer.Space.Local);
            spline.Close();
            spline.RebuildImmediate();

            BuildMesh(spline, go.transform);

            var belt = go.AddComponent<ConveyorBelt>();
            belt.Configure(spline, _speed, _acceleration, _direction, _beltHalfWidth, _beltMaxHeight);
            return belt;
        }

        private static List<Vector3> BuildPath(IReadOnlyList<Vector2Int> splinePoints, Vector2 center, float cellSize)
        {
            var raw = new List<Vector3>(splinePoints.Count);
            foreach (var p in splinePoints)
                raw.Add(GridMapper.ToWorld(p, center, cellSize));

            return RoundCorners(raw);
        }

        private static List<Vector3> RoundCorners(List<Vector3> pts)
        {
            int n = pts.Count;
            if (CornerRadius <= 0f || CornerSegments < 1 || n < 3) return pts;

            var result = new List<Vector3>();
            for (int i = 0; i < n; i++)
            {
                Vector3 prev = pts[(i - 1 + n) % n];
                Vector3 cur = pts[i];
                Vector3 next = pts[(i + 1) % n];

                Vector3 inDir = cur - prev;
                Vector3 outDir = next - cur;
                float inLen = inDir.magnitude, outLen = outDir.magnitude;
                if (inLen < 1e-4f || outLen < 1e-4f) { result.Add(cur); continue; }

                inDir /= inLen; outDir /= outLen;
                float r = Mathf.Min(CornerRadius, inLen * 0.5f, outLen * 0.5f);
                Vector3 start = cur - inDir * r;
                Vector3 end = cur + outDir * r;

                for (int s = 0; s <= CornerSegments; s++)
                {
                    float t = (float)s / CornerSegments;
                    float u = 1f - t;
                    result.Add(u * u * start + 2f * u * t * cur + t * t * end);
                }
            }

            return result;
        }

        private void BuildMesh(SplineComputer spline, Transform parent)
        {
            if (_conveyorBlock == null || _conveyorBlock.sharedMesh == null)
            {
                Debug.LogWarning("[ConveyorBuilder] Conveyor block has no mesh.");
                return;
            }

            var mesh = _conveyorBlock.sharedMesh;
            if (!mesh.isReadable)
                Debug.LogWarning($"[ConveyorBuilder] Mesh '{mesh.name}' is not Read/Write enabled.");

            var go = new GameObject("ConveyorMesh");
            go.transform.SetParent(parent, false);

            var splineMesh = go.AddComponent<SplineMesh>();
            splineMesh.spline = spline;

            var channel = splineMesh.AddChannel(mesh, "belt");
            channel.type = SplineMesh.Channel.Type.Extrude;
            channel.autoCount = true;

            var sourceRenderer = _conveyorBlock.GetComponentInChildren<MeshRenderer>();
            if (sourceRenderer != null && sourceRenderer.sharedMaterials.Length > 0)
                go.GetComponent<MeshRenderer>().sharedMaterials = sourceRenderer.sharedMaterials;

            splineMesh.RebuildImmediate();

            var collider = go.AddComponent<MeshCollider>();
            collider.sharedMesh = go.GetComponent<MeshFilter>().sharedMesh;
        }
    }
}
