# Dreams of Everus — Game Design Document

Oct 14, 2026 · @Andre Hamilton

Dreams of Everus is a 2D Metroidvania action-adventure with platform-fighter combat, capturable creatures called Everites, and 3D characters on a side-on plane. This version folds in every decision made during the October 2026 review.

## 1. Vision and pillars

Players explore a dreamlike world as one of several dream walkers, capture Everites born from a corrupted meteor, and use them as extensions of themselves in exploration and combat. Movement and combat feel like a platform fighter; progression feels like a Metroidvania with a flexible path.

**Pillars**

1. **Feel comes first.** Movement and combat responsiveness decide whether everything else works. Frame timing is central.
2. **Everites are functional.** They open routes, solve obstacles and change how you fight; they are not just a collection.
3. **Customization is a system.** Modular equipment and Everite variants are planned into the asset pipeline from the start.
4. **One foundation, many experiences.** Adventure, couch PvP and online PvP share the same character, combat and Everite systems.
5. **Build small, keep the vision.** Each milestone proves one thing and doesn't block the larger game.

**Inspirations** (principles, not rulesets to copy): Smash Bros. Brawl's Subspace Emissary (adventure encounters), Smash Wii U (custom specials), Rivals of Aether (side-on fighting feel), Lost Kingdoms (creatures as tools), Pokémon HMs and Mega Man X (ability-based exploration), Marvel vs. Capcom (assists), Ice Climbers (companion behavior), Splatoon (modular fashion), Elden Ring (flexible world path), Guilty Gear and Spider-Verse (stylized 3D animation), Cowboy Bebop and Samurai Champloo (music).

## 2. Names and terminology

These names are canon. Never replace or invent terms without the creator's approval. "Eternus" is retired everywhere.

| Term | Meaning | Status |
| --- | --- | --- |
| Dreams of Everus | Game title | Decided |
| Everus | Lore event tied to a corrupted meteor or "bad energy" | Name decided; lore open |
| Everites | The capturable creatures, born of the meteor ("dream rock") | Decided |
| Dream walkers | Beings not from the meteor, like the playable characters | Decided |
| Bun Jester | Lead playable character, the "Mario" of the roster | Decided |
| Everus button | Circle; moves set by your Everite team and equipment | Decided |
| Everus Link | Retired: folded into the Everus button (Circle) | Decided |
| Everus Tap | L1; cancel and parry, spends Dream Gauge | Decided |
| Dream Gauge | Meter from 0 to 105%; spent and built by many actions (see section 5); full gauge unlocks the dream state | Decided |
| Fragility | Damage value that drives knockback | Decided |
| Sturdy, Dull, Fragile, Busted, Broken | Fragility phases | Decided |
| Sentry | Everite mode: timed assist | Decided |
| Companion | Everite mode: wanders beside you | Decided |
| Wince | Bracing for a big hit: better directional influence or less knockback | Concept decided; trigger open |
| Classes | Subtle secondary layer on a character, such as the Dundant | Tentative |
| Psionic, Reborn, Flame, Viral | Element names; list may be incomplete | Draft rules in section 7 |
| Sad Fish, Yolwel Rock | Lore events | Developed later |

## 3. Characters and controls

**Roster.** Bun Jester is the lead and is built first; every system is proven on Bun before other characters. The launch roster is 4 to 8 selectable characters, growing over time. Each has its own specials, movement values and possibly class.

**Controls.** Controller support is required from the first prototype, using Unity's Input System with named actions. Keyboard works for testing. Players can remap everything.

| Input (PS4) | Action |
| --- | --- |
| Square | Normal attacks: tilts and smash attacks |
| Cross | Special: character-specific and directional; some can be charged or angled |
| Triangle | Jump; one air jump by default, some characters get more |
| Circle | Everus button: neutral, side, up and down versions, set by Everite team and equipment |
| L1 | Everus Tap |
| L2 | Inventory: hold for a radial menu, tap to use |
| R1 | Grab on the ground; neutral attack in the air |
| R2 | Shield; down + shield to dodge; air dodge in the air; add a direction and spend gauge for a directional dodge |
| Left stick | Movement |
| Right stick | Radial menu while L2 is held; otherwise medium or smash attacks |
| D-pad | Everite slots; can swap with inventory slots |
| Options / Share | Pause and menus (not yet assigned in detail) |

R1 needs no second slot: the game checks whether you are grounded. Design note: some players expect the right stick for aerials, so its dual role should be tested early.

## 4. Movement

