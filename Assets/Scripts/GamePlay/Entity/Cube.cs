using Data.Levels;
using UnityEngine;
using UnityUtils.Attribute;

namespace GamePlay.Entity
{
    public class Cube : MonoBehaviour
    {
        [SerializeField, AutoAssign] private Rigidbody _rigidbody;
        [SerializeField, AutoAssign] private Collider _collider;
        [AutoAssign] public Renderer rendererComponent;
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
            if (!rendererComponent)
                return;
            rendererComponent.sharedMaterial = material;
        }

        public void SetAngularVelocity(Vector3 velocityVector)
        {
            if (!_rigidbody)
                return;
            _rigidbody.angularVelocity = velocityVector;
        }

        public void SetKinematic(bool isKinematic)
        {
            if (!_rigidbody)
                return;
            _rigidbody.isKinematic = isKinematic;
        }
    }
}