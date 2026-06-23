using UnityEditor;
using UnityEngine;

namespace UnityUtils.Attribute.Drawer
{
    [CustomPropertyDrawer(typeof(SpritePreviewAttribute))]
    public class SpritePreviewDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SpritePreviewAttribute previewAttribute = (SpritePreviewAttribute)attribute;
            return previewAttribute.previewSize; // Yükseklik sadece sprite kadar olacak
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                EditorGUI.HelpBox(position, "SpritePreview attribute only works with Sprite properties.", MessageType.Warning);
                return;
            }

            SpritePreviewAttribute previewAttribute = (SpritePreviewAttribute)attribute;
            Sprite sprite = property.objectReferenceValue as Sprite;

            float previewSize = previewAttribute.previewSize;
            float spacing = 5f; // Görselle ObjectField arasında boşluk bırakmak için

            // Sprite için kare bir alan ayarla
            Rect spriteRect = new Rect(position.x, position.y, previewSize, previewSize);

            // ObjectField için alan ayarla (sprite'ın SAĞ TARAFINA gelecek)
            Rect fieldRect = new Rect(position.x + previewSize + spacing, position.y + (previewSize / 4), position.width - previewSize - spacing, EditorGUIUtility.singleLineHeight);

            // Sprite çizimi
            if (sprite != null)
            {
                Texture2D texture = AssetPreview.GetAssetPreview(sprite);
                if (texture != null)
                {
                    GUI.DrawTexture(spriteRect, texture, ScaleMode.ScaleToFit);
                }
            }
            else
            {
                // Eğer sprite yoksa gri bir placeholder kutusu ekle
                EditorGUI.DrawRect(spriteRect, new Color(0, 0, 0, 0.1f));
                EditorGUI.LabelField(spriteRect, "No Sprite", new GUIStyle { alignment = TextAnchor.MiddleCenter });
            }

            // Standart ObjectField (sprite seçme alanı)
            EditorGUI.PropertyField(fieldRect, property, GUIContent.none);
        }
    }
}
