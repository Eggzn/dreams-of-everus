# Analytics, Session Replay and Reviews (studio-wide playbook)

> **Part of The Protocol** (source of truth: `C:\Users\Andre\Desktop\AIGames\the-protocol\standards\`). Each repo's `compliance/` folder holds an identical copy. Change it in The Protocol, then sync (prompt 07).
>
> Shared by every game repo, identical everywhere. It covers the two tools from the @ayoships video, vetted against our legal rules:
> 1. **PostHog analytics + session replays**: see where players get stuck, quit or get confused.
> 2. **Asking for reviews at the right moment**, done the allowed way (no review gating).
>
> `COMPLIANCE_STANDARD.md` wins if anything here seems to conflict with it.

---

## Part 0: Andre's setup (about 10 minutes per game, done once)

1. Sign in at **posthog.com** (US Cloud). Use one organization for the studio (Nest Eggz'n Production) and **one project per game**, plus one shared **"Dev / testing"** project so editor and dev sessions never mix with real players.
2. In each game's project → **Settings**:
   - **Replay:** turn on *Record user sessions* (recordings don't work without it).
   - **Privacy:** turn on *Discard client IP data*. Keep input masking on.
   - **Data retention:** note the retention period. The privacy policy must state it (the free plan keeps data up to 1 year).
3. **Organization settings:** sign PostHog's **Data Processing Agreement (DPA)**. GDPR requires it, and it's what makes PostHog our "service provider" rather than someone we "share" data with.
4. Turn on two-step sign-in for the PostHog account.
5. Copy the project's **Project API key** (starts `phc_`) and give it to Claude Code with this one line:

   > **"Set up PostHog for this project following `compliance/ANALYTICS_AND_FEEDBACK.md`. Project key: phc_XXXX, host: https://us.i.posthog.com, dev project key: phc_YYYY."**

   Claude Code does everything else: the consent screen, the SDK, masking, events, the review prompt, the feedback button, and the legal doc updates in Part D.

Free each month (Oct 2026): 1,000,000 events, 5,000 web recordings, 2,500 mobile recordings, 1,500 survey responses. That's plenty for testing and early launch.

**Watching replays:** PostHog → Replay. Filter by "rage clicks", "quit during tutorial", or an event like `level_failed`, and use *Summarize this recording* for the AI summary. Look at a handful every week. That's the "sit next to your player" habit from the video.

---

## Part A: Analytics and session replay

### A1. Consent flow (every game, every platform)

- Nothing from PostHog loads or initializes until the player says yes. There are **two separate switches** on the first-launch privacy card and in **Settings → Privacy**:
  - **"Help improve the game"** = anonymous gameplay analytics (events).
  - **"Share gameplay recordings"** = session replay. Off by default, its own yes, and it says plainly: "We record what happens on screen while you play (not your typing) to find confusing parts."
- Equal-size Yes / No buttons, nothing pre-ticked, closing = No.
- **Under 18 (by the game's age check): replay is never offered.** Analytics needs its own opt-in, as for everyone. Where there's no age check (e.g. single-player Steam games with no accounts), show replay only to players who join the **playtest/beta** branch, and state 18+ in the playtest sign-up.
- Turning a switch off calls PostHog's opt-out (web `opt_out_capturing()`, Unity `PostHog.OptOut()`, iOS equivalent) and stops recording immediately.
- Save the choice, its date and the privacy-policy version.

### A2. What gets sent

- **Identity:** a random player id (the account's internal id, or a random id stored locally). **Never** email, real name, Steam name, IP or device ad ID. Use PostHog's "identified only" person profiles on web.
- **Events (all games):** `game_started`, `session_ended` (length), `tutorial_step` (step), `level_started` / `level_completed` / `level_failed` (level, time, cause), `quit_to_menu` (where), `settings_changed` (which), `store_opened` / `purchase_completed` (item id, no payment data), `review_prompt_shown`, `feedback_sent`, `error` (crash/exception without personal data).
- Plus 3–5 game-specific events listed in the project's `DATA_INVENTORY.md` (e.g. Egg Nest `egg_hatched`, Block Brawl `chain_made`, Vessel `spell_cast` / `vessel_possessed`).
- **No free text** from players goes into events (no chat, names or notes).

### A3. Session replay rules (non-negotiable)

- Only with the separate replay opt-in, never for under-18s (A1).
- **Never record:** chat, player/Steam names, friend lists, sign-in, account, birth-year, payment and email screens. Mask or exclude them, or pause recording while they're open.
- All text inputs masked.
- Recordings are deleted with the player's account (§6 of the standard): delete the PostHog person, which removes their recordings.

### A4. Per platform (Claude Code: check PostHog's current docs for exact option names before coding)

**Web (Egg Nest, Block Brawl web)**
- `posthog-js` from npm, **dynamically imported by the consent module** only after Yes. No script tag on the page.
- Replay: inputs masked (default); mask player-text elements with `maskTextSelector`; add the `ph-no-capture` class to chat, names, sign-in, account and payment panels.
- **Canvas games** (three.js/Phaser) draw on a `<canvas>`, which replay doesn't show unless canvas capture is enabled. Enable it at a low frame rate and quality to keep data small.
- Respect Global Privacy Control as "No" to anything beyond analytics.

**iOS (Block Brawl app, future phone apps)**
- `posthog-ios` via Swift Package Manager; initialize from the consent store only after Yes.
- Replay on, with all text inputs masked (SwiftUI `SecureField` is always masked). Use `.postHogMask()` / the `ph-no-capture` accessibility identifier on friend lists, names and account views.
- SpriteKit game boards may not appear in the default replay mode. Use the SDK's screenshot mode for the game screen if needed, still masking UI as above.
- No ATT prompt needed for PostHog under these rules. The App Privacy label and privacy manifest must list it (Part D).

**Unity (Dreams of Everus, Vessel, ShardSlapper; Unity 6 is supported)**
- Package Manager → *Add package from git URL* → `https://github.com/PostHog/posthog-unity.git?path=com.posthog.unity`.
- **Code-based setup only, after consent.** No `PostHogSettings.asset` in Resources (it auto-starts before consent).
- Events via `PostHog.Capture(...)` and screens via `PostHog.Screen(...)`. Opt-out via `PostHog.OptOut()` / `PostHog.OptIn()`.
- **Unity replay is screenshot-based with no built-in masking.** So: replay only for opted-in playtest/beta players (A1). Hide or blur chat, nameplates (co-op/invasion players' Steam names) and account UI while recording, or don't enable replay in builds that show them. Keep `CaptureLogs` off unless logs are checked to contain no personal data.
- Replay does not work in WebGL builds.
- Editor and development builds send to the **Dev** project only, never the live one.

**Tabletop Simulator (Interlewd): no analytics and no replay.** A Workshop mod can't show a proper consent flow, and players don't expect tracking. Use Workshop ratings, comments and Discord for feedback.

---

## Part B: Reviews and feedback (the allowed version of the video's trick)

The video's idea of a popup that sends happy players to the store and unhappy players to a private form is **review gating**. Google Play's in-app review rules forbid asking questions (like "Do you like the app?") before or while showing the review card. Apple expects its own review prompt. We don't do it. We keep the good part instead: **ask at the right moment, and give unhappy players an easy place to vent.**

- **When to ask:** after a positive moment (a win, a chapter cleared, a new creature hatched), from the 3rd session onward. Never on first launch, after a loss, mid-match or in a menu loop. At most once every few weeks per player.
- **How:** the platform's own prompt, offered to everyone:
  - iOS (SwiftUI): `@Environment(\.requestReview)` → `requestReview()`. Apple limits how often it really appears, so don't promise a prompt.
  - Unity iOS: `UnityEngine.iOS.Device.RequestStoreReview()`. Unity Android: the Google Play In-App Review plugin, called directly with no question first.
  - Steam: no review API. Show a small "Enjoying the game? Leave a review on Steam" button on the victory/credits screen that opens the store page in the Steam overlay.
  - Web: a link to the game's store page once there is one.
- **Never** reward reviews (items, currency, unlocks) or hide the review option from anyone. Steam, Apple and Google all forbid incentivised reviews.
- **Feedback is separate and always available:** a "Send feedback / Report a bug" button in Settings and the pause menu (on web/iOS a PostHog survey works). It is never shown as the alternative to a review in the same popup. Feedback text is listed in `DATA_INVENTORY.md`; no email is asked for unless the player wants a reply.

---

## Part C: Per-project plan

| Project | Platforms | Analytics | Session replay | Reviews / feedback |
|---|---|---|---|---|
| **Egg Nest** | Web (later phones) | Opt-in, 13+/16+ accounts; players without accounts use a local random id | Separate opt-in, **18+ by birth year only**, canvas capture on, `ph-no-capture` on chat/names/account | Feedback button + PostHog survey now; store prompts when apps exist |
| **Block Brawl** *(partner reviews; work on a branch)* | iOS + web | Opt-in | Opt-in, adults only; friend lists and friend codes masked | Native iOS prompt after wins; feedback in Settings. **Needs an age policy first**: if under-13s may play, no analytics for them |
| **Dreams of Everus** | Unity, Steam | Opt-in | Playtest/beta opt-in only | Steam review button on chapter/credits screens; feedback in pause menu |
| **Vessel** | Unity, Steam | Opt-in | Playtest/beta opt-in only; hide invader/co-op nameplates while recording | Same as Dreams |
| **ShardSlapper** | Unity, Steam + phones | Opt-in | Playtest/beta opt-in only; hide nameplates/lobby names | Steam button; native iOS/Android prompts on mobile |
| **Interlewd** | TTS Workshop (18+) | None | None | Workshop ratings/comments, Discord |

---

## Part D: Legal and doc updates Claude Code makes when adding PostHog

Do these **in the same change** that adds the SDK. Before that, the docs say PostHog is *planned*, never *live*.

1. **`DATA_INVENTORY.md`**: rows for gameplay events, session recordings, random player id, feedback text, and the consent record, each with vendor **PostHog Inc. (US Cloud)**, purpose, retention, and "consent: yes (opt-in)".
2. **`PRIVACY_POLICY.md`**: add a section like:
   > *"With your permission, we use PostHog (PostHog Inc., USA) to understand how the game is played: which screens you visit, where you get stuck and when the game has errors. If you also turn on 'Share gameplay recordings', PostHog records what appears on your screen while you play, with typing and personal details hidden. We use a random ID, not your name or email, and we don't store your IP address. You can turn either setting off at any time in Settings → Privacy; we keep this data for [RETENTION] and delete it when you delete your account. PostHog processes it only on our behalf under a data processing agreement. Data is stored in the United States [international transfer wording, lawyer]."*
3. **`CONSENT_PLAN.md`**: the two switches, the age rule, where the choice is stored.
4. **`PLATFORM_CHECKLIST.md`**:
   - Apple App Privacy: *Usage Data → Product Interaction* and *Diagnostics*; "used for analytics/app functionality"; "not used for tracking"; linked to the user only if tied to an account id. Add PostHog's privacy manifest to the app's.
   - Google Play Data safety: *App activity → App interactions*, *App info and performance → Crash logs/Diagnostics*; collected, **not shared** (service provider), **optional**, encrypted in transit, deletable.
   - Steam: the privacy policy URL covers it.
5. **`OPERATIONS.md`**: account deletion also deletes the PostHog person and recordings, via the PostHog API from the **server** with a personal API key kept in server secrets only.
6. **`TERMS_OF_SERVICE.md`**: no change needed beyond the existing third-party-services section naming PostHog.