The full game includes running, jumping, short hop, double jump, fast-fall, wall jump, ledge grab, dash and dodges. What each character or Everite can do varies, so all movement values live in per-character data, not hard-coded numbers.

- **Short hop** works only from the ground. Some flying types have their own variations.
- **Air jumps:** one by default; some characters get more.
- **Platforms:** thin platforms can be jumped up through and dropped down through. Floors and walls are solid. Later, some platforms are destructible, burnable, or freezable and then breakable.
- **Dodges:** spot dodge (down + shield), air dodge, and a directional dodge that spends Dream Gauge.
- **Tunable values:** run speed, acceleration, deceleration, jump height, short-hop height, gravity, fall speed, fast-fall speed, air control and air-jump count.

## 5. Combat

Combat is hitbox and hurtbox driven, measured in frames, and works like Smash: taking hits raises Fragility, and higher Fragility means more knockback.

**Fragility**

| Phase | Range | Effect |
| --- | --- | --- |
| Sturdy | 0–30 | Lowest knockback |
| Dull | 31–55 | Knockback steps up |
| Fragile | 56–80 | Knockback steps up |
| Busted | 81–100 | A strong hit can break you |
| Broken | 101–110 | Even weak attacks can break you |
| Past 110 | Over 110 | Any knockback breaks the character |

Knockback ramps up at each phase threshold. Exact numbers come from playtesting. Still open: whether Fragility recovers, and whether it applies to Everites and equipment too.

**Hit reactions:** hitstun, knockback, hit-pause, directional influence, elemental statuses and buffs, and heavy knockback from counter hits.

**Attacks:** normals (tilts and smash attacks) on Square or the right stick, character-specific specials on Cross, grab on R1, and the Everus button's four directional moves on Circle. The weapon slot connects to special moves; a custom-special system is a later goal.

**Defense**

