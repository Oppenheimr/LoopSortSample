---
name: prefer-single-so-over-baked-assets
description: User prefers one GameData-style SO that reads source data live, over baking many SO assets
metadata:
  type: feedback
---

For data-driven systems, the user dislikes pipelines that bake many ScriptableObject assets (e.g. one LevelData asset per level + a database asset + an editor import menu). They prefer a single `GameData`-style `SingletonScriptable` that holds an inspector reference to the source file (drag the Excel/data file into a field) and reads it directly.

**Why:** Cleaner, fewer assets to manage, matches the project's existing `GameData`/`AudioData` SingletonScriptable convention, and matches the case doc wording "Levels.xlsx dosyası okunarak otomatik oluşturulması" (read the file directly).

**How to apply:** Default to one SO with a serialized source-file field + live read/parse (cache in a NonSerialized field). Avoid generating per-item assets unless the user asks. Editor convenience = a button on the SO's custom inspector, not a separate bake menu.

Related: [[level-generation-architecture]]
