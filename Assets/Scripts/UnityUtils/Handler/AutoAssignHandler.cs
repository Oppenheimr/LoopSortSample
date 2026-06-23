using System.Reflection;
using UnityEngine;
using UnityUtils.Attribute;

namespace UnityUtils.Handler
{
    public class AutoAssignHandler : MonoBehaviour
    {
        // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        // private static void Create()
        // {
        //     // Create an empty GameObject
        //     //GameObject gameObject = new GameObject("Core Manager");
        //     // Add this Component
        //     //var result = gameObject.AddComponent<AutoAssignHandler>();
        // }
        
        void OnValidate()
        {
            var fields = GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            foreach (var field in fields)
            {
                var attribute = field.GetCustomAttribute<AutoAssignAttribute>();
                if (attribute != null)
                {
                    var componentType = field.FieldType;
                    var component = GetComponent(componentType);
                
                    if (component != null)
                    {
                        field.SetValue(this, component);
                    }
                    else if (attribute.Required)
                    {
                        Debug.LogWarning($"[{name}] {field.Name} bileşeni atanamadı. {componentType} bulunamadı!");
                    }
                }
            }
        }
    }
}