using System.Collections.Generic;
using Data.Levels;
using UnityEngine;

namespace GamePlay.Entity
{
    public class Truck : MonoBehaviour
    {
        public int capacity;
        public int boxesPerCell = 16;

        public Vector3 dockPoint;

        private readonly List<Cube> _stack = new();

        private Vector3 _stackOrigin;
        private int _cellW = 4;
        private int _cellD = 2;
        private int _cellH = 2;
        private float _boxSize = 0.5f;
        private float _cellGap = 0.1f;

        public int Count => _stack.Count;
        public bool HasSpace => _stack.Count < capacity;
        public CubeColor TopColor => _stack.Count > 0 ? _stack[_stack.Count - 1].color : CubeColor.None;

        public bool Accepts(CubeColor color)
            => HasSpace && (_stack.Count == 0 || TopColor == color);

        public bool IsUniform
        {
            get
            {
                for (int i = 1; i < _stack.Count; i++)
                    if (_stack[i].color != _stack[0].color) return false;
                return true;
            }
        }

        public bool IsComplete => _stack.Count == capacity && IsUniform;

        public void Configure(int capacity, int cellW, int cellD, int cellH, float boxSize, float cellGap, Vector3 dockPoint, Vector3 stackOrigin)
        {
            this.capacity = capacity;
            _cellW = Mathf.Max(1, cellW);
            _cellD = Mathf.Max(1, cellD);
            _cellH = Mathf.Max(1, cellH);
            boxesPerCell = _cellW * _cellD * _cellH;
            _boxSize = boxSize;
            _cellGap = cellGap;
            this.dockPoint = dockPoint;
            _stackOrigin = stackOrigin;
        }

        public Vector3 SlotLocalPosition(int index)
        {
            int cell = index / boxesPerCell;
            int within = index % boxesPerCell;
            int perLayer = _cellW * _cellD;
            int y = within / perLayer;
            int rem = within % perLayer;
            int z = rem / _cellW;
            int x = rem % _cellW;

            float px = (x - (_cellW - 1) * 0.5f) * _boxSize;
            float py = (y + 0.5f) * _boxSize;
            float pz = -(cell * (_cellD * _boxSize + _cellGap) + (z + 0.5f) * _boxSize);
            return _stackOrigin + new Vector3(px, py, pz);
        }

        public void AddCube(Cube cube)
        {
            cube.transform.SetParent(transform, false);
            cube.transform.localPosition = SlotLocalPosition(_stack.Count);
            cube.transform.localRotation = Quaternion.identity;

            // Keep the cube's world size independent of the (possibly scaled) truck.
            Vector3 lossy = transform.lossyScale;
            Vector3 local = cube.transform.localScale;
            cube.transform.localScale = new Vector3(
                lossy.x != 0f ? local.x / lossy.x : local.x,
                lossy.y != 0f ? local.y / lossy.y : local.y,
                lossy.z != 0f ? local.z / lossy.z : local.z);

            _stack.Add(cube);
        }

        public Cube PopTop()
        {
            if (_stack.Count == 0) return null;
            int last = _stack.Count - 1;
            var cube = _stack[last];
            _stack.RemoveAt(last);
            return cube;
        }
    }
}