- **Shield (R2).** Blocks attacks and doesn't shrink while held. Shields can crumble and break: some moves poke through a crumbling shield, others deal extra impact damage to crumble it faster. The crumbling is shown visually.
- **Shield break.** Heavy hitstun plus a short knockback that depends on the move that broke it. A shield broken just from holding it too long bursts, and the player crumples (similar to Street Fighter 6) and probably loses Dream Gauge.
- **Perfect shield.** Shield as the hit connects. It reflects projectiles, lets you act quickly out of shield, gains Dream Gauge, and holds the attacker in their active frames longer so you can counter.
- **Everus Tap (L1).** After landing a hit, spends Dream Gauge to cancel your animation so you can act again. With no action in progress, it is a parry: costs gauge, has a long cooldown if missed, and when landed gives invincibility frames and stalls the attacker for a punish.
- **Wince.** The moment a character sees a big hit coming and curls up to take it, like an anime beat. They still take the big hit, but get better control of directional influence or take less knockback. It replaces the helpless feeling of a guaranteed punish (for example, getting knee'd after a missed grab in Melee). Exact trigger and input are open.

**Dream Gauge**

The gauge runs from 0 to 105%. When it reaches 105% it holds briefly, then drains back to 100% if you gain nothing more.

| Spends gauge | Builds gauge |
| --- | --- |
| Directional air dodges | Hitting opponents |
| Everus Tap cancels | Getting hit |
| Some inventory items | Long combo chains |
| Summoning and using Everites | Perfect shields and parries |
| Having your shield broken | Successful dodges (tap down while shielding) |
| Several dodges in a row | Moves that leech it from the opponent |
| Spamming a move (to be defined) |  |

**Dream state.** At max gauge a player can enter a dream state that unlocks upgraded abilities and subtle perks. Each character taps into different things, and gear and Everites change it too. How long it lasts and how to trigger it are open.

## 6. Everites

Everites are creatures born of the corrupted meteor, and any Everite can be captured. The player character stays in control; Everites extend what the player can do. The game should have Pokémon's "hunting for the right creature" feel, and the grind should be fun platform action.

**Capturing.** A dream walker first puts an Everite into a capturable state through damage, status effects or items. The player then catches it with a dream catcher. Like Pokémon, hitting it with the right item while it is in certain states improves the odds.

**Finding Everites**

- **Instances:** enterable play areas, possibly procedurally generated, each with a different pool of Everites.
- **Camps:** set up camp outside towns to bait and search for specific Everites on specific routes, similar to dens in Pokémon Sword and Shield.

**Roster and evolution.** Each playable character has 15 Everites: 5 families of 3 forms, loosely designed around that character. Four characters means 60 Everites. Evolution goes creature to creature through 1st, 2nd and 3rd forms, which also helps balance.

**Ways to use an Everite**

| Mode | How it works | Inspiration |
| --- | --- | --- |
| Sentry | Summoned as an assist on a timer | Marvel vs. Capcom assists |
| Companion | Wanders beside you with trained behaviors; needs its own movement and AI | Nana and Popo |
| Remote control (name open) | Sent out and steered directly while your character stands still | — |

Summoning and using Everites spends Dream Gauge. Everites sit on the D-pad slots, which can swap with inventory slots. Whether they can be hurt, knocked out or stay on screen depends on the loadout.

**Companion training.** Companion behavior can be trained, similar to amiibo in Smash. At high levels, tuning that behavior to fit your play style is key. The Obedience stat affects how well behaviors stick, and key items unlock more behavior options.

**Breeding and molds.** Breeding is not male and female. Players collect "dirts" (Everite drops) to craft base molds of Everites, and use them to build their perfect companion. Details are open, but the system ties into both gameplay and story.

**Stats**

| Type | Stats |
| --- | --- |
| Base stats | Speed, Strength, Attack, Special Attack, Defense, Special Defense, Obedience |
| Augment stats (examples) | Knockback, impact type, beam %, ball %, size |

Every move carries its own base stats and details. The Everite's stats and body, gear and abilities then adjust them. A creature with a cannonball-sized mouth should fire a stronger beam than one with a tennis-ball mouth. Instead of flat Pokémon-style EV training, different training methods adjust these stats and percentages (a separate system, still to be designed). Open: how Strength differs from Attack, and the full augment list.

**Classes (tentative, may be cut).** A subtle secondary layer on top of the character that changes how you interact with Everites and equipment. Examples: the **Dundant** gets perks for carrying several of the same Everite or type; another class would reward synergy with Companion behaviors.

**Utility.** Everites open routes, solve obstacles and add combat options. Some abilities gate the main path; many open optional routes and rewards.

**Variants.** Individual Everites differ in coat, palette, markings, ears, tails and other features, without breaking animations. Species can use different rigs (quadruped, bird, serpent, humanoid). Appearance stays separate from combat values in the code.

## 7. World, progression and customization

**Structure.** A general path with flexibility, in the spirit of Elden Ring, ending at a final boss you prepare for. Roughly 8 bosses for v1. Areas are worth revisiting because new abilities open optional routes, shortcuts and secrets. Connected rooms, transitions and checkpoints come later; a map editor is a later ambition.

**Elements: draft proposal for your review (not canon yet)**

Elements change *how* a hit behaves rather than winning or losing a rock-paper-scissors chart. Each element has three parts: a combat effect that rides on hits, a world interaction, and a clear counter. Type advantages stay small, so the skill of landing the move matters more than the matchup.

| Element | Combat effect | World interaction | Countered by |
| --- | --- | --- | --- |
| Flame | Burn: Fragility rises a little over time; burning targets take slightly more knockback | Burns vines, wooden platforms and barriers; lights dark areas | Dodging off the burn; Reborn cleanses it |
| Psionic | Control: pulls, pushes and bends knockback angles; weakens the target's directional influence | Moves objects, triggers distant switches | Wince restores directional influence |
| Viral | Spread: a debuff that jumps to nearby enemies and Everites, slows Dream Gauge gain and crumbles shields faster | Corrodes barriers, infects plants to change routes | Flame burns it away |
| Reborn | Renewal: lowers your own Fragility, cleanses statuses, revives knocked-out Everites | Regrows bridges and plants, restores broken objects | Viral blocks its healing |

The four form a loop of reactions rather than a damage chart: Flame clears Viral, Viral blocks Reborn, Reborn cleanses Flame, and Psionic redirects all of them. A move's element combines with its augment stats (beam %, impact type, size) to set the final effect. Moves without an element are neutral. Open: are there more elements, and should frozen platforms come from a separate Ice element or something else?

**Saving.** Progress is saved per character, plus a shared level on the main profile.

**Death.** On defeat you drop materials and can go back to recover them. Crafting and customization materials are the main thing at stake. Some quests require finishing without dying.

**Equipment.** Four slots: Hat, Top, Bottom and Weapon. Clothing can change appearance and possibly stats; cosmetics and stats stay separable so players aren't forced into one look. The Weapon slot connects to special moves. Built as modular 3D pieces on shared rigs. Crafting and item acquisition are not yet designed.

**Story and lore.** The corrupted meteor ("dream rock") creates Everites and triggers the Everus event. Playable characters are dream walkers. A held idea for the ending: achieving something that makes your dreams come true, with endings that may differ by player. Sad Fish and Yolwel Rock are lore events, to be developed later. No lore should be invented to fill gaps.

**Scope changes:** base-building will probably be dropped. Battle royale becomes a game mode, most likely endgame content (see section 8).

## 8. Modes and multiplayer

The game launches on PC only. Couch PvP and peer-to-peer online PvP are both planned.

| Mode | Description | Status |
| --- | --- | --- |
| Adventure | Single-player Metroidvania campaign | Primary mode |
| Traditional fighting | Characters fight on movement, attacks and defense; Everites limited | Planned |
| Event battles | Characters, Everites, equipment and dynamic objectives; DOTA-like energy, not a MOBA | Exploratory |
| Couch PvP | Same-screen PvP | Wanted |
| Online PvP | Peer-to-peer | Wanted; built after the core rules are stable |
| Battle royale | A mode setting, most likely endgame content; format to be defined | Planned |

All modes share one set of character, combat and Everite systems; each mode switches parts on or off.

**Why it matters early:** online fighting games usually use rollback netcode, which needs every frame to play out exactly the same on both PCs. Building combat as fixed-step, frame-by-frame logic from the start keeps that door open cheaply.

## 9. Presentation and technical approach

3D characters and Everites play on a 2D side-on plane. What the model looks like never decides a hit: dedicated hitboxes and hurtboxes do.

| Area | Decision | Status |
| --- | --- | --- |
| Engine | Unity 6000.6.5f1, Windows, 3D project with URP | Decided |
| Camera | Perspective, side-on; follows the player in combat; story moments can move, pull back or push in | Decided |
| Character controller | Rollback-ready, like Rivals of Aether and Smash: a custom kinematic controller with its own simple box collision (no Unity physics for gameplay), inputs read once per frame, game state that can be saved and rewound, and fixed-point math for all gameplay numbers (visuals can use normal decimals) | Decided |
| Game timing | Logic fixed at 60 updates per second | Decided |
| Input | Unity Input System, named actions, remappable | Decided |
| Version control | Git repo exists locally; check the Unity ignore file and connect it to GitHub for an online backup | In progress |
| Visual style | Stylized, expressive animation (toon shading) | Direction set |
| Music | Upbeat, hip-hop leaning | Direction set |
| Menus and HUD | Built for players going in and out often to check Everite stats, armor and the map | Direction set |

**Who does what.** Solo developer for now, possibly a team later. The creator makes the art. AI agents write all the code and help make assets. Unity Editor work comes as step-by-step instructions.

**Art pipeline: options for Bun**

Milestone 1 uses a placeholder shape, so art doesn't block coding. Bun's model is needed by milestone 3 (the 3D test). AI image-to-3D tools make fast first drafts, but rarely the clean, animation-friendly mesh a character needs to bend well and wear modular outfits. Plan for a cleanup step in Blender.

| Step | Options | Notes |
| --- | --- | --- |
| 1. Reference | Your art, plus AI image tools for front, side and back views | Generators work best from a clean front view in a T-pose |
| 2. Draft model | Tripo, Meshy, Hyper3D Rodin, Hunyuan3D (free but needs a strong graphics card) | Quality varies on organic shapes; test each on Bun |
| 3. Cleanup | Blender: rebuild the mesh with clean, even geometry ("retopology") | Needed for good bending and outfits that fit |
| 4. Rigging | Mixamo (free, humanoids only), AccuRIG, auto-riggers in Tripo or Meshy (vendors claim quadruped support), or by hand in Blender | Unusual Everite bodies will likely need hand-built rigs |
| 5. Automation | Blender MCP: lets Claude drive Blender, run Blender scripts and pull models from Tripo, Hunyuan3D or Rodin | Third-party add-on; save often and keep it on your own computer only |
| 6. Into Unity | Export FBX, then a URP toon shader for the stylized look | Shader choice comes with the 3D test |

**Recommendation:** run a "Bun bake-off". Feed one clean front view of Bun into two or three generators, compare the results, then clean up the best one in Blender. Build Bun's base body carefully, because every outfit sits on it. Use AI drafts more freely for props and early Everite tests.

Sources (October 2026): [image-to-3D comparison](https://www.3daistudio.com/3d-generator-ai-comparison-alternatives-guide/best-image-to-3d-tools-2026) (published by a vendor), [auto-rigging guide](https://app.cinevva.com/guides/free-character-animations-rigging), [Blender MCP](https://mcpservers.org/hi/servers/ahujasid/blender-mcp).

## 10. Roadmap

Each milestone proves one thing and is playable when done. Suggested first big target: a vertical slice with Bun, one region, one boss and three Everites.

**Milestone 1: Movement Sandbox V0.1**

- Step 0: check the existing Unity project and Git repo, add a Unity ignore file if missing, and connect the repo to GitHub.
- A graybox room: floor, solid walls, platforms at different heights including pass-through platforms, a fall zone and a fixed respawn point.
- A placeholder Bun with walk, dash (double-tap forward or back), trot, dash dance, run, run turnaround, jump, short hop, one air jump, fast-fall and drop-through. **Trot** is the length of a character's initial dash; one press of the opposite direction during it dashes the other way (dash dance); turning after it, while running, forces the slower run turnaround. All values are per character.
- A light attack (Square) with a visible hitbox and a standing training dummy that reacts when hit. No damage, Fragility or knockback yet.
- The rollback-ready foundation: a fixed 60-per-second game step, inputs read once per frame, custom collision, no Unity physics for gameplay, fixed-point math for gameplay numbers.
- Side-on follow camera; controller and keyboard input through named actions.
- Movement values and attack frame data tunable in the Editor and stored as per-character data.
- **Done when:** you press Play, move and jump between platforms with a controller, throw a light attack that visibly hits the dummy, fall, respawn, and tune values.
- **Not included:** damage, Fragility, knockback, specials, shields, Everites, equipment, Dream Gauge, elements, world map, saves, multiplayer, menus.

**Later milestones**

2. Movement refinement: wall jump, ledge grab, spot dodge and air dodge. (Dash moved into Milestone 1, October 2026.)
3. 3D presentation test: one rigged model running and jumping on the 2D plane.
4. Combat room: normals, specials, hitboxes, hurtboxes, Fragility, knockback, hitstun and hit-pause.
5. Defense and gauges: shield, perfect shield, Everus Tap, Dream Gauge, directional dodge.
6. First Everite: one Sentry-mode Everite on the Everus button.
7. Exploration gate: one Everite ability opening an optional route.
8. Basic capture: weaken an Everite into a capturable state, catch it with a dream catcher, slot it on the D-pad.
9. Modular equipment: one clothing slot and the weapon slot with a special.
10. World structure: connected rooms, checkpoints, saving and death with recoverable materials.
11. Stats and progression for characters and Everites.
12. More Everites, Companion and remote-control modes, visual variants.
13. Couch PvP.
14. Adventure content: region, enemies and the first boss.
15. Additional characters.
16. Online peer-to-peer PvP.

## 11. Open questions

Nothing blocks Milestone 1. Everything below is sorted by the milestone that first needs it.

| Question | Needed by |
| --- | --- |
| Art: results of the Bun bake-off and the tools chosen | Milestone 3 (3D test) |
| Fragility: does it recover? Does it apply to Everites or equipment? | Milestone 4 (combat) |
| Wince: what triggers it, and is there an input? | Milestone 5 (defense) |
| Shield: how fast it crumbles, and how it recovers | Milestone 5 |
| Dream state: trigger, duration and each character's perks | Milestone 5 |
| Elements: approve or change the draft; any more elements? | Milestone 7 (exploration gate) |
| Capture odds and types of dream catcher | Milestone 8 (capture) |
| Strength vs. Attack, the full augment stat list, training methods | Milestone 11 (stats) |
| Classes: keep or cut? | Milestone 11 |
| Name for remote-control mode | Milestone 12 |
| Instances (procedural or hand-built) and camps | Later |
| Breeding with dirts and molds | Later |
| Crafting and how equipment is earned | Later |
| Battle royale format | Later |

## 12. Rules for Claude Code

A condensed version of these goes into CLAUDE.md in the Unity project, so every session follows them.

1. Use the names in section 2 exactly. Never use "Eternus". Never rename or invent terms.
2. Never invent lore, mechanics, stats or numbers and present them as decided. If something is open, use a clearly labeled placeholder and ask.
3. Build only the current milestone. Leave room for later systems, but don't build them early.
4. Gameplay logic runs on a fixed 60-per-second step using fixed-point math, never Unity physics or Time.deltaTime, so it stays rollback-ready. Hits come from hitboxes, never from model shapes.
5. Keep input, movement, combat, Everites, visuals and data separate, so a 3D model or new ability can be added without rewriting movement.
6. Store tunable values (speeds, jump heights, frame data) in data files the creator can edit in the Editor, never hard-coded.
7. The creator is learning. Explain each change in plain words, keep code readable, and give step-by-step instructions for anything done in the Unity Editor.
8. Commit to Git after each working step, with a clear message.
9. Finish each milestone with a short test checklist the creator can follow.
