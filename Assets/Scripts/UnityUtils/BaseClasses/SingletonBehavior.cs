using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityUtils.BaseClasses
{
    public class SingletonBehavior : MonoBehaviour
    {
        protected void Awake()
        {
            if (FindObjectsOfType(GetType()).Length <= 1) 
                return;
            Destroy(this);
        }
    }

    /// <summary>
    /// Türetilecek tim sınıflara "Ins" tanımı ekler
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SingletonBehavior<T> : SingletonBehavior where T : Object, new()
    {
        private static T _instance;
        public static T Instance => _instance ? _instance : (_instance = FindObjectOfType<T>(false));
        public static T SlowInstance => _instance ? _instance : (_instance = FindObjectOfType<T>(true));
        
        protected static bool InstanceIsAvailable => _instance;
        public void SetInstance(T instance) => _instance = instance;
        
        public static bool TryGetInstance(out T instance)
        {
            instance = Instance;
            return InstanceIsAvailable;
        }
    }
}