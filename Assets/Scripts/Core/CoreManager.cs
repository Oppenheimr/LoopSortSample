using Game.Core;
using UnityEngine;
using UnityUtils.BaseClasses;

namespace Core
{
    public class CoreManager : SingletonBehavior<CoreManager>
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Create()
        {
            if (InstanceIsAvailable)
                return;
            
            // Create an empty GameObject
            GameObject gameObject = new GameObject("Core Manager");
            // Add this Component
            var result = gameObject.AddComponent<CoreManager>();
         
            Instance.SetInstance(result);
            Memory.Initialize();
        }

        private void OnDisable() => OnApplicationQuit();
        private void OnDestroy() => OnApplicationQuit();
        private void OnApplicationQuit() => Reset();
        
        public void Reset()
        {
            Memory.Reset();
        }
    }
}