using UnityEditor;
using UnityEngine;
using AutoAssignScope = UnityUtils.Attribute.AutoAssignAttribute.AutoAssignScope;

namespace UnityUtils.Attribute.Drawer
{
    [CustomPropertyDrawer(typeof(AutoAssignAttribute))]
    public class AutoAssignDrawer : PropertyDrawer
    {
        private bool editMode = false; // Edit aktif mi?

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var att = (AutoAssignAttribute)attribute;
            var fieldType = fieldInfo.FieldType;

            const float buttonWidth = 45f;
            const float space = 4f;

            bool hasValue = property.objectReferenceValue != null;

            // Alan dikdörtgeni
            var fieldRect = position;

            if (hasValue)
            {
                // Eğer atanmışsa, yanına Edit butonu koy
                var editRect = new Rect(position.xMax - buttonWidth, position.y, buttonWidth, position.height);
                fieldRect.width -= (buttonWidth + space);

                using (new EditorGUI.DisabledScope(!editMode))
                {
                    EditorGUI.PropertyField(fieldRect, property, label, true);
                }

                if (GUI.Button(editRect, editMode ? "Lock" : "Edit"))
                {
                    editMode = !editMode;
                }

                return;
            }

            // null ise, Auto ve Add butonlarına yer bırak
            var autoRect = new Rect(position.xMax - (buttonWidth * 2 + space), position.y, buttonWidth, position.height);
            var addRect = new Rect(position.xMax - buttonWidth, position.y, buttonWidth, position.height);

            fieldRect.width -= (buttonWidth * 2 + space * 2);
            EditorGUI.PropertyField(fieldRect, property, label, true);

            // "Auto" butonu
            if (GUI.Button(autoRect, "Auto"))
            {
                var target = property.serializedObject.targetObject as Component;
                if (target != null)
                {
                    Object found = null;
                    switch (att.Scope)
                    {
                        case AutoAssignScope.Self:
                            found = target.GetComponent(fieldType);
                            break;
                        case AutoAssignScope.Children:
                            found = target.GetComponentInChildren(fieldType, true);
                            break;
                        case AutoAssignScope.Parent:
                            found = target.GetComponentInParent(fieldType, true);
                            break;
                    }

                    if (found != null)
                    {
                        property.objectReferenceValue = found;
                        property.serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(target);
                    }
                    else if (att.Required)
                    {
                        Debug.LogWarning($"[{target.name}] {property.displayName} için {fieldType.Name} bulunamadı.");
                    }
                }
            }

            // "Add" butonu
            if (GUI.Button(addRect, "Add"))
            {
                var target = property.serializedObject.targetObject as Component;
                if (target != null)
                {
                    var added = target.gameObject.AddComponent(fieldType);
                    property.objectReferenceValue = added;
                    property.serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(target);
                }
            }
        }
    }
}
