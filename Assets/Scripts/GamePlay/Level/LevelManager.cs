using Core;
using Data.Levels;
using UnityEngine;
using UnityUtils.Attribute;

namespace Game.Levels
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField, AutoAssign] private LevelGenerator _generator;
        [SerializeField] private bool _generateOnStart = true;

        public int CurrentIndex { get; private set; }

        private void OnEnable() => EventDispatcher.OnClickNextLevel.AddListener(NextLevel);
        private void OnDisable() => EventDispatcher.OnClickNextLevel.RemoveListener(NextLevel);

        private void Start()
        {
            if (_generateOnStart) Load(0);
        }

        public void Load(int index)
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
            EventDispatcher.OnLevelChangedEvent(CurrentIndex);
        }

        public void NextLevel() => Load(CurrentIndex + 1);

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
