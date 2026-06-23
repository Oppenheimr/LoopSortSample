using UnityEngine;

namespace UnityUtils.Attribute
{
    public class SpritePreviewAttribute : PropertyAttribute
    {
        public float previewSize;
    
        public SpritePreviewAttribute(float size = 64f)
        {
            this.previewSize = size;
        }
    }
}
