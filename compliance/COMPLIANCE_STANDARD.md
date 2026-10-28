# Compliance Standard (all games)

The shared rulebook for every game in this studio. It applies to every project, every platform (Steam, iOS, Google Play, any website) and every Claude session. It is not legal advice. Anything marked **[lawyer]** should be checked by a lawyer before launch.

The three rules behind everything below:
1. **Have real legal documents** (Terms/EULA + Privacy Policy) that match what the game actually does.
2. **Get consent before any tracking or analytics.**
3. **Protect the data**: the server is the authority, and each player can only reach their own data.

---

## 1. Collect as little as possible
- Before adding any data field, SDK, account feature, chat, purchase or analytics, ask: *does the game need this to work?* If not, don't collect it.
- Every piece of personal or device data must be listed in `compliance/DATA_INVENTORY.md` (what, why, where stored, shared with whom, kept how long, consent needed?). **If it isn't in the inventory, it doesn't ship.**
- Personal data includes: Steam ID, email, username, IP address, device/advertising ID, location, voice/text chat, player-made content, purchase history, crash logs that contain any of the above, age/birthdate.
- Keep data only as long as needed. Write the retention period down.

## 2. Consent before tracking
- Analytics, ad SDKs, attribution and anything not strictly needed for the game to run stay **off until the player opts in** (first-launch privacy screen with Accept / Decline, changeable later in Settings).
- iOS: show the App Tracking Transparency prompt before any tracking or use of the advertising ID.
- Default to no ad SDKs. Adding one is an owner decision, not a coding decision.
- Crash reporting: strip personal info where possible, and disclose it in the Privacy Policy.
- **No session replay** (tools that record clicks, scrolls and keystrokes). If one is ever approved, it must mask every input field and never capture passwords or payment details. Session-replay recording is the basis of a wave of California wiretap (CIPA) lawsuits.
- **Websites load nothing from third parties before consent.** Self-host fonts (no Google Fonts links), icons and scripts. Embeds (YouTube, Maps, Discord widgets) only load after the visitor clicks to allow them. Loading Google Fonts from Google's servers sends the visitor's IP to Google; a Munich court awarded €100 per visitor for this in 2022.

## 3. Age rules
- Every project documents its age policy in `compliance/CONSENT_PLAN.md`.
- **Under 13 possible** (or under 16 in the EU) → COPPA / GDPR-K apply. Use a neutral birthdate age gate, and turn off chat, purchases and analytics for minors, or block them. **[lawyer]**
- **Adult-only (18+) content** → document the 18+ gating and follow each store's adult-content rules.

## 4. Legal documents (per project, in `compliance/`)
Start from General Legal's free CC0 "Legal Templates" repo on GitHub, adapted to the actual game.
- `TERMS_OF_SERVICE.md` / EULA, covering: acceptance; license to use the game; eligibility/age; purchases, pricing, virtual currency and items (no cash value, random reward odds disclosed); player content (ownership and license); other players (chat conduct, recording or streaming others, harassment); third-party services; acceptable use (cheats, exploits, bots, griefing, reverse engineering); our IP; disclaimers; limitation of liability; indemnification; termination and bans; App Store terms (Apple/Google); governing law; dispute resolution and arbitration; state-specific notices; changes and contact. **[lawyer]**
- `PRIVACY_POLICY.md`, built from `DATA_INVENTORY.md`: what's collected and why, legal basis (GDPR), sharing, retention, security, player rights (access, deletion, correction, opt-out; GDPR, UK GDPR, CCPA/CPRA and other US state laws), children, international transfers, contact, how to request deletion. **[lawyer]**
- Both must be hosted at a **public URL** before any store submission.
- Unknown owner details (company/entity name, contact email, governing state) are written as `[PLACEHOLDER]` and listed for the owner. Never invent them.

## 5. Platform requirements (`compliance/PLATFORM_CHECKLIST.md`)
- **Steam:** privacy policy URL, EULA on store page, content survey/age rating, list of what Steamworks data is used.
- **Apple:** App Privacy "nutrition label", ATT, in-app account deletion if accounts exist, Sign in with Apple if any other social login is offered, Apple in-app purchase rules, loot box odds shown before purchase, age rating.
- **Google Play:** Data Safety form, account deletion in-app **and** via a web link, Families policy if kids are targeted, IARC rating.
- The answers to these forms must match `DATA_INVENTORY.md` exactly. A mismatch gets apps rejected or pulled.

## 6. Players' rights and incidents (`compliance/OPERATIONS.md`)
- A working way for a player to get a copy of their data and to delete their account and data, with the actual steps or scripts.
- Data breach checklist: contain, assess, notify (GDPR: regulator within 72 hours; US states: per state law). **[lawyer]**

## 7. Player-made content and chat
- Any chat or user-generated content needs: report, block and mute tools, a moderation process, and rules in the Terms. Both app stores require this.

## 8. Every release / every new feature
Re-run before shipping, and whenever a new SDK, data field or online feature is added:
- [ ] `DATA_INVENTORY.md` updated
- [ ] `PRIVACY_POLICY.md` and store forms still match the inventory
- [ ] Consent still gates every non-essential SDK
- [ ] Security checklist in `SECURITY_STANDARD.md` passes
- [ ] Hosted legal pages updated, with a new "Last updated" date
