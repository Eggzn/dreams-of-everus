# Agent Team Standard

> **Part of The Protocol** (source of truth: `C:\Users\Andre\Desktop\AIGames\the-protocol\standards\`).
>
> How Claude Code works on our projects as a small team instead of one long conversation. It's based on the "orchestrator + R.B.T.Q + scoped subagents" method (@duck.tate, vetted Oct 29, 2026; see `VIDEO_LOG.md`), adapted to stay inside Claude: no extra AI vendors, so our code and data don't go to more companies.

## The idea in one line

**One brain plans. Cheap hands build. Tests decide. Andre judges fun and art.**

## The roles

| Role | Who | Model | Does | Never does |
|---|---|---|---|---|
| **Brain** | The main Claude Code session | The best model Andre has selected | Frames the work, splits it, hands pieces out, reviews every result, merges, talks to Andre | Write product code itself in team mode (it plans, reviews and fixes the plan) |
| **researcher** | `.claude/agents/researcher.md` | sonnet | Reads design docs, code and (if needed) the web; returns facts, risks and a pass/fail checklist | Edit files |
| **tester** | `.claude/agents/tester.md` | sonnet | Writes the tests **first** from the checklist, and confirms they fail ("red") | Write the feature code |
| **builder** | `.claude/agents/builder.md` | sonnet | Writes the code until the tests pass | Change or delete tests to make them pass (it reports instead) |
| **qa** | `.claude/agents/qa.md` | sonnet | Runs everything, checks screenshots, checks the Protocol standards, reports pass/fail with evidence | Edit files |

**Art team** (for projects with a locked art style):

| Role | Model | Does |
|---|---|---|
| **art-director** | opus | Turns the design doc + art rules into an **asset manifest**: one short brief per asset, grouped into batches |
| **asset-builder** | sonnet | Makes each asset in the batch with the project's pipeline (code recipes, Blender scripts, generators) |
| **art-qa** | sonnet | Runs automatic style checks, looks at the renders, makes a **contact sheet** for Andre, reports pass/fail per asset |

Each project's own art rules live in `.claude/skills/art-pipeline/SKILL.md` (template and Egg Nest example in The Protocol's `skills/art-pipeline/`). **Only the art agents read it** (scoped subagents: nobody loads instructions they don't need).

## The phases (R.B.T.Q, game version)

1. **Plan (brain + researcher).** The plan goes in `docs/plans/<date>-<short-name>.md`: goal, what changes, the pass/fail checklist, what Andre must decide.
2. **Red (tester).** Tests for the checklist are written and **fail**. No tests possible (pure visuals, feel)? The checklist says how QA will check instead (a screenshot, a number on screen).
3. **Build (builder).** Up to **3 builders at once** on separate files/areas, each with only its piece of the plan.
4. **QA (qa / art-qa).** Tests green, nothing else broken, standards respected (compliance, security, analytics), screenshots looked at.
5. **Andre's check.** Anything about **fun, feel or look** goes to Andre with a short "how to test it" list or a contact sheet. Agents never decide that something is fun.
6. **Ship.** The brain commits (and pushes per the project's routine) only after 4 and 5.

## When to use the team

- **One small change** (a typo, one number, one button): just do it, no team.
- **A feature that touches logic** (economy, combat, crafting, saves, accounts, anything with money or player data): **full team**.
- **A batch of similar assets** (furniture, apparel, critters, tiles, cards): **art team**.
- **Something new and creative** (a new art style, a new mechanic's first prototype): brain + Andre first. Only bring in the team once the style or rules are locked, or it mass-produces the wrong thing.

## Rules

- **Tests are the bar, and the bar doesn't move.** If a test seems wrong, the builder reports it and the brain decides (asking Andre if it's a design question).
- Every agent gets **only what it needs**: its piece of the plan, the files involved, the relevant skill.
- Agents follow the project's `CLAUDE.md` and the Protocol standards. QA checks them on every change that touches data, accounts, SDKs, builds or secrets.
- Keep costs sane: one batch or feature at a time; stop and ask Andre if a phase fails twice.
- Plans, manifests and contact sheets are saved in the repo, so Andre can see what happened and why.
- Never invent design decisions: anything not in the design doc is a question for Andre.

## Games need tests: where to put them

- **Web (Vite/TypeScript):** `vitest` for logic; Playwright for screenshots.
- **Unity:** Unity Test Framework, EditMode for logic (damage, crafting, costs, timers, save/load), PlayMode for scene checks, run headless with `-batchmode -runTests`. Screenshots via a small editor script.
- **iOS (Swift):** XCTest.
- **Tabletop Simulator:** keep logic in testable Python/Lua outside TTS (as Interlewd already does).

## The art team flow

1. **art-director** reads the design doc's current art direction and the art skill, picks the next batch (5–20 items) and writes briefs into `art/ASSET_MANIFEST.md` (status `briefed`).
2. **asset-builder** makes each one (status `built`).
3. **art-qa** runs the automatic checks, looks at every render, builds one contact sheet in `art/review/<batch>.png`, and marks each item `qa-pass` or `qa-fail` (with the reason).
4. **Andre** looks at the contact sheet: approve, or write a note per item. Approved → `approved`; notes go back to step 2.
5. Only `approved` assets ship.

Measure it: note Andre's minutes per approved asset for the first batches. If the team doesn't clearly beat doing it by hand, change or drop it.
