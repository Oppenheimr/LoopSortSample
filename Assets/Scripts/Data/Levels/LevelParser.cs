using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Data.Levels
{
    public static class LevelParser
    {
        private const int FirstGridColumn = 2;   // Excel column B == grid column 0
        private static readonly Regex TruckPattern = new(@"^\s*([A-Za-z]+)\s*(-?\d+)?", RegexOptions.Compiled);

        public static List<Level> Parse(Dictionary<string, XlsxReader.Sheet> sheets)
        {
            var levels = new List<Level>();
            if (sheets == null) return levels;
            if (!sheets.TryGetValue("Splines", out var splines) || !sheets.TryGetValue("Carriers", out var carriers))
            {
                Debug.LogError("[LevelParser] Missing 'Splines' or 'Carriers' sheet.");
                return levels;
            }

            var splineData = ParseSplines(splines);
            var carrierData = ParseCarriers(carriers);

            foreach (var number in splineData.Keys.Union(carrierData.Keys).OrderBy(n => n))
            {
                var level = new Level { number = number };
                if (splineData.TryGetValue(number, out var s))
                {
                    level.splinePoints = s.waypoints.Select(w => w.grid).ToList();
                    carrierData.TryGetValue(number, out var c);
                    foreach (var id in s.trucks.Keys.OrderBy(k => k))
                    {
                        var (grid, rotation) = s.trucks[id];
                        level.trucks.Add(new TruckData
                        {
                            id = id,
                            gridPosition = grid,
                            rotationY = rotation,
                            cubes = c != null && c.TryGetValue(id, out var cubes) ? new List<CubeColor>(cubes) : new List<CubeColor>()
                        });
                    }
                }
                levels.Add(level);
            }

            return levels;
        }

        private struct SplineLevel
        {
            public List<(int order, Vector2Int grid)> waypoints;
            public Dictionary<string, (Vector2Int grid, float rotation)> trucks;
        }

        private static Dictionary<int, SplineLevel> ParseSplines(XlsxReader.Sheet sheet)
        {
            var result = new Dictionary<int, SplineLevel>();

            foreach (var (number, startRow, endRow) in LevelBlocks(sheet))
            {
                var level = new SplineLevel
                {
                    waypoints = new List<(int, Vector2Int)>(),
                    trucks = new Dictionary<string, (Vector2Int, float)>()
                };

                for (int row = startRow; row < endRow; row++)
                {
                    if (!sheet.rows.TryGetValue(row, out var cells)) continue;
                    foreach (var kv in cells)
                    {
                        int col = kv.Key;
                        if (col < FirstGridColumn) continue;
                        var raw = kv.Value;
                        if (string.IsNullOrWhiteSpace(raw) || raw == "-") continue;

                        var grid = new Vector2Int(col - FirstGridColumn, row);

                        if (int.TryParse(raw.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int order))
                        {
                            level.waypoints.Add((order, grid));
                        }
                        else
                        {
                            var m = TruckPattern.Match(raw);
                            if (!m.Success) continue;
                            string id = m.Groups[1].Value.ToUpperInvariant();
                            float rotation = m.Groups[2].Success
                                ? float.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture)
                                : 0f;
                            level.trucks[id] = (grid, rotation);
                        }
                    }
                }

                level.waypoints.Sort((a, b) => a.order.CompareTo(b.order));
                result[number] = level;
            }

            return result;
        }

        private static Dictionary<int, Dictionary<string, List<CubeColor>>> ParseCarriers(XlsxReader.Sheet sheet)
        {
            var result = new Dictionary<int, Dictionary<string, List<CubeColor>>>();

            // Row 2 maps grid columns to truck letters.
            var columnToId = new Dictionary<int, string>();
            if (sheet.rows.TryGetValue(2, out var header))
                foreach (var kv in header)
                    if (kv.Key >= FirstGridColumn && !string.IsNullOrWhiteSpace(kv.Value))
                        columnToId[kv.Key] = kv.Value.Trim().ToUpperInvariant();

            foreach (var (number, startRow, endRow) in LevelBlocks(sheet))
            {
                var trucks = new Dictionary<string, List<CubeColor>>();

                foreach (var (col, id) in columnToId)
                {
                    var cubes = new List<CubeColor>();
                    for (int row = startRow; row < endRow; row++)
                    {
                        var color = ToColor(sheet.Get(row, col));
                        if (color != CubeColor.None) cubes.Add(color);
                    }
                    if (cubes.Count > 0) trucks[id] = cubes;
                }

                result[number] = trucks;
            }

            return result;
        }

        private static CubeColor ToColor(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return CubeColor.None;
            switch (raw.Trim().ToUpperInvariant())
            {
                case "Y": return CubeColor.Yellow;
                case "B": return CubeColor.Blue;
                case "G": return CubeColor.Green;
                case "R": return CubeColor.Red;
                default: return CubeColor.None;
            }
        }

        private static List<(int level, int start, int end)> LevelBlocks(XlsxReader.Sheet sheet)
        {
            var markers = new List<(int level, int row)>();
            foreach (var row in sheet.rows.Keys.OrderBy(r => r))
            {
                var marker = sheet.Get(row, 1);
                if (marker == null) continue;
                if (float.TryParse(marker.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float n))
                    markers.Add((Mathf.RoundToInt(n), row));
            }

            var blocks = new List<(int, int, int)>();
            int lastRow = sheet.MaxRow + 1;
            for (int i = 0; i < markers.Count; i++)
            {
                int end = i + 1 < markers.Count ? markers[i + 1].row : lastRow;
                blocks.Add((markers[i].level, markers[i].row, end));
            }
            return blocks;
        }
    }
}
