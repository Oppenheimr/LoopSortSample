using System.Collections.Generic;
using UnityEngine;
using UnityUtils.Extensions;

namespace Core
{
    public static class ObjectPooler
    {
        public static GameObject PoolsParent;
        private static GameObject _poolsParentForUI;
        private static Dictionary<int, Queue<UnityEngine.Component>> _queuedPools;
        private static Dictionary<int, Transform> _poolParents;

        public static UnityEngine.Component[] GetPool(int poolId)
        {
            return !_queuedPools.TryGetValue(poolId, out var objects) ? null : objects.ToArray();
        }

        public static T[] GetPool<T>(int poolId) where T : UnityEngine.Component, new()
        {
            if (!_queuedPools.TryGetValue(poolId, out var objects))
                return null;

            var result = new T[objects.Count];
            var components = objects.ToArray();

            for (int i = 0; i < objects.Count; i++)
                result[i] = (T)components[i];

            return result;
        }

        public static T GetPoolObject<T>(int poolId, bool active = true) where T : UnityEngine.Component
        {
            var result = GetPoolObject(poolId, active);
            return (T)result;
        }   
        
        public static UnityEngine.Component GetPoolObject(int poolId, bool active = true)
        {
            if (_queuedPools == null)
                return null;
            
            if (!_queuedPools.TryGetValue(poolId, out var objects))
                return null;

            var result = _queuedPools[poolId].Dequeue();

            //TR: Eğer object kapalıysa hazır değil, tekrar kuyruğa alıyoruz.
            //EN: If the object is closed, it is not ready, we queue it again.
            for (int i = 0; i < _queuedPools[poolId].Count; i++)
            {
                if (!result.gameObject.activeSelf)
                    break;

                _queuedPools[poolId].Enqueue(result);
                result = _queuedPools[poolId].Dequeue();

                if (i != _queuedPools[poolId].Count - 1)
                    continue;

                foreach (var poolObject in _queuedPools[poolId])
                    poolObject.gameObject.SetActive(false);
                return GetPoolObject(poolId, active);
            }

            if (active)
                result.gameObject.SetActive(true);

            _queuedPools[poolId].Enqueue(result);
            return result;
        }
        
        public static void PutPoolObject(int poolId, UnityEngine.Component objectToPool)
        {
            if (!_queuedPools.TryGetValue(poolId, out var objects))
                return;
            
            _queuedPools[poolId].Enqueue(objectToPool);
            objectToPool.gameObject.SetActive(false);
            objectToPool.transform.SetParent(_poolParents[poolId]);
        }

/*         public static Component GetObject(int poolId, bool active = true)
        {
            if (!_queuedPools.TryGetValue(poolId, out var objects))
                return null;

            var result = _queuedPools[poolId].Dequeue();

            if (active)
                result.gameObject.SetActive(true);

            _queuedPools[poolId].Enqueue(result);
            return result;
        } */

        public static int CreatePool(UnityEngine.Component poolObject, int copy = 500, Transform parent = null)
        {
            _queuedPools ??= new Dictionary<int, Queue<UnityEngine.Component>>();

            Queue<UnityEngine.Component> objects = new Queue<UnityEngine.Component>();

            int key = _queuedPools.Count;

            if (!parent)
                parent = CreatePoolParent(poolObject.name, key);

            for (int i = 0; i < copy; i++)
            {

                var ins = Object.Instantiate(poolObject, parent);
                ins.gameObject.name += " " + i;
                ins.SetActivate(false);
                objects.Enqueue(ins);
            }

            _queuedPools.Add(key, objects);
            return key;
        }

        public static int CreatePoolForUI(UnityEngine.Component poolObject, int copy = 500, Transform parent = null)
        {
            _queuedPools ??= new Dictionary<int, Queue<UnityEngine.Component>>();

            Queue<UnityEngine.Component> objects = new Queue<UnityEngine.Component>();

            int key = _queuedPools.Count;

            if (!parent)
                parent = CreatePoolParentForUI(poolObject.name, key);

            for (int i = 0; i < copy; i++)
            {

                var ins = Object.Instantiate(poolObject, parent);
                ins.gameObject.name += " " + i;
                ins.SetActivate(false);
                objects.Enqueue(ins);
            }

            _queuedPools.Add(key, objects);
            return key;
        }

        //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Reset()
        {
            PoolsParent = null;
            _poolsParentForUI = null;
            _poolParents = null;
            _queuedPools = null;
        }

        private static Transform CreatePoolParent(string name, int key)
        {
            if (PoolsParent == null)
                PoolsParent = new GameObject($"Object Pools");

            var result = new GameObject($"Name : {name}, key : {key}");
            result.transform.SetParent(PoolsParent.transform);
            _poolParents ??= new Dictionary<int, Transform>();
            _poolParents.Add(key, result.transform);
            return result.transform;
        }

        private static Transform CreatePoolParentForUI(string name, int key)
        {
            if (_poolsParentForUI == null)
                _poolsParentForUI = new GameObject($"Object Pools For UI");

            _poolsParentForUI.AddComponent<Canvas>();
            var result = new GameObject($"Name : {name}, key : {key}");
            result.transform.SetParent(_poolsParentForUI.transform);
            return result.transform;
        }
    }
}