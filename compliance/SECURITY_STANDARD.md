# Security Standard (studio-wide)

> **Part of The Protocol** (source of truth: `C:\Users\Andre\Desktop\AIGames\the-protocol\standards\`). Each repo's `compliance/` folder holds an identical copy. Change it in The Protocol, then sync (prompt 07).
>
> Shared by every game repo, identical everywhere. Works with `COMPLIANCE_STANDARD.md` and `ANALYTICS_AND_FEEDBACK.md`.

How we protect the code, the IP, player data and the live game from people who poke at it, including people running their own AI agents to scrape builds, websites and repos for anything left behind.

**The core truth:** anything shipped to a player's machine (a game build, an app, website JavaScript) *can* be decompiled and read. Protection comes from **keeping secrets and authority off the client**, and **keeping the source private**, not from hiding things in the build.

---

## 1. Repositories and accounts
- All game repos are **private** on GitHub unless the owner explicitly decides otherwise.
- GitHub, Steamworks, Apple Developer, Google Play Console, Cloudflare, Hostinger, Unity and any database host use **2FA** (an authenticator app or passkey, not SMS) and a unique password.
- Collaborators get the lowest access that works, and access is removed when they're done.
- Turn on GitHub **secret scanning / push protection** and **Dependabot alerts** for each repo.
- Protect the main branch: no force-push, no history rewrites.
- Personal access tokens are fine-grained, scoped to one repo, and set to expire.

## 2. Secrets never touch the repo or the client
- No API keys, database passwords, service-role keys, signing keys, webhook secrets or tokens in code, configs, ScriptableObjects, Unity assets, JSON, or comments. They live in environment variables / the host's secret store (e.g. Cloudflare `wrangler secret`, `.dev.vars` for local only).
- `.gitignore` covers: `.env*`, `.dev.vars`, `*.pem`, `*.key`, `*.p12`, `*.keystore`, `google-services.json` / `GoogleService-Info.plist` (if they hold anything sensitive), build folders, local tool caches (`.wrangler/`, `Library/`, `Logs/`, `UserSettings/`), and `.claude/settings.local.json`.
- **Anything a client needs is public by definition.** If the game client must hold a key, that key must be safe to leak (a public/anon key restricted by server rules). A PostHog **project** key (`phc_…`) is fine in the client because it can only send data. A PostHog **personal API** key (`phx_…`) can read everything and must never be in the client or repo.
- If a secret is ever committed: **rotate it immediately** (assume it's stolen), then remove it from history. Deleting the file is not enough.
- Before every push, run a secret scan (e.g. `gitleaks detect`), or ask Claude to scan the diff for secrets.

## 3. The server is the authority
- The client sends *requests*; the server decides. Currency, items, crafting, damage, match results, inventory, progression, leaderboards and rewards are validated or computed on the server (or the host, in host-authoritative netcode, with checks for obviously impossible values).
- Every endpoint checks **who is asking** (authentication) and **whether they're allowed** (authorization). A player can only read and change their own data (e.g. row-level security in the database).
- Admin / Game Master tools check an admin role **on the server**, are logged, and are never reachable from a normal account or a hidden URL.
- Validate every input (type, range, length). Rate-limit login, chat, purchases and matchmaking.
- Use HTTPS only. Allow CORS only for our own domains.

## 4. Don't leave artifacts behind (what scrapers and agents look for)
Check that release builds and live sites contain none of these:
- **Debug tools**: dev consoles, cheat menus, god mode, test endpoints, "skip login" flags, admin backdoors. These go behind `#if UNITY_EDITOR || DEVELOPMENT_BUILD` (Unity) or are stripped from production builds.
- **Source maps** on the live website (`*.map` files expose the original source). Turn them off for production builds.
- **Leftover files** on the web host: `.git/`, `.env`, backups (`*.bak`, `*.zip`), `node_modules/`, test pages, old builds, design docs, logs. Deploy only the build output folder.
- **Comments and logs** revealing internals: internal URLs, server addresses, TODOs about security holes, player data in logs, verbose error messages sent to players.
- **Private material**: unreleased art, private builds, design docs and anything in private folders stay out of git and out of builds.
- AI session leftovers: scratch scripts, pasted keys, exported chat logs, `.claude/settings.local.json`.
- Analytics in development: dev/editor sessions must not send analytics or recordings to the live PostHog project (use a separate dev project, or disable capture in editor/dev builds).
- Note: `robots.txt` does **not** protect anything. It's a public list that scrapers read first. Never list secret paths there.

## 5. Harden what ships
- **Unity:** build with **IL2CPP** (not Mono) and managed code stripping for release. This makes decompiling much harder (not impossible). Don't rely on obfuscation for security; rely on server authority.
- **Web:** security headers (Content-Security-Policy, X-Content-Type-Options, Referrer-Policy, frame-ancestors), no inline secrets, and dependencies kept updated.
- **Mobile:** release signing keys stored offline and backed up, never in the repo.

## 6. Protect the IP
- Each repo has a `LICENSE` reading "Copyright (c) [YEAR] [OWNER]. All rights reserved." (proprietary, no open-source license), and the Terms forbid reverse engineering and asset ripping.
- Keep design docs, unreleased content and source files in private repos only.
- Game names and logos: check availability before launch and consider trademark registration for the main titles. **[lawyer]**
- Original work only: no names, characters, logos or assets from other companies' games. Inspired-by is fine; copies are not.
- Keep dated commits and design docs. They are your evidence of authorship.

## 7. Working with AI agents (Claude Code and others)
- Never paste secrets into a chat. Reference them by name ("the key in `.dev.vars`").
- Review what an agent commits: run `git diff --staged` before every push.
- Only install MCP servers/plugins from sources you trust; they can read your project.
- Agents must not create public repos, public gists, or deploys without the owner saying so.

## 8. Before every release / deploy
- [ ] Secret scan passes (repo + build output)
- [ ] No debug/cheat/admin tools reachable in the release build
- [ ] No source maps or stray files on the live site
- [ ] Every server endpoint checks auth and validates input
- [ ] Dependencies updated, no critical alerts
- [ ] Repo still private; collaborator list current
