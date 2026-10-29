# Compliance Standard (studio-wide, v2)

> **Part of The Protocol** (source of truth: `C:\Users\Andre\Desktop\AIGames\the-protocol\standards\`). Each repo's `compliance/` folder holds an identical copy. Change it in The Protocol, then sync (prompt 07).
>
> The shared rulebook for every game Andre / Nest Eggz'n Production LLC makes: Egg Nest, Block Brawl, Dreams of Everus, Vessel, ShardSlapper, Interlewd, and anything new. It applies to every platform (Steam, iOS, Google Play, websites) and every Claude session, and is copied **identically** into each repo's `compliance/` folder. To change it, change it everywhere.
>
> Not legal advice. Anything marked **[lawyer]** is checked by a lawyer before launch. v2 (Oct 28, 2026) merges Egg Nest's standard with the studio standard, and adds the rules from two vetted videos: legal docs/consent/database (@duck.tate) and the vibe-coder liability list (@adilet.fndr). It also adds the analytics, session replay and review rules from `ANALYTICS_AND_FEEDBACK.md`.

The three rules behind everything:
1. **Have real legal documents** (Terms/EULA + Privacy Policy) that match what the game actually does.
2. **No tracking without a yes.**
3. **Protect the data**: the server decides, and each player can only reach their own data.

## The files every repo keeps in `compliance/`

Shared (identical everywhere): `COMPLIANCE_STANDARD.md` (this file), `SECURITY_STANDARD.md`, `ANALYTICS_AND_FEEDBACK.md`.
Per project (written from the project's own code): `DATA_INVENTORY.md`, `SECURITY_AUDIT.md`, `CONSENT_PLAN.md`, `PRIVACY_POLICY.md`, `TERMS_OF_SERVICE.md`, `PLATFORM_CHECKLIST.md`, `OPERATIONS.md`.
The privacy policy is written **from** the data inventory, and the two never disagree. A project that doesn't have the per-project files yet hasn't had its full compliance pass.

---

## 1. Collect less
- Only collect data the game needs to work. Every field in `DATA_INVENTORY.md` has a reason, a storage place, a list of who receives it, a retention time, and whether it needs consent. **If it isn't in the inventory, it doesn't ship.**
- Personal data includes: Steam ID, email, username, IP address, device/advertising ID, location, voice/text chat, player-made content, purchase history, analytics events tied to a player, session recordings, crash logs containing any of these, birth year/age.
- Keep data only as long as needed. Delete inactive accounts after a stated period, with a warning email first.
- **Before adding any SDK, service, data field, account feature, chat, purchase, ad or analytics:** update `DATA_INVENTORY.md` and `PRIVACY_POLICY.md` in the same change, and re-run the release checklist (§12).

## 2. No tracking without a yes
- Analytics, session replay, ads, attribution, third-party crash reporting and social embeds **don't even load** until the player opts in. "No" is as easy as "yes" (equal buttons, nothing pre-ticked, closing = no). The choice can be changed in Settings at any time. On the web, Global Privacy Control counts as "no" to selling, sharing and ads.
- One consent module per game (e.g. `consent.ts` / `Consent.cs` / `ConsentStore.swift`) loads every optional SDK. No other file may start one.
- Record the choice, its date and the policy version it was given against.
- iOS: App Tracking Transparency only if something actually tracks across other companies' apps/sites. First-party analytics (PostHog under our rules) is not tracking in Apple's sense, but it still needs our own opt-in and the App Privacy label.
- **Session replay** follows `ANALYTICS_AND_FEEDBACK.md` exactly: separate opt-in, never for under-18s, inputs masked, chat/names/payment screens never recorded. Recording without prior consent is the basis of the California wiretap (CIPA) lawsuit wave. Prior opt-in consent is the defence.
- **Websites load nothing from third parties on page load** that the game doesn't need. Self-host fonts (no Google Fonts links), icons and scripts. Sign-in scripts load only when the player chooses to sign in. Embeds (YouTube, Maps, Discord) are click-to-load. (A Munich court awarded €100 per visitor for Google Fonts in 2022.)
- Crash reports without IP, email or account id where possible.
- No ad SDKs unless Andre decides otherwise; if added, contextual (not personalised) by default and never personalised for under-18s.

## 3. Age
- Decide the age policy before accounts ship, and document it in `CONSENT_PLAN.md`.
- Use a **neutral** birth-year question (no hint where the line is), remember a failed answer, and store as little as possible (e.g. birth year + "US / not US").
- **Under 13s:** no account unless the game is built for COPPA (FTC penalty ≈ $53k per violation). Studio default: **13+ in the US, 16+ elsewhere** (Egg Nest's decided policy). **[lawyer]**
- **Under 18s** get safe defaults: no open chat, no personalised ads, no session replay, analytics only with opt-in, no pressure purchase designs.
- **Adult-only (18+) content** (e.g. Interlewd): document the 18+ gating and follow each store's/Workshop's adult-content rules.

## 4. Legal documents
- Start from General Legal's free CC0 "Legal Templates" repo on GitHub, adapted to the actual game.
- `TERMS_OF_SERVICE.md` / EULA covers: acceptance; license to use the game; eligibility/age; purchases, pricing, virtual currency and items (no cash value, random-reward odds disclosed); player content (ownership, license, DMCA process); other players (chat conduct, recording/streaming others, harassment); third-party services; acceptable use (cheats, exploits, bots, griefing, **reverse engineering**, asset ripping); our IP; disclaimers; limitation of liability; indemnification; termination and bans; App Store terms (Apple/Google); governing law; dispute resolution and arbitration; state-specific notices; changes and contact. **[lawyer]**
- `PRIVACY_POLICY.md`, built from `DATA_INVENTORY.md`: what's collected and why, legal basis (GDPR), every vendor by name (e.g. PostHog), sharing, retention, security, player rights (access, deletion, correction, opt-out; GDPR, UK GDPR, CCPA/CPRA and other US state laws), children, international transfers, contact, how to delete. **[lawyer]**
- Both are published at **stable public URLs before any personal data is collected**, linked in the footer/settings and at sign-up. Keep dated copies of every published version.
- Never invent facts about the owner or company: use `[PLACEHOLDER]` and list them. Known studio facts: Nest Eggz'n Production LLC, Illinois (Cook County).

## 5. Protect the data (detail in `SECURITY_STANDARD.md`)
- **The server decides** everything of value (currency, items, inventory, results, crafting, damage, admin actions). The client is never trusted.
- **No secrets in the client or the repo.** Secrets live in host environment variables / CI secrets. Public client keys (e.g. a PostHog project key) are fine because they can only *send* data.
- Every database read and write is **scoped to the signed-in player** (server checks or row-level security). Parameterised queries only.
- **Admin / Game Master tools** check an admin role on the server, are logged (who, what, when), and can't be reached by regular accounts.
- Sign-in delegated to a real provider, or passwords hashed with bcrypt/argon2. HTTPS everywhere. Rate limits on sign-in, chat, saves and purchases. Validate everything players send. Security headers on site and server.
- Accounts that hold everything (GitHub, web host, Steamworks, Apple, Google, PostHog, Cloudflare) use two-step sign-in.

## 6. Players' rights
- Players can **download and delete their data in the game** whenever accounts exist, and via a web link for Google Play. Deletion really deletes, across every table **and every vendor** (including PostHog persons and recordings).

## 7. Player text and player-made content
- Filter all player text; report, block and mute; act on reports within 24 hours; never render player text as HTML. Apple, Google and the UK Online Safety Act expect this.
- **If players can upload anything others see** (pictures, images, levels, yards): register a **DMCA designated agent** with the US Copyright Office before launch (about $6, renew every 3 years), publish the takedown process in the Terms, remove reported content promptly, and keep a repeat-infringer policy. Without the agent you lose the safe harbor when a player uploads stolen art.

## 8. Email
- Marketing emails (waitlist, launch, newsletter) only with opt-in. Every one has a working one-click **unsubscribe** link honoured within 10 business days and a **physical postal address** (CAN-SPAM; a PO box works). Penalties are per email. Account and security emails are exempt but link to email settings. Send through a real email provider.

## 9. Real money
- Platform payment systems where required (Apple IAP, Google Play Billing, Steam Wallet). Random-reward odds published before purchase. Virtual items have no cash value.
- **Subscriptions/memberships:** before purchase, show next to the buy button the price, how often it bills, that it **auto-renews until cancelled**, and how to cancel; the player actively agrees. Send a confirmation email with the same terms. Online cancellation is as easy as signing up (California Automatic Renewal Law, B&P §17600+; otherwise what you sold can count as a free gift). Store subscriptions cover much of this; **web memberships (e.g. Egg Nest supporter) need all of it.**

## 10. Reviews and feedback
- Follow `ANALYTICS_AND_FEEDBACK.md` part B. Ask for reviews at a good moment with the **platform's own prompt**, and offer it to everyone. **No review gating** (asking "do you like it?" first and steering unhappy players away from the store); Google Play's in-app review rules forbid it. Never reward players for reviews (Steam, Apple and Google all forbid it).

## 11. Platforms (`PLATFORM_CHECKLIST.md`)
- **Steam:** privacy policy URL, EULA, content survey/age rating, which Steamworks data is used.
- **Apple:** App Privacy label, privacy manifest (`PrivacyInfo.xcprivacy`, including each SDK's), ATT only if tracking, in-app account deletion, Sign in with Apple if other social logins exist, IAP rules, loot box odds, age rating, UGC tools (Guideline 1.2).
- **Google Play:** Data Safety form, account deletion in-app **and** by web link, no `AD_ID` permission unless ads, Families policy if kids are targeted, IARC rating.
- Store form answers must match `DATA_INVENTORY.md` exactly. A mismatch gets apps rejected or pulled.

## 12. Breaches, operations and every release
- Breach: stop it, record it, assess within 72 hours, notify as `OPERATIONS.md` says (GDPR regulator within 72 hours; US states per state law). **[lawyer]**
- Before every release, and whenever a new SDK, data field or online feature is added:
  - [ ] `DATA_INVENTORY.md` updated; `PRIVACY_POLICY.md` and store forms still match it
  - [ ] Consent still gates every optional SDK; replay rules from `ANALYTICS_AND_FEEDBACK.md` hold; the website loads no third-party fonts/scripts before consent
  - [ ] Uploads → DMCA agent registered and current; marketing email → unsubscribe + address; subscriptions → renewal terms shown and easy cancel
  - [ ] Review prompt is ungated and uses the platform API
  - [ ] `SECURITY_STANDARD.md` release checklist passes
  - [ ] Published legal pages updated, with a new "Last updated" date and a dated copy kept
