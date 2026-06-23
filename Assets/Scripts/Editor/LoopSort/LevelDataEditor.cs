using System.Linq;
using System.Text;
using Data.Levels;
using UnityEditor;
using UnityEngine;

namespace Game.EditorTools
{
    [CustomEditor(typeof(LevelData))]
    public class LevelDataEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();

            var data = (LevelData)target;

            if (GUILayout.Button("Read & Log Levels"))
            {
                data.Reload();
                var levels = data.ReadLevels();
                Debug.Log(Summarize(levels), data);
            }
        }

        private static string Summarize(System.Collections.Generic.IReadOnlyList<Level> levels)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[LevelData] Parsed {levels.Count} level(s):");
            foreach (var level in levels)
            {
                sb.AppendLine($"  Level {level.number}: {level.splinePoints.Count} spline point(s), {level.trucks.Count} truck(s)");
                foreach (var t in level.trucks)
                {
                    var cubes = string.Join(",", t.cubes.Select(c => c.ToString()[0]));
                    sb.AppendLine($"    Truck {t.id} @ {t.gridPosition} rot {t.rotationY}  cubes[{t.cubes.Count}]: {cubes}");
                }
            }
            return sb.ToString();
        }
    }
}
