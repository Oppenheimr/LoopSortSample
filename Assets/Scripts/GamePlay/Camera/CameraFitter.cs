using Game.Levels;
using UnityEngine;

namespace GamePlay.Camera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class CameraFitter : MonoBehaviour
    {
        [SerializeField] private LevelGenerator generator;
        [SerializeField] private float padding = 1.2f;
        [SerializeField] private float minDistance = 5f;

        private UnityEngine.Camera _cam;
        private GameObject _level;

        private void Awake() => _cam = GetComponent<UnityEngine.Camera>();

        private void LateUpdate()
        {
            if (generator == null) return;

            var level = generator.Current;
            if (level == null || level == _level) return;

            _level = level;
            Fit();
        }

        [ContextMenu("Fit Now")]
        public void Fit()
        {
            if (_cam == null) _cam = GetComponent<UnityEngine.Camera>();
            if (generator == null || generator.Current == null) return;
            if (!TryGetBounds(generator.Current, out var bounds)) return;

            Vector3 center = bounds.center;
            Vector3 e = bounds.extents;
            float pad = Mathf.Max(1f, padding);
            float aspect = Mathf.Max(0.01f, _cam.aspect);

            Vector3 r = transform.right, u = transform.up, f = transform.forward;
            float halfRight = Mathf.Abs(e.x * r.x) + Mathf.Abs(e.y * r.y) + Mathf.Abs(e.z * r.z);
            float halfUp = Mathf.Abs(e.x * u.x) + Mathf.Abs(e.y * u.y) + Mathf.Abs(e.z * u.z);
            float halfFwd = Mathf.Abs(e.x * f.x) + Mathf.Abs(e.y * f.y) + Mathf.Abs(e.z * f.z);

            if (_cam.orthographic)
            {
                _cam.orthographicSize = Mathf.Max(halfUp, halfRight / aspect) * pad;
                float dist = halfFwd + _cam.nearClipPlane + minDistance;
                transform.position = center - f * dist;
            }
            else
            {
                float vHalf = _cam.fieldOfView * 0.5f * Mathf.Deg2Rad;
                float hHalf = Mathf.Atan(Mathf.Tan(vHalf) * aspect);
                float dist = Mathf.Max(halfUp / Mathf.Tan(vHalf), halfRight / Mathf.Tan(hHalf)) * pad;
                transform.position = center - f * (Mathf.Max(minDistance, dist) + halfFwd);
            }
        }

        private static bool TryGetBounds(GameObject root, out Bounds bounds)
        {
            var renderers = root.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) { bounds = default; return false; }

            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);
            return true;
        }
    }
}
