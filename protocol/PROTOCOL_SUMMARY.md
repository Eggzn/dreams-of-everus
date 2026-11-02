# The Protocol in one page

> **Part of The Protocol** (master: `C:\Users\Andre\Desktop\AIGames\the-protocol`). Every game repo has a copy in `protocol/`, loaded into every Claude Code session through `CLAUDE.md`. Never edit the copy in a game; change the master and run `sync.py`.

Nest Eggz'n Production: Andre owns every project and makes every design call. Claude plans, builds and checks; Andre judges fun, feel and look.

## How we work

- **Never invent design decisions.** If the design doc doesn't decide it, ask Andre.
- **Small change** (a typo, one number, one button): just do it.
- **Feature that touches logic** (rules, economy, combat, saves, accounts, money, player data): **agent team** (`protocol/AGENT_TEAM.md`). A plan in `docs/plans/<date>-<name>.md` first, tests written first and failing, builders build, QA checks, Andre judges fun.
- **Brand-new mechanic or art style:** brain + Andre prototype it first; the team comes in once the rules or style are locked. The plan still gets saved.
- **Agents missing?** The agents live in `.claude/agents/` and only load when a session starts. If they aren't available as subagents, tell Andre and suggest restarting Claude Code. Never quietly do the whole team's job alone.
- **Say what you skipped.** If a step of the Protocol was skipped, say so and why, in the same message.
- **Git:** follow `protocol/GIT_ROUTINE.md` unless this project's `CLAUDE.md` overrides it.
- **Vet every video or reel** before building on it, and log it in The Protocol's `VIDEO_LOG.md`.

## The rules (details in `compliance/`)

Read the matching standard before any change that touches player data, accounts, chat, uploads, email, purchases, analytics, session replay, review prompts, SDKs, servers, networking, builds, deploys or secrets, and update `compliance/DATA_INVENTORY.md` and `compliance/PRIVACY_POLICY.md` in the same change.

- `compliance/COMPLIANCE_STANDARD.md`: privacy, consent, age, legal docs, email, money, uploads, store rules.
- `compliance/SECURITY_STANDARD.md`: secrets, server authority, repo safety, build leftovers.
- `compliance/ANALYTICS_AND_FEEDBACK.md`: analytics and replay with consent; review prompts.

The short version:

- Collect less. No tracking without a yes. **The server decides; the client only asks.**
- No secrets in code, assets, builds or commits. Repos private. 2FA everywhere.
- No debug, cheat or admin tools in release builds.
- Legal docs live before data is collected. Every vendor named.
- Analytics and replay only after opt-in. Replay never for under-18s. Review prompts never gated, reviews never rewarded.
- No `compliance/DATA_INVENTORY.md` yet = no full compliance pass yet. Tell Andre at the start of the session and offer Protocol prompt 03. It must happen before accounts, online play or any data collection goes live.
- Anything marked **[lawyer]** gets checked before launch. None of this is legal advice.
