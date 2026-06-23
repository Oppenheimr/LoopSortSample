using System.Collections;
using System.Collections.Generic;
using Core;
using Data.Levels;
using Game.Levels;
using GamePlay.Entity;
using UnityEngine;

namespace GamePlay.Level
{
    public class LevelController : MonoBehaviour
    {
        [SerializeField] private LevelGenerator generator;
        [SerializeField, Min(0.1f)] private float dockRadius = 1.5f;

        [Header("Animation")]
        [SerializeField] private float transferDuration = 0.3f;
        [SerializeField] private float unstackInterval = 0.08f;
        [SerializeField] private float unstackHeight = 0.3f;
        [SerializeField] private float flightSpin = 540f;
        [SerializeField] private float landTumble = 6f;

        private readonly List<Rigidbody> _scratch = new();
        private readonly HashSet<Truck> _unstacking = new();
        private readonly Dictionary<Truck, int> _loading = new();
        private UnityEngine.Camera _cam;
        private GameObject _level;
        private int _pendingTransfers;
        private bool _won;

        private void Update()
        {
            if (generator == null || _won || !Input.GetMouseButtonDown(0)) return;

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
            var color = truck.TopColor;

            while (truck.Count > 0 && truck.TopColor == color)
            {
                var popped = truck.PopTop();
                Vector3 slotWorld = popped.transform.position;
                generator.Cubes.Despawn(popped);

                var cube = generator.Cubes.CreateBeltCube(color, slotWorld, generator.Current.transform);
                cube.originTruck = truck;
                StartCoroutine(LaunchRoutine(cube, slotWorld, truck.dockPoint + Vector3.up * unstackHeight));

                yield return new WaitForSeconds(unstackInterval);
            }

            _unstacking.Remove(truck);
        }

        private IEnumerator LaunchRoutine(Cube cube, Vector3 from, Vector3 to)
        {
            _pendingTransfers++;
            Vector3 axis = Random.onUnitSphere;
            yield return MoveSpin(cube.transform, from, to, transferDuration, axis, null);

            generator.Belt.Place(cube.gameObject);
            var rb = cube.GetComponent<Rigidbody>();
            if (rb != null) rb.angularVelocity = Random.onUnitSphere * landTumble;
            _pendingTransfers--;
        }

        private void FixedUpdate()
        {
            if (generator == null) return;

            // Must reset even while _won, or the next level stays "won" and taps stay blocked.
            if (generator.Current != _level)
            {
                StopAllCoroutines();
                _level = generator.Current;
                _won = false;
                _pendingTransfers = 0;
                _unstacking.Clear();
                _loading.Clear();
            }

            var belt = generator.Belt;
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
                    HorizontalDistance(rb.position, cube.originTruck.dockPoint) > dockRadius)
                    cube.originTruck = null;

                bool collectorExists = HasCollector(cube.color);

                foreach (var truck in generator.Trucks)
                {
                    if (truck == cube.originTruck) continue;
                    if (_unstacking.Contains(truck)) continue;
                    if (!truck.Accepts(cube.color)) continue;
                    if (truck.Count == 0 && collectorExists) continue;
                    if (HorizontalDistance(rb.position, truck.dockPoint) > dockRadius) continue;

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
            generator.Cubes.Despawn(cube);

            var stackCube = generator.Cubes.CreateStackCube(color);
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
            yield return MoveSpin(t, from, to, transferDuration, axis, truck.transform.rotation);
            if (t) { t.position = to; t.localRotation = Quaternion.identity; }
            _pendingTransfers--;
            RemoveLoading(truck);
            CheckWin(belt);
        }

        private bool HasCollector(CubeColor color)
        {
            foreach (var truck in generator.Trucks)
                if (!_unstacking.Contains(truck) && truck.Count > 0 && truck.TopColor == color && truck.HasSpace)
                    return true;
            return false;
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

                t.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, k));

                if (endRotation.HasValue)
                {
                    Quaternion baseRot = Quaternion.Slerp(startRot, endRotation.Value, k);
                    t.rotation = Quaternion.AngleAxis(flightSpin * (1f - k), axis) * baseRot;
                }
                else
                {
                    t.rotation = Quaternion.AngleAxis(flightSpin * k, axis) * startRot;
                }
                yield return null;
            }

            t.position = to;
            if (endRotation.HasValue) t.rotation = endRotation.Value;
        }

        private void CheckWin(ConveyorBelt belt)
        {
            if (_won || _pendingTransfers > 0 || belt.Count > 0) return;
            foreach (var truck in generator.Trucks)
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
