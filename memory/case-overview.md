---
name: case-overview
description: What the Hybridcell Loop Sort case study requires and how the provided assets are structured
metadata:
  type: project
---

Hybridcell game-developer case: build a playable 3-level mini prototype referencing the **Loop Sort** game. Unity **2022.3.62f3** (matches project).

Required mechanics:
1. Cube transfer stack/unstack (truck <-> conveyor)
2. Physics-based cube movement on conveyor
3. Sort mechanic (correct color cubes into correct trucks)
4. Win screen ("You Win" + next-level button)
5. **Excel-driven level generation** from `Assets/_CaseStudy/Levels.xlsx`

Deliverable: Unity project + in-game video of 3 levels, emailed.

`_CaseStudy` folder is the EMPLOYER-provided folder. Its only `.cs` files are two third-party packages — **Dreamteck Splines** (commercial spline tool; intended for the conveyor system) and **Toony Colors Pro** (toon shader). No gameplay code is provided. Art assets: BusV3.fbx (truck), Body.prefab, Conveyor/*.fbx prefabs, Cube Beveled.fbx (cubes), Materials.

`Levels.xlsx` format (rows split into levels by col A = 1/2.0/3.0):
- **Splines sheet**: 24-col top-down grid. Numbers = ordered conveyor spline waypoints (1->2->3...). Letters (A,B,...) = truck positions. The number directly below a letter = that truck's rotation (e.g. 90, 180).
- **Carriers sheet**: per truck (col = letter), the stacked cube colors. Y=Yellow, B=Blue, G=Green, R=Red.

User has their own base project skeleton under `Assets/` (Art, Scripts, Scenes, Prefabs, LocalPackages, etc.), Built-in render pipeline.

Related: [[missing-gradient-shader]]
