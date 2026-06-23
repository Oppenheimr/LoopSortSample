using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Xml.Linq;

namespace Data.Levels
{
    public static class XlsxReader
    {
        private static readonly XNamespace Main = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        private static readonly XNamespace Rel = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        private static readonly XNamespace Pkg = "http://schemas.openxmlformats.org/package/2006/relationships";

        public class Sheet
        {
            public string name;
            public readonly Dictionary<int, Dictionary<int, string>> rows = new();

            public string Get(int row, int col)
                => rows.TryGetValue(row, out var r) && r.TryGetValue(col, out var v) ? v : null;

            public int MaxRow { get { int m = 0; foreach (var r in rows.Keys) if (r > m) m = r; return m; } }
        }

        public static Dictionary<string, Sheet> Read(string xlsxPath)
        {
            using var stream = File.OpenRead(xlsxPath);
            return Read(stream);
        }

        public static Dictionary<string, Sheet> Read(byte[] bytes)
        {
            using var stream = new MemoryStream(bytes);
            return Read(stream);
        }

        public static Dictionary<string, Sheet> Read(Stream stream)
        {
            var result = new Dictionary<string, Sheet>();
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

            var shared = ReadSharedStrings(archive);
            var relTargets = ReadWorkbookRels(archive);

            var workbook = LoadXml(archive, "xl/workbook.xml");
            foreach (var sheetEl in workbook.Root.Element(Main + "sheets").Elements(Main + "sheet"))
            {
                var name = (string)sheetEl.Attribute("name");
                var rId = (string)sheetEl.Attribute(Rel + "id");
                if (name == null || rId == null || !relTargets.TryGetValue(rId, out var target)) continue;

                var entryPath = NormalizePath("xl/" + target);
                var sheet = ReadSheet(archive, entryPath, shared);
                sheet.name = name;
                result[name] = sheet;
            }

            return result;
        }

        private static Sheet ReadSheet(ZipArchive archive, string entryPath, List<string> shared)
        {
            var sheet = new Sheet();
            var doc = LoadXml(archive, entryPath);
            var data = doc.Root.Element(Main + "sheetData");
            if (data == null) return sheet;

            foreach (var rowEl in data.Elements(Main + "row"))
            {
                foreach (var cEl in rowEl.Elements(Main + "c"))
                {
                    var reference = (string)cEl.Attribute("r");
                    if (string.IsNullOrEmpty(reference)) continue;

                    SplitRef(reference, out int col, out int row);
                    var value = ReadCellValue(cEl, shared);
                    if (value == null) continue;

                    if (!sheet.rows.TryGetValue(row, out var cells))
                        sheet.rows[row] = cells = new Dictionary<int, string>();
                    cells[col] = value;
                }
            }

            return sheet;
        }

        private static string ReadCellValue(XElement cEl, List<string> shared)
        {
            var type = (string)cEl.Attribute("t");

            if (type == "s")
            {
                var v = cEl.Element(Main + "v");
                if (v == null) return null;
                int idx = int.Parse(v.Value);
                return idx >= 0 && idx < shared.Count ? shared[idx] : null;
            }

            if (type == "inlineStr")
            {
                var isEl = cEl.Element(Main + "is");
                return isEl == null ? null : JoinText(isEl);
            }

            var val = cEl.Element(Main + "v");
            return val?.Value;
        }

        private static List<string> ReadSharedStrings(ZipArchive archive)
        {
            var list = new List<string>();
            var entry = archive.GetEntry("xl/sharedStrings.xml");
            if (entry == null) return list;

            var doc = LoadXml(entry);
            foreach (var si in doc.Root.Elements(Main + "si"))
                list.Add(JoinText(si));
            return list;
        }

        private static Dictionary<string, string> ReadWorkbookRels(ZipArchive archive)
        {
            var map = new Dictionary<string, string>();
            var doc = LoadXml(archive, "xl/_rels/workbook.xml.rels");
            foreach (var rel in doc.Root.Elements(Pkg + "Relationship"))
            {
                var id = (string)rel.Attribute("Id");
                var target = (string)rel.Attribute("Target");
                if (id != null && target != null) map[id] = target;
            }
            return map;
        }

        private static string JoinText(XElement element)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var t in element.Descendants(Main + "t")) sb.Append(t.Value);
            return sb.ToString();
        }

        private static XDocument LoadXml(ZipArchive archive, string entryPath)
        {
            var entry = archive.GetEntry(entryPath)
                        ?? throw new FileNotFoundException($"Missing part in xlsx: {entryPath}");
            return LoadXml(entry);
        }

        private static XDocument LoadXml(ZipArchiveEntry entry)
        {
            using var stream = entry.Open();
            return XDocument.Load(stream);
        }

        private static string NormalizePath(string path) => path.Replace("\\", "/").Replace("//", "/");

        private static void SplitRef(string reference, out int col, out int row)
        {
            int i = 0;
            col = 0;
            while (i < reference.Length && char.IsLetter(reference[i]))
            {
                col = col * 26 + (char.ToUpperInvariant(reference[i]) - 'A' + 1);
                i++;
            }
            row = int.Parse(reference.Substring(i));
        }
    }
}
