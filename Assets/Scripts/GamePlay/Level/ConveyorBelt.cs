using System.Collections.Generic;
using Dreamteck.Splines;
using UnityEngine;

namespace GamePlay.Level
{
    public class ConveyorBelt : MonoBehaviour
    {
        [SerializeField] private float _speed = 3f;
        [SerializeField] private float _acceleration = 15f;
        [SerializeField] private float _startDelay = 0.2f;
        [SerializeField] private Spline.Direction _direction = Spline.Direction.Forward;

        [Header("Containment (invisible walls)")]
        [SerializeField] private float _halfWidth = 1.5f;
        [SerializeField] private float _maxHeight = 2f;
        [SerializeField] private bool _drawContainment = true;

        private SplineComputer _spline;
        private SplineSample _sample = new();
        private readonly List<Rigidbody> _cubes = new();
        private readonly Dictionary<Rigidbody, float> _driveAt = new();

        public float Speed { get => _speed; set => _speed = value; }
        public int Count => _cubes.Count;
        public IReadOnlyList<Rigidbody> Cubes => _cubes;
        public SplineComputer Computer => _spline;
        public float HalfWidth => _halfWidth;

        public void Configure(SplineComputer spline, float speed, float acceleration, float startDelay,
            Spline.Direction direction, float halfWidth, float maxHeight)
        {
            _spline = spline;
            this._speed = speed;
            this._acceleration = acceleration;
            this._startDelay = startDelay;
            this._direction = direction;
            this._halfWidth = halfWidth;
            this._maxHeight = maxHeight;
        }

        public void Add(Rigidbody cube)
        {
            if (cube == null || _cubes.Contains(cube)) return;
            _cubes.Add(cube);
            _driveAt[cube] = Time.time + _startDelay; // settle briefly before the belt grabs it
        }

        public void Remove(Rigidbody cube)
        {
            _cubes.Remove(cube);
            _driveAt.Remove(cube);
        }

        public void Place(GameObject cube) => Add(Prepare(cube));

        public static Rigidbody Prepare(GameObject cube)
        {
            var rb = cube.GetComponent<Rigidbody>();
            if (rb == null) rb = cube.AddComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.angularDrag = 0.5f;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            if (cube.GetComponentInChildren<Collider>() == null)
            {
                var meshHolder = cube.GetComponentInChildren<MeshFilter>();
                (meshHolder != null ? meshHolder.gameObject : cube).AddComponent<BoxCollider>();
            }
            return rb;
        }

        private void FixedUpdate()
        {
            if (_spline == null) return;

            float dir = _direction == Spline.Direction.Backward ? -1f : 1f;

            for (int i = _cubes.Count - 1; i >= 0; i--)
            {
                var rb = _cubes[i];
                if (rb == null) { _cubes.RemoveAt(i); continue; }

                _spline.Project(rb.position, ref _sample);

                bool driving = !_driveAt.TryGetValue(rb, out float t) || Time.time >= t;
                if (driving)
                {
                    Vector3 tangent = _sample.forward.normalized * dir;
                    float along = Vector3.Dot(rb.velocity, tangent);
                    rb.AddForce(tangent * ((_speed - along) * _acceleration), ForceMode.Acceleration);
                }

                Contain(rb);
            }
        }

        private void Contain(Rigidbody rb)
        {
            Vector3 fwd = _sample.forward.normalized;
            Vector3 up = _sample.up.normalized;
            Vector3 right = Vector3.Cross(up, fwd).normalized;
            Vector3 offset = rb.position - _sample.position;

            float lateral = Vector3.Dot(offset, right);
            if (Mathf.Abs(lateral) > _halfWidth)
            {
                rb.position -= right * (lateral - Mathf.Sign(lateral) * _halfWidth);
                float vLat = Vector3.Dot(rb.velocity, right);
                rb.velocity -= right * vLat;
            }

            float vertical = Vector3.Dot(offset, up);
            if (vertical > _maxHeight)
            {
                rb.position -= up * (vertical - _maxHeight);
                float vUp = Vector3.Dot(rb.velocity, up);
                if (vUp > 0f) rb.velocity -= up * vUp;
            }
        }

        private void OnDrawGizmos()
        {
            if (!_drawContainment || _spline == null) return;

            const int steps = 80;
            Gizmos.color = Color.cyan;
            Vector3 pL = default, pR = default, pLT = default, pRT = default;

            for (int i = 0; i <= steps; i++)
            {
                var s = _spline.Evaluate((double)i / steps);
                Vector3 fwd = s.forward.normalized;
                Vector3 up = s.up.normalized;
                Vector3 right = Vector3.Cross(up, fwd).normalized;

                Vector3 L = s.position + right * _halfWidth;
                Vector3 R = s.position - right * _halfWidth;
                Vector3 LT = L + up * _maxHeight;
                Vector3 RT = R + up * _maxHeight;

                if (i > 0)
                {
                    Gizmos.DrawLine(pL, L);
                    Gizmos.DrawLine(pR, R);
                    Gizmos.DrawLine(pLT, LT);
                    Gizmos.DrawLine(pRT, RT);
                }
                if (i % 4 == 0)
                {
                    Gizmos.DrawLine(L, LT);
                    Gizmos.DrawLine(R, RT);
                }

                pL = L; pR = R; pLT = LT; pRT = RT;
            }
        }
    }
}
