---
name: code-style-preferences
description: User's C# code style preferences for this Unity project
metadata:
  type: feedback
---

When writing/editing C# in this project, follow these (the user finds verbose XML docs "AI-smelling"):

- **Always write `private` explicitly** on every private field AND every private method (don't rely on the implicit default).
- **No XML `/// <summary>` doc comments** and **no `[Tooltip(...)]`** attributes. Keep comments minimal; only a short inline `//` when genuinely needed.
- **Remove unused code** (unused methods, fields, usings) rather than leaving it "for later".
- Don't add convenience/PlayerPrefs persistence (e.g. level-progress saving) unless asked — keep mechanics minimal.

**Why:** the user wants lean, human-looking code without boilerplate doc/tooltip noise.

**How to apply:** apply to new code by default; when cleaning a file, also strip existing summaries/tooltips and make private members explicit.

Related: [[level-generation-architecture]]
