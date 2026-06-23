using System.Collections.Generic;
using Core;
using Data.Levels;
using Game.Levels;
using GamePlay.Entity;
using UnityEngine;

namespace GamePlay.Level
{
    public class CubeFactory : MonoBehaviour
    {
        [SerializeField] private Cube _stackCubePrefab;
        [SerializeField] private Cube _beltCubePrefab;
        [SerializeField] private ColorMaterial[] _colorMaterials;
        [SerializeField] private int _stackPoolSize = 256;
        [SerializeField] private int _beltPoolSize = 128;

        private readonly HashSet<Cube> _active = new();
        private Dictionary<CubeColor, Material> _materials;
        private int _stackPoolKey = -1;
        private int _beltPoolKey = -1;
        private float _boxSize = -1f;

        public float BoxSize => _boxSize > 0f ? _boxSize : (_boxSize = MeasureBoxSize());

        private float MeasureBoxSize()
        {
            if (_stackCubePrefab == null) return 1f;

            var cube = Instantiate(_stackCubePrefab);
            cube.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            float size = 1f;
            if (cube.rendererComponent != null)
            {
                var s = cube.rendererComponent.bounds.size;
                size = Mathf.Max(s.x, s.y, s.z);
            }

            if (Application.isPlaying) Destroy(cube.gameObject);
            else DestroyImmediate(cube.gameObject);
            return size;
        }

        public Cube CreateStackCube(CubeColor color)
        {
            var cube = Spawn(_stackCubePrefab, ref _stackPoolKey, _stackPoolSize);
            cube.transform.localScale = _stackCubePrefab.transform.localScale;
            cube.name = $"StackCube_{color}";
            cube.SetColor(Material(color));
            cube.color = color;
            cube.SetTrigger(true);
            _active.Add(cube);
            return cube;
        }

        public Cube CreateBeltCube(CubeColor color, Vector3 worldPos, Transform parent)
        {
            var cube = Spawn(_beltCubePrefab, ref _beltPoolKey, _beltPoolSize);
            cube.transform.SetParent(parent, false);
            cube.transform.position = worldPos;
            cube.name = $"BeltCube_{color}";
            cube.SetColor(Material(color));
            cube.color = color;

            cube.SetKinematic(true);
            _active.Add(cube);
            return cube;
        }

        public void Despawn(Cube cube)
        {
            if (cube == null) return;
            _active.Remove(cube);
            Return(cube);
        }

        public void DespawnAll()
        {
            foreach (var cube in _active) Return(cube);
            _active.Clear();
        }

        private void Return(Cube cube)
        {
            if (cube == null) return;
            if (cube.poolKey >= 0) ObjectPooler.PutPoolObject(cube.poolKey, cube);
            else if (Application.isPlaying) Destroy(cube.gameObject);
            else DestroyImmediate(cube.gameObject);
        }

        private Cube Spawn(Cube prefab, ref int poolKey, int size)
        {
            if (Application.isPlaying)
            {
                if (poolKey < 0) poolKey = ObjectPooler.CreatePool(prefab, size);
                var pooled = ObjectPooler.GetPoolObject<Cube>(poolKey);
                if (pooled != null)
                {
                    pooled.poolKey = poolKey;
                    return pooled;
                }
            }

            var instance = Instantiate(prefab);
            instance.poolKey = -1;
            return instance;
        }

        private Material Material(CubeColor color)
        {
            if (_materials == null)
            {
                _materials = new Dictionary<CubeColor, Material>();
                if (_colorMaterials != null)
                    foreach (var entry in _colorMaterials)
                        _materials[entry.color] = entry.material;
            }
            return _materials.TryGetValue(color, out var material) ? material : null;
        }
    }
}
