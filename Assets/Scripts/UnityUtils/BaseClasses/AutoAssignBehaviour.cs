using System.Reflection;
using UnityEngine;
using UnityUtils.Attribute;
using AutoAssignScope = UnityUtils.Attribute.AutoAssignAttribute.AutoAssignScope;

namespace UnityUtils.BaseClasses
{
    public abstract class AutoAssignBehaviour : MonoBehaviour
    {
#if UNITY_EDITOR
        // Editörde alan değiştikçe/komponent eklenince de doldurur
        protected virtual void OnValidate() => TryAutoAssign(assignOnlyIfNull: true, warn: false);
#endif

        // Runtime’da da garanti altına al
        protected virtual void Awake() => TryAutoAssign(assignOnlyIfNull: true, warn: true);

        protected void TryAutoAssign(bool assignOnlyIfNull, bool warn)
        {
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (var field in GetType().GetFields(flags))
            {
                var att = field.GetCustomAttribute<AutoAssignAttribute>();
                if (att == null) continue;

                // Yalnızca UnityEngine.Object referans alanlarını hedefleyelim
                if (!typeof(Object).IsAssignableFrom(field.FieldType)) continue;

                // Zaten bir değer varsa (ve sadece boşsa atayalım istiyorsak) geç
                if (assignOnlyIfNull && field.GetValue(this) != null) continue;

                Object found = null;

                // Scope yoksa Self varsayalım
                var scope = AutoAssignScope.Self;
                var scopeProp = att.GetType().GetProperty("Scope"); // opsiyonel genişletme yoksa null olur
                if (scopeProp != null) scope = (AutoAssignScope)scopeProp.GetValue(att);

                if (scope == AutoAssignScope.Self)
                {
                    found = GetComponent(field.FieldType);
                }
                else if (scope == AutoAssignScope.Children)
                {
                    var m = typeof(Component).GetMethod("GetComponentInChildren", new[] { typeof(System.Boolean) });
                    var generic = m.MakeGenericMethod(field.FieldType);
                    found = (Object)generic.Invoke(this, new object[] { true }); // inactive dahil
                }
                else if (scope == AutoAssignScope.Parent)
                {
                    var m = typeof(Component).GetMethod("GetComponentInParent", new[] { typeof(System.Boolean) });
                    var generic = m.MakeGenericMethod(field.FieldType);
                    found = (Object)generic.Invoke(this, new object[] { true });
                }

                if (found != null)
                {
                    field.SetValue(this, found);
#if UNITY_EDITOR
                    // Inspector’da değişikliği kirletme/serialize et
                    UnityEditor.EditorUtility.SetDirty(this);
#endif
                }
                else
                {
                    // Required ise uyar
                    var reqProp = att.GetType().GetProperty("Required");
                    bool required = reqProp != null && (bool)reqProp.GetValue(att);
                    if (warn && required)
                        Debug.LogWarning($"[{name}] {GetType().Name}.{field.Name} atanamadı: {field.FieldType.Name} bulunamadı (Scope: {scope}).", this);
                }
            }
        }
    }
}
