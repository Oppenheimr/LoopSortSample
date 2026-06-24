using System.Collections;
using System.Collections.Generic;
using Core;
using Data.Levels;
using Game.Levels;
using GamePlay.Entity;
using UnityEngine;
using UnityUtils.Attribute;

namespace GamePlay.Level
{
    public class LevelController : MonoBehaviour
    {
        [SerializeField, AutoAssign] private LevelGenerator _generator;
        [SerializeField, Min(0.1f)] private float _dockRadius = .9f;

        [Header("Animation")]
        [SerializeField] private float _transferDuration = 0.15f;
        [SerializeField] private float _unstackInterval = 0.1f;
        [SerializeField] private float _unstackHeight = 0.3f;
        [SerializeField] private float _flightSpin = 540f;
        [SerializeField] private float _landTumble = 6f;
        [SerializeField] private float _arcHeight = 1.5f;
        [SerializeField] private int _landSlots = 4;
        [SerializeField, Range(0f, 1f)] private float _landFill = 0.7f;

        private readonly List<Rigidbody> _scratch = new();
        private readonly HashSet<Truck> _unstacking = new();
        private readonly Dictionary<Truck, int> _loading = new();
        private UnityEngine.Camera _cam;
        private GameObject _level;
        private int _pendingTransfers;
        private bool _won;

        private void Update()
        {
            if (_generator == null || _won || !Input.GetMouseButtonDown(0)) return;

            if (_cam == null) _cam = UnityEngine.Camera.main != null ? UnityEngine.Camera.main : FindObjectOfType<UnityEngine.Camera>();
            if (_cam == null) return;

            var ray = _cam.ScreenPointToRay(Input.mousePosition);
            var hits = Physics.RaycastAll(ray, 500f, ~0, QueryTriggerInteraction.Collide);

            Truck nearest = null;
            float nearestDist = float.MaxValue;
            foreach (var hit in hits)
            {
                var truck = hit.collider.GetComponentInParent<Truck>();
                if (truck != null && hit.distance < nearestDist)
                {
                    nearestDist = hit.distance;
                    nearest = truck;
                }
            }

            if (nearest != null && !_unstacking.Contains(nearest) && !IsLoading(nearest))
                StartCoroutine(UnstackRoutine(nearest));
        }

        private IEnumerator UnstackRoutine(Truck truck)
        {
            if (truck.Count == 0) yield break;

            _unstacking.Add(truck);

            var run = truck.TakeTopColorRun();
            Vector3 landBase = truck.dockPoint + Vector3.up * _unstackHeight;
            Vector3 spreadAxis = RailDirectionAt(truck.dockPoint);

            for (int i = 0; i < run.Count; i++)
            {
                var popped = run[i];
                Vector3 slotWorld = popped.transform.position;
                var color = popped.color;
                _generator.Cubes.Despawn(popped);

                var cube = _generator.Cubes.CreateBeltCube(color, slotWorld, _generator.Current.transform);
                cube.originTruck = truck;
                Vector3 to = landBase + spreadAxis * LandOffset(i);
                StartCoroutine(LaunchRoutine(cube, slotWorld, to));

                yield return new WaitForSeconds(_unstackInterval);
            }

            _unstacking.Remove(truck);
        }

        private IEnumerator LaunchRoutine(Cube cube, Vector3 from, Vector3 to)
        {
            _pendingTransfers++;
            Vector3 axis = Random.onUnitSphere;
            yield return MoveSpin(cube.transform, from, to, _transferDuration, axis, null);

            _generator.Belt.Place(cube.gameObject);
            cube.SetAngularVelocity(Random.onUnitSphere * _landTumble);
            _pendingTransfers--;
        }

        private void FixedUpdate()
        {
            if (_generator == null) return;

            // Must reset even while _won, or the next level stays "won" and taps stay blocked.
            if (_generator.Current != _level)
            {
                StopAllCoroutines();
                _level = _generator.Current;
                _won = false;
                _pendingTransfers = 0;
                _unstacking.Clear();
                _loading.Clear();
            }

            var belt = _generator.Belt;
            if (_won || belt == null || belt.Count == 0) return;

            _scratch.Clear();
            _scratch.AddRange(belt.Cubes);

            foreach (var rb in _scratch)
            {
                if (rb == null) continue;
                var cube = rb.GetComponent<Cube>();
                if (cube == null) continue;

                // Release the origin lock once the box has travelled away (prevents instant snap-back).
                if (cube.originTruck != null &&
                    HorizontalDistance(rb.position, cube.originTruck.dockPoint) > _dockRadius)
                    cube.originTruck = null;

                bool collectorExists = HasCollector(cube.color);

                foreach (var truck in _generator.Trucks)
                {
                    if (truck == cube.originTruck) continue;
                    if (_unstacking.Contains(truck)) continue;
                    if (!truck.Accepts(cube.color)) continue;
                    if (truck.Count == 0 && collectorExists) continue;
                    if (HorizontalDistance(rb.position, truck.dockPoint) > _dockRadius) continue;

                    Dock(rb, cube, truck, belt);
                    break;
                }
            }
        }

