using UnityEditor;
using UnityEngine;
using UnityUtils.BaseClasses;

namespace Data
{
    [CreateAssetMenu(fileName = nameof(GameData), menuName = "Scriptables/GameData", order = 1)]
    public class GameData : SingletonScriptable<GameData>
    {
        [Header("Entities")]
        public ParticleSystem exampleData;
        
        
#if UNITY_EDITOR
        [MenuItem("Custom/Assets/Game Data")]
        public static void SelectData() => SelectAsset();
#endif
    }
}