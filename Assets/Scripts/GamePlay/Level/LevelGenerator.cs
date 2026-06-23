using System.Collections.Generic;
using Data.Levels;
using Game.Levels;
using GamePlay.Entity;
using UnityEngine;
using UnityUtils.Attribute;

namespace GamePlay.Level
{
    public class LevelGenerator : MonoBehaviour
    {
        [SerializeField, AutoAssign] private ConveyorBuilder _conveyorBuilder;
        [SerializeField, AutoAssign] private CubeFactory _cubeFactory;
        [SerializeField] private Truck _truckPrefab;
        [SerializeField] private float _cellGap = 0.1f;
        [SerializeField] private Vector3 _stackOrigin = Vector3.zero;

        private const float CellSize = 1f;
        private const int CellCapacity = 4;
        private const int CellWidth = 4;
        private const int CellDepth = 2;
        private const int CellHeight = 2;

        private GameObject _current;
        private ConveyorBelt _belt;
        private readonly List<Truck> _trucks = new();

        public GameObject Current => _current;
        public ConveyorBelt Belt => _belt;
        public IReadOnlyList<Truck> Trucks => _trucks;
        public CubeFactory Cubes => _cubeFactory;

        public GameObject Generate(Data.Levels.Level level)
        {
            if (level == null)
            {
                Debug.LogError("[LevelGenerator] Level is null.");
                return null;
            }

            Clear();

            _current = new GameObject($"Level_{level.number}");
            _current.transform.SetParent(transform, false);
            _trucks.Clear();

            Vector2 center = level.GridCenter();
            _belt = _conveyorBuilder.Build(level.splinePoints, center, CellSize, _current.transform);

            foreach (var data in level.trucks)
                BuildTruck(data, center);

            return _current;
        }

        public void Clear()
        {
            if (_current == null) return;

            if (Application.isPlaying)
            {
                foreach (var cube in _current.GetComponentsInChildren<Cube>(true))
                    _cubeFactory.Despawn(cube);
                Destroy(_current);
            }
            else
            {
                DestroyImmediate(_current);
            }

            _current = null;
            _belt = null;
        }

        private void BuildTruck(TruckData data, Vector2 center)
        {
            var truck = Instantiate(_truckPrefab, _current.transform, false);
            truck.name = $"Truck_{data.id}";
            truck.transform.localPosition = GridMapper.ToWorld(data.gridPosition, center, CellSize);
            truck.transform.localRotation = Quaternion.Euler(0f, data.rotationY, 0f);

            var spline = _belt != null ? _belt.Computer : null;
            Vector3 dock = spline != null ? spline.Project(truck.transform.position).position : truck.transform.position;

            int boxesPerCell = CellWidth * CellDepth * CellHeight;
            int capacity = CellCapacity * boxesPerCell;
            truck.Configure(capacity, CellWidth, CellDepth, CellHeight, _cubeFactory.BoxSize, _cellGap, dock, _stackOrigin);

            foreach (var color in data.cubes)
                for (int b = 0; b < boxesPerCell; b++)
                    truck.AddCube(_cubeFactory.CreateStackCube(color));

            _trucks.Add(truck);
        }
    }
}