        private void Dock(Rigidbody rb, Cube cube, Truck truck, ConveyorBelt belt)
        {
            belt.Remove(rb);
            // Fly-in starts from where the box visually is (its mesh may be offset from the rigidbody root).
            var renderer = rb.GetComponentInChildren<Renderer>();
            Vector3 from = renderer != null ? renderer.bounds.center : rb.position;
            var color = cube.color;
            _generator.Cubes.Despawn(cube);

            var stackCube = _generator.Cubes.CreateStackCube(color);
            truck.AddCube(stackCube);
            Vector3 target = stackCube.transform.position;
            stackCube.transform.position = from;

            AddLoading(truck);
            StartCoroutine(SettleRoutine(stackCube.transform, from, target, belt, truck));
        }

        private IEnumerator SettleRoutine(Transform t, Vector3 from, Vector3 to, ConveyorBelt belt, Truck truck)
        {
            _pendingTransfers++;
            Vector3 axis = Random.onUnitSphere;
            yield return MoveSpin(t, from, to, _transferDuration, axis, truck.transform.rotation);
            if (t) { t.position = to; t.localRotation = Quaternion.identity; }
            _pendingTransfers--;
            RemoveLoading(truck);
            CheckWin(belt);
        }

        private bool HasCollector(CubeColor color)
        {
            foreach (var truck in _generator.Trucks)
                if (!_unstacking.Contains(truck) && truck.Count > 0 && truck.TopColor == color && truck.HasSpace)
                    return true;
            return false;
        }

        private float LandOffset(int index)
        {
            if (_landSlots <= 1 || _generator.Belt == null) return 0f;
            float slot = Mathf.PingPong(index, _landSlots - 1);
            float normalized = slot / (_landSlots - 1) * 2f - 1f; // -1 .. +1 sweep
            return normalized * _generator.Belt.HalfWidth * _landFill;
        }

        private Vector3 RailDirectionAt(Vector3 worldPoint)
        {
            var belt = _generator.Belt;
            if (belt == null || belt.Computer == null) return Vector3.forward;

            Vector3 forward = belt.Computer.Project(worldPoint).forward;
            forward.y = 0f;
            return forward.sqrMagnitude > 1e-6f ? forward.normalized : Vector3.forward;
        }

        private bool IsLoading(Truck truck) => _loading.TryGetValue(truck, out var n) && n > 0;
        private void AddLoading(Truck truck) => _loading[truck] = (_loading.TryGetValue(truck, out var n) ? n : 0) + 1;
        private void RemoveLoading(Truck truck)
        {
            if (_loading.TryGetValue(truck, out var n)) _loading[truck] = Mathf.Max(0, n - 1);
        }

        private IEnumerator MoveSpin(Transform t, Vector3 from, Vector3 to, float duration, Vector3 axis, Quaternion? endRotation)
        {
            if (t == null) yield break;
            if (duration <= 0f) { t.position = to; yield break; }

            Quaternion startRot = t.rotation;
            if (axis.sqrMagnitude < 1e-4f) axis = Vector3.up;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                if (t == null) yield break;
                elapsed += Time.deltaTime;
                float k = Mathf.Clamp01(elapsed / duration);

                Vector3 pos = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, k));
                pos.y += _arcHeight * 4f * k * (1f - k); // parabolic toss, peaks mid-flight
                t.position = pos;

                if (endRotation.HasValue)
                {
                    Quaternion baseRot = Quaternion.Slerp(startRot, endRotation.Value, k);
                    t.rotation = Quaternion.AngleAxis(_flightSpin * (1f - k), axis) * baseRot;
                }
                else
                {
                    t.rotation = Quaternion.AngleAxis(_flightSpin * k, axis) * startRot;
                }
                yield return null;
            }

            t.position = to;
            if (endRotation.HasValue) t.rotation = endRotation.Value;
        }

        private void CheckWin(ConveyorBelt belt)
        {
            if (_won || _pendingTransfers > 0 || belt.Count > 0) return;
            foreach (var truck in _generator.Trucks)
                if (truck.Count != 0 && !truck.IsComplete) return;

            _won = true;
            EventDispatcher.OnLevelCompleteEvent();
            Debug.Log("[LevelController] Level complete!");
        }

        private static float HorizontalDistance(Vector3 a, Vector3 b)
        {
            float dx = a.x - b.x, dz = a.z - b.z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }
    }
}
