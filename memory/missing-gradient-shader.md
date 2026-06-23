---
name: missing-gradient-shader
description: _CaseStudy materials reference a custom gradient shader that is missing from the project
metadata:
  type: project
---

The 6 case-study materials in `Assets/_CaseStudy/Materials` (Outside, Side, Surface + color variants) reference a custom shader by GUID `2f12a17f093f4400c8e5c56fa25ede39` that does NOT exist anywhere in the project (Assets/Packages/Library all checked). Unity falls back to `Hidden/InternalErrorShader` → "internal error" + magenta render.

The shader is a Built-in pipeline vertical gradient + toon shadow shader (props: `_ColorAbove`, `_ColorBelow`, `_SplitY`, `_ShadowColor`, plus full Standard props). The employer included the .mat files and gradient textures but not the .shader.

Project is **Built-in** render pipeline (no URP/HDRP package, `m_CustomRenderPipeline: 0`), so this is NOT a pipeline mismatch — just a missing file.

**Decision (2026-06-22):** User chose to request the missing .shader + .shader.meta from the employer rather than recreate/reassign. Once the shader with the matching GUID is added, all 6 materials auto-fix. Not blocking — gameplay/Excel/conveyor work can proceed meanwhile.

Related: [[case-overview]]
