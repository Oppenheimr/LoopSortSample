using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityUtils.BaseClasses;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Data.Levels
{
    [CreateAssetMenu(fileName = nameof(LevelData), menuName = "Scriptables/LevelData", order = 2)]
    public class LevelData : SingletonScriptable<LevelData>
    {
        [Header("Source")]
        [SerializeField] private Object excelFile;

        [System.NonSerialized] private List<Level> _levels;

        public IReadOnlyList<Level> Levels => _levels ??= ReadLevels();

        public int Count => Levels.Count;

        public Level Get(int index)
        {
            var levels = Levels;
            return levels.Count == 0 ? null : levels[Mathf.Clamp(index, 0, levels.Count - 1)];
        }

        public void Reload() => _levels = null;

        public List<Level> ReadLevels()
        {
            var path = ResolveExcelPath();
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                Debug.LogError("[LevelData] Excel file is not assigned or could not be found. " +
                               "Assign Levels.xlsx on the LevelData asset.");
                return new List<Level>();
            }

            return LevelParser.Parse(XlsxReader.Read(path));
        }

        private string ResolveExcelPath()
        {
#if UNITY_EDITOR
            if (excelFile != null) return AssetDatabase.GetAssetPath(excelFile);
#endif
            if (excelFile != null)
            {
                var streaming = Path.Combine(Application.streamingAssetsPath, excelFile.name + ".xlsx");
                if (File.Exists(streaming)) return streaming;
            }
            return null;
        }

#if UNITY_EDITOR
        [MenuItem("Custom/Assets/Level Data")]
        public static void SelectData() => SelectAsset();
#endif
    }
}
