# ROG ZOMBIE — Codex Project Instructions

## Project
- Project name: ROG ZOMBIE
- Engine: Unity 6000.3.16f1 (Unity 6.3 LTS)
- Render Pipeline: Universal Render Pipeline (URP) with 2D Renderer
- Primary platform: Windows
- Genre: Roguelite
- Visual style: Pixel Art
- Camera / perspective: Isometric
- Multiplayer target: Up to 4 players

## Role of Codex
Codex acts primarily as the software developer for ROG ZOMBIE.

Game design decisions are defined by the project owner and the approved Game Design Documentation (GDD).

Codex must implement the approved design accurately and must not independently redesign game mechanics.

## Game Design Rules
- Do not invent missing gameplay values, statistics, mechanics, rules, abilities, items, enemies, progression systems, or other design information.
- If required information is missing or ambiguous, explicitly report what information is missing before implementing the affected behavior.
- Do not silently make assumptions about game design.
- Do not rebalance gameplay values unless explicitly requested.
- Do not rename established gameplay terminology without explicit approval.
- Preserve the terminology used by the GDD.

## Scope Control
- Modify only files necessary for the requested task.
- Avoid unrelated refactoring.
- Do not implement additional features that were not requested.
- Do not remove existing functionality unless explicitly requested.
- Before making large architectural changes, explain why they are necessary.
- Prefer small, reviewable changes over large uncontrolled modifications.

## Unity Rules
- Do not change the Unity version.
- Do not upgrade or downgrade Unity packages unless explicitly requested.
- Do not change the Render Pipeline unless explicitly requested.
- Do not modify Project Settings unless required by the requested feature.
- Preserve Unity `.meta` files.
- Do not manually modify generated Unity folders such as:
  - Library
  - Temp
  - Logs
  - UserSettings

## Architecture
Keep gameplay systems modular and maintainable.

Where practical, separate:
- Character data
- Character runtime logic
- Combat logic
- Ability logic
- Passive ability logic
- Enemy logic
- Spawn systems
- Progression systems
- UI
- Audio
- Visual presentation

Avoid unnecessary dependencies between systems.

Prefer data-driven solutions when they make gameplay values easier to configure and balance.

## Code Quality
- Write clear and maintainable C#.
- Use descriptive names.
- Avoid unnecessary complexity.
- Avoid duplicated logic when a shared implementation is appropriate.
- Comment code when the reason behind an implementation is not obvious.
- Follow existing project conventions once they have been established.

## Validation
After implementing a feature:
1. Check for compilation errors.
2. Check for relevant Unity errors or warnings.
3. Run appropriate tests when available.
4. Verify that the implementation matches the requested specification.
5. Report what was changed.
6. Report any assumptions, limitations, warnings, or unresolved issues.

## Git Safety
- Do not delete or rewrite Git history.
- Do not force push.
- Do not perform destructive Git operations unless explicitly requested.
- Do not commit automatically unless explicitly requested.
- Keep changes suitable for review before commit.

## Priority
When instructions conflict, use this priority:

1. The user's latest explicit instruction.
2. The approved ROG ZOMBIE GDD/specification.
3. This AGENTS.md file.
4. Existing implementation and project conventions.

If a conflict cannot be resolved safely, stop and ask for clarification.