using UnityEditor;
using UnityEngine;
using UnityUtils.BaseClasses;

namespace Data
{
    [CreateAssetMenu(fileName = nameof(AudioData), menuName = "Scriptables/Audio Data", order = 1)]
    public class AudioData : SingletonScriptable<AudioData>
    {
        [Header("Play Lists")]
        public AudioClip[] buttons;
        
        [Header("Clips")]
        public AudioClip sceneTranslation;
        
#if UNITY_EDITOR
        [MenuItem("Custom/Assets/Audio Data")]
        public static void SelectData() => SelectAsset();
#endif
    }
}