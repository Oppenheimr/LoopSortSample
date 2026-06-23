using Core;
using Data.Levels;
using Game.Levels;
using UnityEngine;
using UnityUtils.Attribute;

namespace GamePlay.Level
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField, AutoAssign] private LevelGenerator _generator;
        private int CurrentIndex { get; set; }

        private void OnEnable()
        {
            EventDispatcher.OnClickNextLevel.AddListener(NextLevel);
            EventDispatcher.OnGamePlay.AddListener(LoadFirst);
        }
  
        private void OnDisable()
        {
            EventDispatcher.OnClickNextLevel.RemoveListener(NextLevel);
            EventDispatcher.OnGamePlay.RemoveListener(LoadFirst);
        }

        private void LoadFirst() => Load(0);

        private void Load(int index = 0)
        {
            var data = LevelData.Instance;
            if (data == null || _generator == null)
            {
                Debug.LogError("[LevelManager] LevelData or generator not assigned.");
                return;
            }
            if (data.Count == 0)
            {
                Debug.LogError("[LevelManager] No levels parsed. Assign Levels.xlsx on the LevelData asset.");
                return;
            }

            CurrentIndex = ((index % data.Count) + data.Count) % data.Count;
            _generator.Generate(data.Get(CurrentIndex));
            EventDispatcher.OnLevelChangedEvent(_generator.Current);
        }

        private void NextLevel() => Load(CurrentIndex + 1);

        [ContextMenu("Generate (Editor Test)")]
        private void EditorGenerate() => Load(CurrentIndex);

        [ContextMenu("Next Level")]
        private void EditorNext() => NextLevel();

        [ContextMenu("Clear")]
        private void EditorClear()
        {
            if (_generator != null) _generator.Clear();
        }
    }
}
