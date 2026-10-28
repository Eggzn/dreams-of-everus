# Dreams of Everus

A 2D Metroidvania action-adventure with platform-fighter combat (in the style of Smash Bros. and Rivals of Aether), capturable creatures called **Everites**, and 3D characters on a side-on 2D plane. Solo creator, learning game development as the project goes.

The full design lives in `Docs/GameDesign.md`. Read the sections that matter for the current task before you start. That document overrides anything you assume.

`Docs/CodeGuide.md` explains every code file line by line for the creator, and `Docs/Milestone1Checklist.md` is the Milestone 1 test checklist. When code changes, update the matching section of the code guide in the same commit.

## Current milestone

**Milestone 1: Movement Sandbox V0.1.** Only build what is listed here.

- A graybox room: floor, solid walls, platforms at different heights including pass-through platforms, a fall zone, and a fixed respawn point.
- A placeholder Bun (a capsule is fine) with these moves: walk (gentle tilt), dash (hard press forward or back, like a Smash input), trot, dash dance, run, run turnaround, jump, short hop (from the ground only), one air jump, fast-fall, dropping through pass-through platforms (hard press of Down), duck (crouch), and crawl for characters that can (Bun Jester can). A dash or run carries momentum into a jump. Each character has their own traction (how slippery they are on the ground) and moonwalk strength (a gentle tilt back during the trot slides them backward while facing forward). On keyboard, hold Shift to walk.
  - **Trot** is the length of a character's initial dash. A hard press the other way during the trot dashes that way (dash dance). After the trot, the character runs; turning while running plays the slower run turnaround.
- A light attack on Square, with a visible hitbox, against a standing training dummy that reacts when hit. No damage, Fragility or knockback yet.
- A side-on perspective follow camera.
- Controller (PS4/Xbox) and keyboard input through named Input System actions.
- Movement values and attack frame data that the creator can tune in the Editor, stored as per-character data (for example ScriptableObjects).

Out of scope for now: damage, Fragility, knockback, specials, shields, Everites, equipment, Dream Gauge, elements, menus, saves, and multiplayer.

## Technical rules (do not break)

1. **Rollback-ready simulation.** All gameplay runs on a fixed 60-tick-per-second step. It reads inputs once per tick and never uses `Time.deltaTime` in gameplay logic.
2. **Fixed-point math** for every gameplay number: positions, velocities and timers. Floats are allowed only in visuals such as rendering, the camera and effects.
3. **No Unity physics for gameplay.** Use a custom kinematic controller with its own simple box collision on a flat 2D plane.
4. **Game state must be copyable.** It should be possible to save a full gameplay snapshot and restore it later, which is what rollback needs. Keep that state in plain data, separate from MonoBehaviours.
5. **Keep systems separate.** Input, simulation, presentation (models, animation, camera) and data each have their own place. Visuals read from the simulation and never drive it.
6. **Hits come from hitboxes and hurtboxes,** never from the 3D model's shape.
7. **No hard-coded tuning values.** They go in data assets the creator can edit.
8. Unity 6000.6.5f1, a 3D project using URP, and the Input System package.

## Names (canon, use them exactly)

- Game: **Dreams of Everus**. Never use "Eternus", which is the retired old name.
- Lead character: **Bun Jester**. The creatures are **Everites**, and playable characters are **dream walkers**.
- Mechanics: **Everus button** (Circle), **Everus Tap** (L1), **Dream Gauge**, **Fragility** (Sturdy, Dull, Fragile, Busted, Broken), **Sentry** and **Companion** (Everite modes), **wince**.
- Elements: Psionic, Reborn, Flame and Viral. The rules for them are only a draft.

Never rename a term or invent a new one without asking.

## Controls (PS4 layout, all remappable)

| Input | Action |
|---|---|
| Square | Normal attacks (the Milestone 1 light attack) |
| Cross | Special |
| Triangle | Jump |
| Circle | Everus button |
| L1 | Everus Tap |
| L2 | Inventory |
| R1 | Grab, or neutral attack in the air |
| R2 | Shield and dodges |
| Left stick | Move |
| Right stick | Radial menu while L2 is held; otherwise attacks |
| D-pad | Everite slots |

## How to work with the creator

- **Never invent** lore, mechanics, stats or numbers and present them as decided. If something is undefined, use a clearly labeled placeholder and ask.
- **Plan first.** Before you write code for a task, explain your plan in plain words and wait for approval.
- **Work in small steps.** After each step, say what changed, why, and how to test it.
- **Explain Editor work click by click.** When something has to be done in the Unity Editor, give numbered steps (menu paths, which object to select, which field to set). Where it is practical, write an Editor script that does the setup automatically.
- **Commit to Git** after each working step, with a clear message. Never force-push or rewrite history.
- **Finish each milestone** with a short checklist the creator can follow to test it.
- **Keep code readable.** Use clear names and brief comments, and avoid clever abstractions.

## Compliance and security (read first, every session)

Before starting work, read the three shared studio files in `compliance/`: `COMPLIANCE_STANDARD.md`, `SECURITY_STANDARD.md` and `ANALYTICS_AND_FEEDBACK.md`. They are identical in every repo; never edit them in just one. Follow them for any change that touches player data, accounts, chat, uploads, email, purchases, analytics, session replay, review prompts, SDKs, servers, networking, builds, deploys or secrets.

- Before adding any SDK, data field, account feature, chat, purchase or analytics, update `compliance/DATA_INVENTORY.md` and `compliance/PRIVACY_POLICY.md` in the same change.
- Analytics and session replay load only after the player opts in; replay never for under-18s; review prompts are never gated. Details are in `ANALYTICS_AND_FEEDBACK.md`.
- Never put secrets in code, assets or commits. Never ship debug/cheat/admin tools in release builds. The server decides; the client only asks.
- If `compliance/DATA_INVENTORY.md` doesn't exist yet, this project hasn't had its full compliance pass. Tell the owner at the start of the session and offer to run it.
