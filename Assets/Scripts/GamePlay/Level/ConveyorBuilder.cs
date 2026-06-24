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
        [SerializeField] private float _startDelay = 0.2f;
        [SerializeField] private Spline.Direction _direction = Spline.Direction.Forward;
        [SerializeField] private float _beltHalfWidth = 1.5f;
        [SerializeField] private float _beltMaxHeight = 2f;

        [SerializeField, Min(2)] private int _splineSampleRate = 40;
        [SerializeField] private float _cornerRadius = 4f;
        [SerializeField, Range(1, 32)] private int _cornerSegments = 12;
        [SerializeField, Min(1)] private int _meshSubdivisions = 4;

        private const Spline.Type SplineKind = Spline.Type.Linear;

        public ConveyorBelt Build(IReadOnlyList<Vector2Int> splinePoints, Vector2 center, float cellSize, Transform parent)
        {
            if (splinePoints == null || splinePoints.Count < 2) return null;

            var go = new GameObject("Conveyor");
            go.transform.SetParent(parent, false);

            var spline = go.AddComponent<SplineComputer>();
            spline.space = SplineComputer.Space.Local;
            spline.type = SplineKind;
            spline.sampleRate = _splineSampleRate;

            var positions = BuildPath(splinePoints, center, cellSize);
            var points = new SplinePoint[positions.Count];
            for (int i = 0; i < positions.Count; i++)
                points[i] = new SplinePoint(positions[i]);

            spline.SetPoints(points, SplineComputer.Space.Local);
            spline.Close();
            spline.RebuildImmediate();

            BuildMesh(spline, go.transform);

            var belt = go.AddComponent<ConveyorBelt>();
            belt.Configure(spline, _speed, _acceleration, _startDelay, _direction, _beltHalfWidth, _beltMaxHeight);
            return belt;
        }

        private List<Vector3> BuildPath(IReadOnlyList<Vector2Int> splinePoints, Vector2 center, float cellSize)
        {
            var raw = new List<Vector3>(splinePoints.Count);
            foreach (var p in splinePoints)
                raw.Add(GridMapper.ToWorld(p, center, cellSize));

            return RoundCorners(Simplify(raw));
        }

        // Drops points that sit on a straight run, leaving only real corners so the
        // fillet edges are long and the radius isn't clamped down to a tiny value.
        private List<Vector3> Simplify(List<Vector3> pts)
        {
            int n = pts.Count;
            if (n < 3) return pts;

            var result = new List<Vector3>(n);
            for (int i = 0; i < n; i++)
            {
                Vector3 a = pts[i] - pts[(i - 1 + n) % n];
                Vector3 b = pts[(i + 1) % n] - pts[i];
                if (a.sqrMagnitude < 1e-6f || b.sqrMagnitude < 1e-6f) continue;

                if (Vector3.Dot(a.normalized, b.normalized) < 0.9999f)
                    result.Add(pts[i]); // direction changes here → keep this corner
            }

            return result.Count >= 3 ? result : pts;
        }

        private List<Vector3> RoundCorners(List<Vector3> pts)
        {
            int n = pts.Count;
            if (_cornerRadius <= 0f || _cornerSegments < 1 || n < 3) return pts;

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
                float r = Mathf.Min(_cornerRadius, inLen * 0.5f, outLen * 0.5f);
                Vector3 start = cur - inDir * r;
                Vector3 end = cur + outDir * r;

                for (int s = 0; s <= _cornerSegments; s++)
                {
                    float t = (float)s / _cornerSegments;
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

            // A box only bends at the joints between copies; more (shorter) copies = smoother
            // corners. autoCount fits one copy per block length, so multiply that count up.
            if (_meshSubdivisions > 1)
            {
                channel.autoCount = false;
                channel.count = Mathf.Max(1, channel.count * _meshSubdivisions);
                splineMesh.RebuildImmediate();
            }

            var collider = go.AddComponent<MeshCollider>();
            collider.sharedMesh = go.GetComponent<MeshFilter>().sharedMesh;
        }
    }
}
