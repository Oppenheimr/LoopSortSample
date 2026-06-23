using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace UnityUtils.Extensions
{
    public static class GameObjectExtensions
    {
        public static bool IsSingle<T>(this GameObject target) where T : Object => Object.FindObjectsOfType<T>().Length == 1;
        
        public static bool DontDestroyOnLoadIfSingle<T>(this GameObject target) where T : Object
        {
            if (Object.FindObjectsOfType<T>().Length > 1)
            {
                Object.Destroy(target);
                return false;
            }
            
            Object.DontDestroyOnLoad(target);
            return true;
        }
        
        public static GameObject FindInChildren(this GameObject target, string childName)
        {
            List<GameObject> children = new List<GameObject>();
            for (int i = 0; i < target.transform.childCount; i++)
            {
                GameObject child = target.transform.GetChild(i).gameObject;
                if (child.name == childName)
                    return child;
                children.Add(child);
            }
            return null;
        }
        
        public static void SetActivate(this Object gObject)
        {
            if (gObject == null) return;
            gObject.GameObject().SetActive(!gObject.GameObject().activeSelf);
        }
        
        public static void SetActivate(this Object gObject, bool active)
        {
            if (gObject == null) return;
            gObject.GameObject().SetActive(active);
        }
        
        public static async void SetActivate(this Object gobject, bool active, float delay)
        {
            //delay kadar süre sonra set active yapar
            if (gobject == null) return;
            await Task.Delay((int)(delay * 1000));
            
            gobject.SetActivate(active);
        }

        public static void SetActivate(this GameObject[] gObjects, bool active)
        {
            foreach (var gObject in gObjects)
            {
                if (gObject == null) return;
                if (gObject.activeSelf != active)
                    gObject.SetActive(active);   
            }
        }

        
        public static void SetActivate(this List<GameObject> gObjects, bool active)
        {
            foreach (var gObject in gObjects)
            {
                if (gObject == null) return;
                if (gObject.activeSelf != active)
                    gObject.SetActive(active);   
            }
        }
        
        #region Unity Editor
#if UNITY_EDITOR
        public static GameObject GetGameObjectFromPath(this string filter, string searchInFolder)
            => filter.GetObjectFromPath<GameObject>(searchInFolder);
#endif
        #endregion
    }
}