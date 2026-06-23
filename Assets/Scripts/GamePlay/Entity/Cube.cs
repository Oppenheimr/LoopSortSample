using Data.Levels;
using UnityEngine;
using UnityUtils.Attribute;

namespace GamePlay.Entity
{
    public class Cube : MonoBehaviour
    {
        [SerializeField, AutoAssign] private Rigidbody _rigidbody;
        [SerializeField, AutoAssign] private Collider _collider;
        [SerializeField, AutoAssign] private Renderer _renderer;
        public CubeColor color;

        // Belt cubes only: cleared once the cube has travelled away (prevents instant snap-back).
        [System.NonSerialized] public Truck originTruck;

        [System.NonSerialized] public int poolKey = -1;

        public void SetTrigger(bool active)
        {
            if (!_collider)
                return;
            _collider.enabled = active;
        }

        public void SetColor(Material material)
        {
            if (!_renderer)
                return;
            _renderer.sharedMaterial = material;
        }

        public void SetAngularVelocity(Vector3 velocityVector)
        {
            if (!_rigidbody)
                return;
            _rigidbody.angularVelocity = velocityVector;
        }
    }
}