# Dreams of Everus: Code Guide (Milestone 1)

This guide explains every system in the Milestone 1 movement sandbox: what it does, how it works, and where to find it. Every code file is reproduced line by line with explanations beside the lines.

**How to read it.** Start with Part 1, the big picture. You don't need to read Part 3 top to bottom. Use the "Where do I find..." table to jump to the file you care about.

In the annotated code, `◀` marks an explanation added for this guide. Those notes are not in the real files. Comments without `◀` are real comments from the code itself. Each class and function in the real files also starts with a `/// <summary>` description. Those are folded into the "What it is" note above each file instead of being repeated in the code.

**Written for:** the code as of the end of Milestone 1, October 2026. If the code changes later, the real files win.

---

## Part 1. The big picture

### The one rule everything follows

The game is split into four separate parts. Information flows in one direction only:

```
  ┌──────────────┐     ┌──────────────────────┐     ┌────────────────────────┐
  │    INPUT     │ ──▶ │      SIMULATION      │ ──▶ │      PRESENTATION      │
  │ controller / │     │  the actual game:    │     │  what you see: capsule,│
  │ keyboard     │     │  movement, collision,│     │  dummy flash, hitbox,  │
  │              │     │  attacks. 60 ticks/s │     │  camera, readout       │
  └──────────────┘     └──────────────────────┘     └────────────────────────┘
                                  ▲
                       ┌──────────┴───────────┐
                       │         DATA         │
                       │ tuning values you    │
                       │ edit in the Inspector│
                       └──────────────────────┘
```

- **Input** turns button presses into one small, exact `InputFrame` per tick.
- **Simulation** is the game itself. It takes the current `GameState` plus one `InputFrame` and produces the next `GameState`. It knows nothing about Unity: no GameObjects, no physics, no `Time.deltaTime`.
- **Presentation** reads the `GameState` and draws it. It never changes it.
- **Data** holds the tuning values (speeds, frame counts, hitbox sizes) in assets you edit in the Inspector.

This one-way flow is what lets the game support rollback later (online play that rewinds and replays). The simulation is the only thing that decides what happens, and it always gives the same answer for the same inputs.

### What happens in one tick (1/60th of a second)

Every tick, `GameSimulation.Step` does exactly this, in this order:

```
 1. FighterInput.Read            "What does this input mean?"   (presses, hard presses, holds)
 2. FighterMovement.UpdateBeforeMove   "What should Bun do?"   (pick action, set velocity)
 3. KinematicMover.Move          "Move Bun, stop at walls/floors"   (collision)
 4. CombatSystem.Step            "Did a hitbox touch the dummy?"
 5. FighterMovement.UpdateAfterMove    "Did Bun land or walk off a ledge?"
 6. Fall zone check              "Touching the red zone? Respawn."
 7. Remember this input, add 1 to the tick counter
```

Meanwhile, on the Unity side, every rendered frame:

```
 PlayerInputReader.Update   notes any button presses this frame
 SimulationRunner.Update    runs 0, 1 or more ticks depending on how much real time passed
 FighterView.LateUpdate     moves the capsule to where the simulation says Bun is
 TrainingDummyView, HitboxView, FollowCamera, DebugHud   draw the rest
```

### Folder map

```
Assets/Game/
├── Scripts/
│   ├── Simulation/      The game rules. Pure C#, Unity is switched OFF here (see "assemblies" below).
│   │   ├── Math/        Fixed, FixedVector2 — exact decimal numbers
│   │   ├── Core/        InputFrame, GameState, FighterConfig, GameSimulation, SnapshotReplay
│   │   ├── Collision/   FixedBox, KinematicMover — boxes and moving through the level
│   │   ├── Level/       LevelGeometry — the level's boxes and respawn point
│   │   ├── Fighter/     FighterInput, FighterMovement — every movement rule
│   │   └── Combat/      AttackConfig, CombatSystem — attacks, hitboxes, the dummy
│   ├── Game/            The Unity side: reading input, running ticks, drawing, data assets
│   │   ├── SimulationRunner.cs   the bridge between Unity frames and simulation ticks
│   │   ├── Input/       PlayerInputReader, InputFrameBuilder, ButtonLatch
│   │   ├── Data/        CharacterData, InputFeelData, AttackData, FollowCameraData (Inspector assets)
│   │   ├── Level/       LevelBox, RespawnPoint, LevelBaker — building the level from scene cubes
│   │   ├── Combat/      TrainingDummy — places the dummy in the scene
│   │   └── View/        FighterView, TrainingDummyView, HitboxView, FollowCamera, DebugHud, SnapshotDebugger
│   └── Editor/          MovementSandboxBuilder — the "Build Movement Sandbox" menu item
├── Tests/EditMode/      Automated tests (run them from Window > General > Test Runner)
├── Data/                The actual tuning assets: BunCharacterData, BunLightAttack, InputFeel, FollowCamera
├── Input/               GameplayControls — the named input actions and their buttons
├── Scenes/              MovementSandbox
└── Art/Graybox/         Placeholder materials
```

### Where do I find...

| I want to change or understand... | Look in | Specifically |
|---|---|---|
| Bun's speeds, jumps, gravity, traction, moonwalk | `Data/BunCharacterData` asset | Inspector; code in `CharacterData.cs` |
| The attack's frames or hitbox | `Data/BunLightAttack` asset | Inspector; code in `AttackData.cs` |
| Deadzone, hard press timing, walk key tilt, crouch threshold | `Data/InputFeel` asset | Inspector; code in `InputFeelData.cs` |
| Camera distance, angle, smoothing | `Data/FollowCamera` asset | Inspector; code in `FollowCameraData.cs` |
| Which buttons do what | `Input/GameplayControls` | Double-click it to open the Input Actions editor |
| How walking, dashing, running work | `FighterMovement.cs` | `UpdateIdleOrWalk`, `UpdateDash`, `UpdateRun` |
| Dash dance and moonwalk | `FighterMovement.cs` | `UpdateDash` |
| Run turnaround | `FighterMovement.cs` | `UpdateRunTurnaround` |
| Crouch and crawl | `FighterMovement.cs` | `UpdateCrouchOrCrawl`; height in `FighterConfig.HeightFor` |
| Jump squat, short hop, momentum | `FighterMovement.cs` | `UpdateJumpSquat` |
| Air jump, gravity, fast-fall, air steering | `FighterMovement.cs` | `UpdateAir`, `UpdateAirControl` |
| What counts as a "hard press" | `FighterInput.cs` | `IsHardPress`, `CountPush` |
| Walls, floors, platforms, drop-through | `KinematicMover.cs` | `MoveHorizontal`, `MoveVertical`, `TryDropThrough` |
| Hitbox vs hurtbox, hitting the dummy | `CombatSystem.cs` | `TryGetActiveHitbox`, `Step` |
| The order things happen in a tick | `GameSimulation.cs` | `Step` |
| Everything that gets saved in a snapshot | `GameState.cs` | `FighterState`, `DummyState`, `GameState` |
| How 60 ticks per second are kept steady | `SimulationRunner.cs` | `Update` |
| Rollback / snapshot replay | `SnapshotReplay.cs`, `SnapshotDebugger.cs` | F5 / F9 / F10 |
| How the room is built | `MovementSandboxBuilder.cs` | `Build` |
| Level pieces (solid, pass-through, fall zone) | `LevelBox.cs` | the **Kind** dropdown |

---

## Part 2. Key ideas

**Tick.** One step of the game: 1/60th of a second. Gameplay only ever moves forward in whole ticks. "Frame" in fighting-game talk (startup frames, jump squat frames) means the same thing: one tick.

**Fixed-point numbers (`Fixed`).** Normal decimal numbers (`float`) can give very slightly different answers on different computers. That's fine for graphics, but online rollback needs both players' games to stay *exactly* identical. So every gameplay number is a `Fixed`: a decimal stored secretly as a whole number. 1.0 is stored as 65536, 0.5 as 32768, and so on. Whole-number math is the same on every computer.

**Deterministic.** Same starting state + same inputs = exactly the same result, every time, on every machine. The whole simulation is built to guarantee this, and the tests check it.

**State vs config.** *State* (`GameState`) is everything that changes during play: positions, speeds, current action, input history. It gets saved in snapshots. *Config* (`FighterConfig`, `AttackConfig`, `InputFeelConfig`) is the tuning values. It doesn't change during a match, so it isn't saved in snapshots.

**Data asset (ScriptableObject).** A file in the Project window that holds values you edit in the Inspector, like `BunCharacterData`. Each character gets their own. The game reads them when Play starts, and in the Editor it re-reads them every frame, so you can tune while playing.

**Rollback.** In online play, your opponent's input arrives a little late. Rollback guesses, keeps playing, and when the real input arrives it rewinds to a saved snapshot and quickly replays the ticks with the correct input. That only works if the game is deterministic and the full state can be copied. Milestone 1 builds the game that way from the start. F5/F9/F10 prove it works.

**Hitbox and hurtbox.** A hitbox is the area an attack can hit with. A hurtbox is the area a character can be hit in. Hits are decided only by these boxes, never by the 3D model's shape.

**Action.** What the fighter is doing right now: Idle, Walk, Dash, Run, RunTurnaround, Crouch, Crawl, JumpSquat, Airborne, LightAttack. Each action has its own rules, and `ActionTick` counts how many ticks it has lasted.

**Assemblies (`.asmdef` files).** Code is split into separate packages:
- `DreamsOfEverus.Simulation` has Unity switched **off**, so gameplay code physically can't use Unity physics, `Time.deltaTime` or GameObjects by accident.
- `DreamsOfEverus.Game` is the Unity side.
- `DreamsOfEverus.Editor` only exists inside the Unity Editor.
- `DreamsOfEverus.Tests.EditMode` holds the tests.

**`ref` and `in`.** You'll see `ref FighterState fighter` a lot. `FighterState` is a *struct* (a value, like a number), so passing it normally would hand the function a copy. `ref` passes the original so the function can change it. `in` passes the original read-only, which is faster than copying and promises not to change it.

---

## Part 3. Every file, line by line

The files are in the order data flows: numbers, then input, then the game state, then the rules, then the Unity side, then the editor tools.

### 3.1 `Scripts/Simulation/Math/Fixed.cs`

**What it is:** the exact-decimal number type used for every gameplay value.
**Used by:** everything in the simulation.

```csharp
using System;                                  // ◀ Basic C# tools (Math, IEquatable...)
using System.Globalization;                    // ◀ For printing numbers the same way in every language/region

namespace DreamsOfEverus.Simulation            // ◀ The "family name" for all gameplay code
{
    [Serializable]                             // ◀ Lets Unity save this type inside assets
    public struct Fixed : IEquatable<Fixed>, IComparable<Fixed>   // ◀ A struct: a small value, copied like a number. It can be compared and sorted.
    {
        public const int FractionalBits = 16;  // ◀ 16 of the 64 bits are used for the part after the decimal point
        public const long OneRaw = 1L << FractionalBits;   // ◀ 1 shifted left 16 places = 65536. This is how "1.0" is stored.

        public long Raw;                       // ◀ The hidden whole number. The real value is Raw / 65536.

        public static readonly Fixed Zero = new Fixed { Raw = 0 };          // ◀ Handy constant: 0
        public static readonly Fixed One = new Fixed { Raw = OneRaw };      // ◀ 1.0
        public static readonly Fixed Half = new Fixed { Raw = OneRaw / 2 }; // ◀ 0.5 (stored as 32768)

        public static Fixed FromRaw(long raw) => new Fixed { Raw = raw };   // ◀ Build from the hidden number directly

        public static Fixed FromInt(int value) => new Fixed { Raw = (long)value << FractionalBits };
                                               // ◀ Whole number to Fixed: shift left 16 places (same as × 65536). 3 becomes 196608.

        public static Fixed FromFraction(int numerator, int denominator)
        {
            return new Fixed { Raw = ((long)numerator << FractionalBits) / denominator };
                                               // ◀ Exact fraction: (3 × 65536) / 2 = 98304 = 1.5. Used mostly in tests.
        }

        public static Fixed FromFloat(float value)
        {
            return new Fixed { Raw = (long)Math.Round((double)value * OneRaw, MidpointRounding.AwayFromZero) };
                                               // ◀ Float to Fixed: multiply by 65536 and round to the nearest whole number.
                                               // ◀ ONLY used when loading values typed in the Inspector, never during a tick.
        }

        public float ToFloat() => (float)Raw / OneRaw;   // ◀ Back to a normal decimal, for drawing things on screen only

        public int FloorToInt() => (int)(Raw >> FractionalBits);   // ◀ Drop the decimal part (rounds down: -1.5 becomes -2)

        // ----- Operators -----                // ◀ These let you write a + b, a * b etc. with Fixed numbers

        public static Fixed operator +(Fixed a, Fixed b) => new Fixed { Raw = a.Raw + b.Raw };   // ◀ Adding: just add the hidden numbers
        public static Fixed operator -(Fixed a, Fixed b) => new Fixed { Raw = a.Raw - b.Raw };   // ◀ Subtracting: same idea
        public static Fixed operator -(Fixed a) => new Fixed { Raw = -a.Raw };                   // ◀ Negative: -a
        public static Fixed operator *(Fixed a, Fixed b) => new Fixed { Raw = (a.Raw * b.Raw) >> FractionalBits };
                                               // ◀ Multiplying: both numbers carry an extra ×65536, so the result has ×65536 twice.
                                               // ◀ Shifting right 16 places (÷65536) removes one of them.
        public static Fixed operator /(Fixed a, Fixed b) => new Fixed { Raw = (a.Raw << FractionalBits) / b.Raw };
                                               // ◀ Dividing: the ×65536 would cancel out, so add one back first (shift left) before dividing

        public static Fixed operator *(Fixed a, int b) => new Fixed { Raw = a.Raw * b };   // ◀ Multiply by a plain whole number (e.g. speed × facing)
        public static Fixed operator /(Fixed a, int b) => new Fixed { Raw = a.Raw / b };   // ◀ Divide by a plain whole number (e.g. ÷ 60 ticks)

        public static bool operator ==(Fixed a, Fixed b) => a.Raw == b.Raw;   // ◀ Comparisons just compare the hidden numbers
        public static bool operator !=(Fixed a, Fixed b) => a.Raw != b.Raw;
        public static bool operator <(Fixed a, Fixed b) => a.Raw < b.Raw;
        public static bool operator >(Fixed a, Fixed b) => a.Raw > b.Raw;
        public static bool operator <=(Fixed a, Fixed b) => a.Raw <= b.Raw;
        public static bool operator >=(Fixed a, Fixed b) => a.Raw >= b.Raw;

        // ----- Helpers -----

        public static Fixed Abs(Fixed a) => a.Raw < 0 ? -a : a;               // ◀ Absolute value: -3 becomes 3
        public static Fixed Min(Fixed a, Fixed b) => a.Raw < b.Raw ? a : b;   // ◀ The smaller of two
        public static Fixed Max(Fixed a, Fixed b) => a.Raw > b.Raw ? a : b;   // ◀ The larger of two
        public static Fixed Clamp(Fixed value, Fixed min, Fixed max) => Max(min, Min(max, value));
                                               // ◀ Keep a value between min and max (used to cap jump momentum)

        public static int Sign(Fixed a) => a.Raw > 0 ? 1 : (a.Raw < 0 ? -1 : 0);   // ◀ 1 if positive, -1 if negative, 0 if zero

        public static Fixed MoveTowards(Fixed current, Fixed target, Fixed maxStep)
        {                                      // ◀ THE most-used helper in movement: change a speed toward a goal by at most one step
            if (Abs(target - current) <= maxStep) return target;   // ◀ Close enough: land exactly on the target (never overshoot)
            return current + (target > current ? maxStep : -maxStep);   // ◀ Otherwise take one step up or down toward it
        }

        // ----- Equality and display -----

        public bool Equals(Fixed other) => Raw == other.Raw;                   // ◀ Two Fixed are equal if their hidden numbers match
        public override bool Equals(object obj) => obj is Fixed other && Equals(other);
        public override int GetHashCode() => Raw.GetHashCode();               // ◀ Needed by C# whenever Equals is customised
        public int CompareTo(Fixed other) => Raw.CompareTo(other.Raw);         // ◀ For sorting

        public override string ToString()
        {
            return ((double)Raw / OneRaw).ToString("0.#####", CultureInfo.InvariantCulture);
                                               // ◀ Prints e.g. "1.5" in the Console and test messages
        }
    }
}
```

**Good to know:** the smallest step is 1/65536 (about 0.000015 units), far smaller than anything you'd notice. Multiplying two values above about 30,000 would overflow, but nothing in a level gets anywhere near that.

### 3.2 `Scripts/Simulation/Math/FixedVector2.cs`

**What it is:** a pair of `Fixed` numbers (X and Y) for positions and speeds. X is right, Y is up.

```csharp
using System;

namespace DreamsOfEverus.Simulation
{
    [Serializable]
    public struct FixedVector2 : IEquatable<FixedVector2>   // ◀ A 2D point or arrow made of two Fixed numbers
    {
        public Fixed X;                        // ◀ Left/right. Bigger = further right.
        public Fixed Y;                        // ◀ Down/up. Bigger = higher.

        public static readonly FixedVector2 Zero = new FixedVector2(Fixed.Zero, Fixed.Zero);   // ◀ (0, 0)

        public FixedVector2(Fixed x, Fixed y)  // ◀ Constructor: new FixedVector2(x, y)
        {
            X = x;
            Y = y;
        }

        public static FixedVector2 operator +(FixedVector2 a, FixedVector2 b) => new FixedVector2(a.X + b.X, a.Y + b.Y);   // ◀ Add each part
        public static FixedVector2 operator -(FixedVector2 a, FixedVector2 b) => new FixedVector2(a.X - b.X, a.Y - b.Y);   // ◀ Subtract each part
        public static FixedVector2 operator -(FixedVector2 a) => new FixedVector2(-a.X, -a.Y);                           // ◀ Flip both
        public static FixedVector2 operator *(FixedVector2 a, Fixed s) => new FixedVector2(a.X * s, a.Y * s);            // ◀ Scale both

        public static bool operator ==(FixedVector2 a, FixedVector2 b) => a.X == b.X && a.Y == b.Y;   // ◀ Equal if both parts match
        public static bool operator !=(FixedVector2 a, FixedVector2 b) => !(a == b);

        public bool Equals(FixedVector2 other) => this == other;
        public override bool Equals(object obj) => obj is FixedVector2 other && Equals(other);
        public override int GetHashCode() => X.GetHashCode() * 397 ^ Y.GetHashCode();   // ◀ Standard way to combine two hash codes
        public override string ToString() => $"({X}, {Y})";                             // ◀ Prints "(1.5, 2)"
    }
}
```

### 3.3 `Scripts/Simulation/Core/InputFrame.cs`

**What it is:** one player's input for one tick, as small exact numbers. This is what would be sent over the network in online play.
**Made by:** `InputFrameBuilder` (Unity side). **Read by:** `FighterInput`.

```csharp
using System;

namespace DreamsOfEverus.Simulation
{
    [Flags]                                    // ◀ "Flags" means several of these can be on at once
    public enum InputButtons : byte            // ◀ Each button is one bit in a single byte
    {
        None = 0,                              // ◀ 00 in binary: nothing held
        Jump = 1 << 0,                         // ◀ 01: the Jump bit
        Attack = 1 << 1,                       // ◀ 10: the Attack bit. Both held = 11.
    }

    [Serializable]
    public struct InputFrame : IEquatable<InputFrame>
    {
        public const int StickMax = 100;       // ◀ Stick values run from -100 to 100

        public sbyte MoveX; // -100 = full left, 100 = full right    // ◀ sbyte = a tiny whole number (-128..127). Exact, no decimals.
        public sbyte MoveY; // -100 = full down, 100 = full up
        public InputButtons Held;              // ◀ Which buttons are held this tick

        public bool IsHeld(InputButtons button) => (Held & button) != 0;   // ◀ "&" picks out one bit: is that button's bit on?

        public bool Equals(InputFrame other) => MoveX == other.MoveX && MoveY == other.MoveY && Held == other.Held;
        public override bool Equals(object obj) => obj is InputFrame other && Equals(other);
        public override int GetHashCode() => (MoveX << 16) ^ (MoveY << 8) ^ (int)Held;
    }
}
```

### 3.4 `Scripts/Simulation/Core/GameState.cs`

**What it is:** *everything* that changes during play. Saving a copy of this is a snapshot. If something isn't in here, rollback can't restore it, so any new gameplay feature that changes over time must add its data here.

```csharp
namespace DreamsOfEverus.Simulation
{
    public enum GroundType : byte              // ◀ What the feet are on
    {
        None,        // In the air
        Solid,
        PassThrough,
    }

    public enum FighterAction : byte           // ◀ The list of things a fighter can be doing. One at a time.
    {
        Idle,          // Standing still (or sliding to a stop)
        Walk,          // Tilting the stick: speed follows how far it's tilted
        Dash,          // Started by a hard sideways press. Lasts for the character's trot; a hard press back dash dances
        Run,           // Holding the direction after the trot ends
        RunTurnaround, // Pressing back while running: the slower skid-and-turn
        JumpSquat,     // The few frames of crouch before leaving the ground
        Airborne,      // In the air (jumping, falling, or after walking off a ledge)
        Crouch,        // Holding Down on the ground: shorter body
        Crawl,         // Crouching and moving (only characters that can crawl)
        LightAttack,   // The Square attack: startup, active (hitbox out), recovery
    }

    public struct FighterState                 // ◀ Everything about one fighter that changes. A struct, so "a = b" copies all of it.
    {
        public FixedVector2 Position;     // Feet position (bottom-center of the body box), in world units
        public FixedVector2 Velocity;     // World units per tick          ◀ e.g. 0.15 = moving 0.15 units every tick (9 units/second)
        public GroundType Ground;         // What the feet are on right now
        public sbyte Facing;              // 1 = facing right, -1 = facing left   ◀ Multiplying by Facing flips things to the right side

        public FighterAction Action;      // What the fighter is doing
        public int ActionTick;            // How many ticks the current action has lasted   ◀ Reset to 0 on every action change

        public byte AirJumpsUsed;         // Reset on landing
        public bool IsFastFalling;        // Set by a hard press of Down while falling; cleared on landing or air jump
        public bool ShortHopQueued;       // Jump was released during jump squat, so this jump will be a short hop
        public bool AttackHasHit;         // The current attack already hit, so it can't hit again

        // Input history, used to spot hard presses. Part of the state so rollback replays them exactly.
        public InputFrame PreviousInput;  // Last tick's input                     ◀ Compared with this tick's to spot "just pressed"
        public byte TicksPushedLeft;      // How many ticks in a row the stick has been pushed left (stops at 255)
        public byte TicksPushedRight;     // ... pushed right
        public byte TicksPushedDown;      // ... pushed down                        ◀ These three decide "hard press" vs "tilt"

        public bool IsGrounded => Ground != GroundType.None;   // ◀ A shortcut: true when standing on anything
        public bool IsLow => Action == FighterAction.Crouch || Action == FighterAction.Crawl;   // ◀ True when the body is short
    }

    public struct DummyState                   // ◀ Everything about the training dummy that changes
    {
        public bool Exists;                    // ◀ False if the scene has no dummy
        public FixedVector2 Position;     // Feet position
        public int HitReactTicksLeft;     // Counts down after a hit; the dummy flashes and shakes while above 0
        public int TimesHit;              // Running total, shown on the debug readout
    }

    public sealed class GameState              // ◀ The full snapshot: one tick of the whole game
    {
        public int Tick;                       // ◀ How many ticks have run since Play started
        public FighterState Player;            // ◀ Bun
        public DummyState Dummy;               // ◀ The dummy

        public void CopyFrom(GameState other)  // ◀ Overwrite this snapshot with another one (used to save and restore)
        {
            Tick = other.Tick;
            Player = other.Player;             // ◀ Struct copy: every field inside FighterState is copied at once
            Dummy = other.Dummy;
        }

        public GameState Clone()               // ◀ Make a brand-new independent copy (a snapshot)
        {
            var copy = new GameState();
            copy.CopyFrom(this);
            return copy;
        }

        public bool SameAs(GameState other)    // ◀ Are two snapshots identical? Used by tests and the F10 rollback check.
        {
            return Tick == other.Tick && Player.Equals(other.Player) && Dummy.Equals(other.Dummy);
                                               // ◀ Struct Equals compares every field automatically, so new fields are included for free
        }
    }
}
```

**Rule to remember:** only put plain values in `FighterState` and `DummyState` (numbers, vectors, enums, bools). If you ever need a list or array in there, `CopyFrom` must be changed to copy it properly, or snapshots would share the same list.

### 3.5 `Scripts/Simulation/Core/FighterConfig.cs`

**What it is:** one character's tuning values converted to fixed-point and per-tick units, ready for the simulation. Built from `CharacterData` (see 3.17). Also holds `InputFeelConfig`, the shared input settings.

```csharp
namespace DreamsOfEverus.Simulation
{
    public sealed class FighterConfig          // ◀ A class (not a struct): one shared object the simulation keeps a reference to
    {
        // Collision box. Its bottom-center is the fighter's position (the feet).
        public Fixed BodyWidth;
        public Fixed BodyHeight;
        public Fixed HalfWidth => BodyWidth / 2;   // ◀ Calculated on demand. Used constantly, since boxes are measured from the center.

        // Ground
        public Fixed WalkSpeed;                // ◀ All speeds here are units PER TICK (the asset's units-per-second ÷ 60)
        public Fixed GroundAcceleration;       // ◀ Accelerations are units per tick per tick (÷ 3600)
        public Fixed Traction;              // How quickly speed is shed on the ground: stopping, skids, slides. Low = slippery
        public Fixed DashSpeed;
        public int TrotTicks;               // How long the initial dash lasts (from the trot distance)   ◀ Calculated from trot distance ÷ dash speed
        public Fixed RunSpeed;
        public int RunTurnaroundTicks;
        public Fixed MoonwalkAcceleration;  // How strongly a gentle back-tilt during the trot pushes backward. 0 = no moonwalk

        // Crouch and crawl
        public Fixed CrouchHeight;          // Body height while crouching or crawling
        public bool CanCrawl;
        public Fixed CrawlSpeed;

        // Jumping
        public int JumpSquatTicks;
        public Fixed FullHopVelocity;          // ◀ Upward launch speed, calculated from the jump HEIGHT you type in the asset
        public Fixed ShortHopVelocity;
        public Fixed AirJumpVelocity;
        public int AirJumps;
        public Fixed MaxJumpMomentum;       // Most sideways speed carried from the ground into a jump

        // Air
        public Fixed Gravity;
        public Fixed MaxFallSpeed;
        public Fixed FastFallSpeed;
        public Fixed AirSpeed;
        public Fixed AirAcceleration;
        public Fixed AirFriction;

        // Attacks (null = this character has no light attack yet)
        public AttackConfig LightAttack;       // ◀ See 3.13. "null" means nothing assigned.

        public Fixed HeightFor(in FighterState fighter) => fighter.IsLow ? CrouchHeight : BodyHeight;
                                               // ◀ The body's height right now: short while crouching/crawling, full otherwise.
                                               // ◀ Collision, the fall zone and the capsule's size all use this.
    }

    public sealed class InputFeelConfig        // ◀ How input is read, shared by every character. Built from InputFeelData (3.18).
    {
        public int StickDeadzone;        // Below this, the stick counts as centered
        public int PressThreshold;       // How far the stick must go to count as a press
        public int HardPressWindowTicks; // A press counts as "hard" if it gets there within this many ticks of starting to push
        public int CrouchThreshold;      // How far down the stick must be held to crouch
    }
}
```

### 3.6 `Scripts/Simulation/Core/GameSimulation.cs`

**What it is:** the conductor. `Step` runs one tick by calling every system in the right order. This is the best file to read first if you want to follow the game's logic.

```csharp
namespace DreamsOfEverus.Simulation
{
    public sealed class GameSimulation
    {
        public const int TicksPerSecond = 60;  // ◀ The game's heartbeat. Used everywhere that converts seconds to ticks.

        private readonly LevelGeometry level;              // ◀ The level's boxes (never changes during play)
        private readonly FighterConfig playerConfig;       // ◀ Bun's tuning values
        private readonly InputFeelConfig inputFeel;        // ◀ How input is read
        private readonly TrainingDummyConfig dummyConfig;  // ◀ The dummy's setup (or null if there's no dummy)
                                                           // ◀ "readonly" = set once in the constructor, never replaced

        public GameSimulation(LevelGeometry level, FighterConfig playerConfig, InputFeelConfig inputFeel, TrainingDummyConfig dummyConfig = null)
        {                                      // ◀ "= null" makes the dummy optional (tests often leave it out)
            this.level = level;
            this.playerConfig = playerConfig;  // ◀ Kept by reference: when live tuning changes its values, the next tick uses them
            this.inputFeel = inputFeel;
            this.dummyConfig = dummyConfig;
        }

        public GameState CreateInitialState()  // ◀ The very first snapshot when Play starts
        {
            var state = new GameState();
            state.Player.Facing = 1;           // ◀ Face right
            PlaceAtRespawn(ref state.Player);  // ◀ Stand on the respawn point

            if (dummyConfig != null)           // ◀ Put the dummy in place, if there is one
            {
                state.Dummy.Exists = true;
                state.Dummy.Position = dummyConfig.Position;
            }
            return state;
        }

        public void Step(GameState state, InputFrame input)   // ◀ ONE TICK. Same state + same input = same result, always.
        {
            ref FighterState player = ref state.Player;       // ◀ "ref" = work directly on Bun's data, not a copy

            InputEvents events = FighterInput.Read(ref player, input, inputFeel);        // ◀ 1. What does the input mean? (3.10)
            FighterMovement.UpdateBeforeMove(ref player, events, playerConfig, level);   // ◀ 2. Decide action and velocity (3.11)
            KinematicMover.Move(ref player, playerConfig, level);                        // ◀ 3. Move and collide (3.9)
            CombatSystem.Step(ref player, playerConfig, ref state.Dummy, dummyConfig);   // ◀ 4. Hitbox vs dummy (3.14)
            FighterMovement.UpdateAfterMove(ref player);                                 // ◀ 5. Landed? Walked off a ledge?

            if (level.TouchesFallZone(KinematicMover.BodyBox(player, playerConfig)))     // ◀ 6. In the red zone?
            {
                PlaceAtRespawn(ref player);    // ◀ Back to the respawn point
            }

            player.PreviousInput = input;      // ◀ 7. Remember this input so next tick can spot new presses
            state.Tick++;                      // ◀ Count the tick
        }

        private void PlaceAtRespawn(ref FighterState fighter)
        {
            fighter.Position = level.RespawnPoint;                         // ◀ Teleport the feet there
            KinematicMover.UpdateGround(ref fighter, playerConfig, level); // ◀ Work out if that spot is on the ground
            FighterMovement.ResetAfterTeleport(ref fighter);               // ◀ Stop moving, refill air jumps, pick Idle or Airborne
        }
    }
}
```

**Why the order matters:** combat runs *after* movement, so the hitbox is checked where Bun actually is this tick. It also runs *before* `UpdateAfterMove`, which adds 1 to `ActionTick`, so "frame 0" of an attack really is the first tick.

### 3.7 `Scripts/Simulation/Core/SnapshotReplay.cs`

**What it is:** rewind-and-replay, the heart of rollback, in nine lines. Used by the F10 check and by tests.

```csharp
using System.Collections.Generic;              // ◀ For lists

namespace DreamsOfEverus.Simulation
{
    public static class SnapshotReplay         // ◀ "static" = a toolbox of functions, not something you create
    {
        public static GameState Replay(GameSimulation simulation, GameState snapshot, IReadOnlyList<InputFrame> inputs)
        {
            GameState state = snapshot.Clone();   // ◀ Work on a copy so the saved snapshot stays untouched
            foreach (InputFrame input in inputs)  // ◀ For every recorded input, in order...
            {
                simulation.Step(state, input);    // ◀ ...run one tick, exactly like the live game did
            }
            return state;                         // ◀ If the game is deterministic, this equals the live state exactly
        }
    }
}
```

### 3.8 `Scripts/Simulation/Collision/FixedBox.cs`

**What it is:** a rectangle, stored as its bottom-left corner (`Min`) and top-right corner (`Max`). Bodies, level pieces, hitboxes and hurtboxes are all `FixedBox`es.

```csharp
using System;

namespace DreamsOfEverus.Simulation
{
    [Serializable]
    public struct FixedBox : IEquatable<FixedBox>
    {
        public FixedVector2 Min;               // ◀ Bottom-left corner
        public FixedVector2 Max;               // ◀ Top-right corner

        public FixedBox(FixedVector2 min, FixedVector2 max)
        {
            Min = min;
            Max = max;
        }

        public bool Overlaps(FixedBox other)   // ◀ Do two boxes share any area?
        {
            return Min.X < other.Max.X && Max.X > other.Min.X     // ◀ They overlap left-to-right...
                && Min.Y < other.Max.Y && Max.Y > other.Min.Y;    // ◀ ...AND bottom-to-top.
                                               // ◀ "<" not "<=": boxes that only touch at an edge do NOT overlap.
                                               // ◀ That's why standing exactly on a floor isn't counted as "inside" it.
        }

        public bool Equals(FixedBox other) => Min == other.Min && Max == other.Max;
        public override bool Equals(object obj) => obj is FixedBox other && Equals(other);
        public override int GetHashCode() => Min.GetHashCode() * 397 ^ Max.GetHashCode();
        public override string ToString() => $"[{Min} .. {Max}]";
    }
}
```

### 3.9 `Scripts/Simulation/Collision/KinematicMover.cs`

**What it is:** the custom collision system, used instead of Unity physics. It moves a fighter by their velocity and stops them at walls, floors and ceilings. It also handles pass-through platforms, dropping through, "what am I standing on?" and "is there room to stand up?".

**The key trick:** it moves sideways first, then vertically. For each direction it checks the whole path from the old position to the new one, not just the end point. That way even a very fast fall can't skip through a thin platform.

```csharp
namespace DreamsOfEverus.Simulation
{
    public static class KinematicMover
    {
        private static readonly Fixed DropThroughNudge = Fixed.FromFraction(1, 256);
                                               // ◀ 1/256 of a unit: too small to see, but enough to put the feet "below" a platform's top

        public static FixedBox BodyBox(in FighterState fighter, FighterConfig config)
        {                                      // ◀ The fighter's body box right now (shorter while crouching)
            return BoxAt(fighter.Position, config.HalfWidth, config.HeightFor(fighter));
        }

        public static bool HasRoomToStand(in FighterState fighter, FighterConfig config, LevelGeometry level)
        {                                      // ◀ Used when crouching under a low ceiling
            FixedBox standing = BoxAt(fighter.Position, config.HalfWidth, config.BodyHeight);   // ◀ Imagine the full-height body here
            foreach (LevelBlock block in level.Blocks)   // ◀ Check every level piece...
            {
                if (block.Kind == LevelBlockKind.Solid && block.Box.Overlaps(standing)) return false;   // ◀ ...a solid one is in the way
            }
            return true;                       // ◀ Nothing in the way: free to stand
        }

        private static FixedBox BoxAt(FixedVector2 feet, Fixed halfWidth, Fixed height)
        {                                      // ◀ Builds a body box from the feet: centered left/right, going UP from the feet
            return new FixedBox(
                new FixedVector2(feet.X - halfWidth, feet.Y),             // ◀ bottom-left
                new FixedVector2(feet.X + halfWidth, feet.Y + height));   // ◀ top-right
        }

        public static void Move(ref FighterState fighter, FighterConfig config, LevelGeometry level)
        {                                      // ◀ Called once per tick by GameSimulation.Step
            MoveHorizontal(ref fighter, config, level);   // ◀ Sideways first
            MoveVertical(ref fighter, config, level);     // ◀ Then up/down
            UpdateGround(ref fighter, config, level);     // ◀ Then work out what's under the feet
        }

        public static bool TryDropThrough(ref FighterState fighter)
        {
            if (fighter.Ground != GroundType.PassThrough) return false;   // ◀ Only from a pass-through platform. Solid ground: no.

            // Putting the feet just under the top means the platform no longer counts as "below" them,
            // so it won't catch the fighter while falling.
            fighter.Position.Y -= DropThroughNudge;   // ◀ Sink by 1/256 of a unit
            fighter.Ground = GroundType.None;         // ◀ Now in the air; gravity takes over next tick
            return true;
        }

        public static void UpdateGround(ref FighterState fighter, FighterConfig config, LevelGeometry level)
        {                                      // ◀ "What am I standing on?"
            Fixed left = fighter.Position.X - config.HalfWidth;    // ◀ Body's left edge
            Fixed right = fighter.Position.X + config.HalfWidth;   // ◀ Body's right edge
            fighter.Ground = GroundType.None;  // ◀ Assume nothing, then look

            foreach (LevelBlock block in level.Blocks)
            {
                if (block.Kind == LevelBlockKind.FallZone) continue;          // ◀ You can't stand on a fall zone; skip it
                if (block.Box.Max.Y != fighter.Position.Y) continue;          // ◀ Its top must be EXACTLY at the feet
                if (!(left < block.Box.Max.X && right > block.Box.Min.X)) continue;   // ◀ ...and underneath the body left-to-right

                // Solid ground wins: you can't drop through if one foot is on something solid.
                if (block.Kind == LevelBlockKind.Solid) fighter.Ground = GroundType.Solid;
                else if (fighter.Ground == GroundType.None) fighter.Ground = GroundType.PassThrough;
            }
        }

        private static void MoveHorizontal(ref FighterState fighter, FighterConfig config, LevelGeometry level)
        {
            Fixed velocityX = fighter.Velocity.X;
            if (velocityX == Fixed.Zero) return;   // ◀ Not moving sideways: nothing to do

            Fixed half = config.HalfWidth;
            Fixed bottom = fighter.Position.Y;                     // ◀ Body's bottom (the feet)
            Fixed top = bottom + config.HeightFor(fighter);        // ◀ Body's top (lower while crouching, so you fit in tunnels)
            Fixed oldX = fighter.Position.X;                       // ◀ Where we start
            Fixed targetX = oldX + velocityX;                      // ◀ Where we'd end up with nothing in the way
            bool blocked = false;

            foreach (LevelBlock block in level.Blocks)
            {
                if (block.Kind != LevelBlockKind.Solid) continue; // only solids block sideways   ◀ Pass-through platforms never stop you sideways
                FixedBox box = block.Box;
                if (!(bottom < box.Max.Y && top > box.Min.Y)) continue; // not at the same height   ◀ The wall must overlap the body vertically

                if (velocityX > Fixed.Zero)    // ◀ Moving right
                {
                    // Right edge was left of the wall, and would end up past it.
                    if (oldX + half <= box.Min.X && targetX + half > box.Min.X)
                    {
                        targetX = box.Min.X - half;   // ◀ Stop with the right edge touching the wall
                        blocked = true;
                    }
                }
                else if (oldX - half >= box.Max.X && targetX - half < box.Max.X)   // ◀ Moving left: same idea, mirrored
                {
                    targetX = box.Max.X + half;
                    blocked = true;
                }
            }
                                               // ◀ If several walls are in the way, each one can only pull targetX CLOSER,
                                               // ◀ so the nearest wall always wins, no matter what order they're checked in.
            fighter.Position.X = targetX;      // ◀ Move
            if (blocked) fighter.Velocity.X = Fixed.Zero;   // ◀ Hit a wall: sideways speed stops
        }

        private static void MoveVertical(ref FighterState fighter, FighterConfig config, LevelGeometry level)
        {
            Fixed velocityY = fighter.Velocity.Y;
            if (velocityY == Fixed.Zero) return;

            Fixed left = fighter.Position.X - config.HalfWidth;    // ◀ Uses the NEW sideways position, since that move already happened
            Fixed right = fighter.Position.X + config.HalfWidth;
            Fixed oldY = fighter.Position.Y;
            Fixed targetY = oldY + velocityY;
            Fixed height = config.HeightFor(fighter);
            bool blocked = false;

            foreach (LevelBlock block in level.Blocks)
            {
                if (block.Kind == LevelBlockKind.FallZone) continue;   // ◀ Fall zones don't block; they're handled in GameSimulation
                FixedBox box = block.Box;
                if (!(left < box.Max.X && right > box.Min.X)) continue; // not above or below this box

                if (velocityY < Fixed.Zero)    // ◀ Falling
                {
                    // Falling: land on top of solids AND pass-through platforms,
                    // but only if the feet started at or above the top.
                    if (oldY >= box.Max.Y && targetY < box.Max.Y)
                    {                          // ◀ "Started above" is what makes pass-through work: jumping up through a platform,
                                               // ◀ the feet start BELOW its top, so it doesn't catch you on the way back down mid-jump.
                        targetY = box.Max.Y;   // ◀ Land exactly on top
                        blocked = true;
                    }
                }
                else if (block.Kind == LevelBlockKind.Solid)   // ◀ Rising: only solid boxes matter
                {
                    // Rising: only solids stop the head. Pass-through platforms let you jump up through.
                    Fixed oldTop = oldY + height;                          // ◀ Where the head was
                    if (oldTop <= box.Min.Y && targetY + height > box.Min.Y)   // ◀ Head was below the ceiling and would go past it
                    {
                        targetY = box.Min.Y - height;   // ◀ Stop with the head touching the ceiling
                        blocked = true;
                    }
                }
            }

            fighter.Position.Y = targetY;
            if (blocked) fighter.Velocity.Y = Fixed.Zero;   // ◀ Landed or bonked: vertical speed stops
        }
    }
}
```

### 3.10 `Scripts/Simulation/Fighter/FighterInput.cs`

**What it is:** turns a raw `InputFrame` into meaningful events like "Jump was just pressed" or "that was a hard press left". It also keeps the input history (in `FighterState`) needed to tell a hard press from a gentle tilt.

**How a hard press works:** the stick must reach the press threshold (80%) and must have started pushing that way only a moment ago (fewer than 2 ticks). If you ease the stick over slowly, it spends too many ticks between "not pushed" and 80%, so it's just a tilt. This one rule drives dash, dash dance, fast-fall and drop-through, and it's why a rolled stick (the moonwalk input) doesn't count as a hard press.

```csharp
namespace DreamsOfEverus.Simulation
{
    public struct InputEvents                  // ◀ The "meaning" of this tick's input. Made fresh every tick.
    {
        public int StickX;            // -100..100
        public int HeldDirectionX;    // -1, 0 or 1: which way the stick is held (outside the deadzone)
        public int HardPressX;        // -1, 0 or 1: a fast, hard sideways press this tick (dash, dash dance)
        public bool HardPressDown;    // A fast, hard press of Down this tick (drop-through, fast-fall)
        public bool HeldDown;         // Stick held down far enough to crouch
        public bool JumpHeld;
        public bool JumpPressed;      // Jump went down this tick
        public bool AttackPressed;    // Attack went down this tick
    }

    public static class FighterInput
    {
        public static InputEvents Read(ref FighterState fighter, InputFrame input, InputFeelConfig feel)
        {
            InputFrame previous = fighter.PreviousInput;   // ◀ Last tick's input, saved in the fighter's state
            var events = new InputEvents
            {
                StickX = input.MoveX,
                HeldDirectionX = input.MoveX > feel.StickDeadzone ? 1 : (input.MoveX < -feel.StickDeadzone ? -1 : 0),
                                               // ◀ Past the deadzone to the right = 1, to the left = -1, otherwise 0
                HeldDown = input.MoveY <= -feel.CrouchThreshold,   // ◀ Down far enough to crouch (50% by default)
                JumpHeld = input.IsHeld(InputButtons.Jump),
            };
            events.JumpPressed = events.JumpHeld && !previous.IsHeld(InputButtons.Jump);
                                               // ◀ "Pressed" = held now but NOT held last tick. Holding it doesn't repeat.
            events.AttackPressed = input.IsHeld(InputButtons.Attack) && !previous.IsHeld(InputButtons.Attack);

            // Each check uses how long the stick had been pushed that way BEFORE this tick.
            if (IsHardPress(input.MoveX, previous.MoveX, fighter.TicksPushedRight, feel)) events.HardPressX = 1;
            else if (IsHardPress(-input.MoveX, -previous.MoveX, fighter.TicksPushedLeft, feel)) events.HardPressX = -1;
                                               // ◀ For "left", the values are flipped (-MoveX) so the same function works both ways
            events.HardPressDown = IsHardPress(-input.MoveY, -previous.MoveY, fighter.TicksPushedDown, feel);

            fighter.TicksPushedRight = CountPush(fighter.TicksPushedRight, input.MoveX, feel);   // ◀ Update the history for next tick
            fighter.TicksPushedLeft = CountPush(fighter.TicksPushedLeft, -input.MoveX, feel);
            fighter.TicksPushedDown = CountPush(fighter.TicksPushedDown, -input.MoveY, feel);

            return events;
        }

        private static bool IsHardPress(int amount, int previousAmount, byte ticksPushedBefore, InputFeelConfig feel)
        {
            return amount >= feel.PressThreshold          // far enough now
                && previousAmount < feel.PressThreshold   // and it only just got there
                && ticksPushedBefore < feel.HardPressWindowTicks; // and it got there quickly
        }

        // Counts ticks in a row the stick is pushed this way; back to 0 as soon as it isn't.
        private static byte CountPush(byte ticks, int amount, InputFeelConfig feel)
        {
            if (amount <= feel.StickDeadzone) return 0;    // ◀ Not pushed this way (inside the deadzone or the other way): reset
            return ticks < byte.MaxValue ? (byte)(ticks + 1) : ticks;   // ◀ Otherwise count up, stopping at 255 so it can't wrap round
        }
    }
}
```

**Example:** flicking right in one tick goes 0 → 100. Before that tick, `TicksPushedRight` was 0, which is less than 2, so it's a hard press. Easing right goes 30 → 50 → 70 → 90. By the time it passes 80, the stick has been pushed right for 3 ticks, so it's not hard.

### 3.11 `Scripts/Simulation/Fighter/FighterMovement.cs`

**What it is:** every movement rule. The fighter is always doing one `FighterAction`, and each action has its own update function. This is the file you'll change most when tuning how movement *works*, as opposed to tuning numbers, which happens in the asset.

**How actions change (simplified):**

```
            hard press ─▶ DASH (trot) ──trot ends, holding forward──▶ RUN ──hold back──▶ RUN TURNAROUND ──▶ RUN or IDLE
                │          │  ▲  hard press back = dash dance                │
  IDLE ◀──▶ WALK           │  └──┘  gentle tilt back = moonwalk             └── let go ──▶ IDLE
   │  hold Down            └── trot ends, not holding forward ──▶ IDLE
   ▼
 CROUCH ◀──▶ CRAWL (if the character can crawl)

 Any ground action (not mid-attack) ── Jump ──▶ JUMP SQUAT ──▶ AIRBORNE ── land ──▶ IDLE
 Idle / Walk / Dash / Run ── Attack ──▶ LIGHT ATTACK ── all frames done ──▶ IDLE
```

```csharp
namespace DreamsOfEverus.Simulation
{
    public static class FighterMovement
    {
        public static void UpdateBeforeMove(ref FighterState fighter, in InputEvents input, FighterConfig config, LevelGeometry level)
        {                                      // ◀ Step 2 of a tick: decide the action and velocity
            if (fighter.Action == FighterAction.Airborne) UpdateAir(ref fighter, input, config);
            else UpdateGround(ref fighter, input, config, level);
        }

        public static void UpdateAfterMove(ref FighterState fighter)
        {                                      // ◀ Step 5 of a tick, after collision has moved the fighter
            fighter.ActionTick++;              // ◀ The current action has lasted one more tick

            bool inAir = fighter.Action == FighterAction.Airborne;
            if (inAir && fighter.IsGrounded && fighter.Velocity.Y <= Fixed.Zero)   // ◀ Was in the air, now has ground under the feet, not rising
            {
                Land(ref fighter);
            }
            else if (!inAir && !fighter.IsGrounded)   // ◀ Was on the ground, now isn't: walked, dashed or slid off a ledge
            {
                SetAction(ref fighter, FighterAction.Airborne); // walked or slid off a ledge
            }
        }

        public static void ResetAfterTeleport(ref FighterState fighter)
        {                                      // ◀ After placing the fighter somewhere new (start of play, respawn)
            fighter.Velocity = FixedVector2.Zero;
            fighter.AirJumpsUsed = 0;
            fighter.IsFastFalling = false;
            SetAction(ref fighter, fighter.IsGrounded ? FighterAction.Idle : FighterAction.Airborne);
        }

        // ----- Ground -----

        private static void UpdateGround(ref FighterState fighter, in InputEvents input, FighterConfig config, LevelGeometry level)
        {
            fighter.Velocity.Y = Fixed.Zero;   // ◀ On the ground: no up/down speed

            // Under a low ceiling the fighter has to stay crouched: no standing up, no jumping.
            bool roomToStand = KinematicMover.HasRoomToStand(fighter, config, level);

            // ◀ First, things that can interrupt the current action. Checked in priority order: jump, attack, drop, crouch.
            bool trotting = fighter.Action == FighterAction.Dash;
            bool committed = fighter.Action == FighterAction.JumpSquat || fighter.Action == FighterAction.LightAttack;
                                               // ◀ "Committed": once these start, nothing can interrupt them
            if (!committed)
            {
                if (input.JumpPressed && roomToStand)
                {
                    SetAction(ref fighter, FighterAction.JumpSquat);   // ◀ Start the crouch before a jump
                    fighter.ShortHopQueued = false;                    // ◀ Full jump unless Jump is released during the squat
                }
                else if (input.AttackPressed && CanStartLightAttack(fighter, config))
                {
                    SetAction(ref fighter, FighterAction.LightAttack);
                    fighter.AttackHasHit = false;  // ◀ A fresh attack hasn't hit anything yet
                }
                else if (!trotting && input.HardPressDown && KinematicMover.TryDropThrough(ref fighter))
                {                              // ◀ Hard Down on a pass-through platform (not mid-dash): drop through
                    SetAction(ref fighter, FighterAction.Airborne);
                    return;                    // ◀ Nothing else to do on the ground this tick
                }
                else if (!trotting && input.HeldDown && !fighter.IsLow)
                {                              // ◀ Holding Down (not mid-dash, where Down is part of the moonwalk roll): crouch
                    SetAction(ref fighter, FighterAction.Crouch);
                }
            }

            switch (fighter.Action)            // ◀ Now run the rules for whatever action we're in
            {
                case FighterAction.Idle:
                case FighterAction.Walk:
                    UpdateIdleOrWalk(ref fighter, input, config);
                    break;
                case FighterAction.Dash:
                    UpdateDash(ref fighter, input, config);
                    break;
                case FighterAction.Run:
                    UpdateRun(ref fighter, input, config);
                    break;
                case FighterAction.RunTurnaround:
                    UpdateRunTurnaround(ref fighter, input, config);
                    break;
                case FighterAction.Crouch:
                case FighterAction.Crawl:
                    UpdateCrouchOrCrawl(ref fighter, input, config, mustStayLow: !roomToStand);
                    break;
                case FighterAction.JumpSquat:
                    UpdateJumpSquat(ref fighter, input, config);
                    break;
                case FighterAction.LightAttack:
                    UpdateLightAttack(ref fighter, input, config);
                    break;
            }
        }

        // PLACEHOLDER rule: the light attack works from standing, walking, dashing and running.
        // Crouching, crawling and turning around are left out (those would be different moves later).
        private static bool CanStartLightAttack(in FighterState fighter, FighterConfig config)
        {
            if (config.LightAttack == null) return false;   // ◀ No attack asset assigned to this character
            switch (fighter.Action)
            {
                case FighterAction.Idle:
                case FighterAction.Walk:
                case FighterAction.Dash:
                case FighterAction.Run:
                    return true;
                default:
                    return false;
            }
        }

        private static void UpdateLightAttack(ref FighterState fighter, in InputEvents input, FighterConfig config)
        {
            // The attack has run its full startup, active and recovery frames: back to normal movement.
            if (fighter.ActionTick >= config.LightAttack.TotalTicks)
            {
                SetAction(ref fighter, FighterAction.Idle);
                UpdateIdleOrWalk(ref fighter, input, config);   // ◀ Act on this tick's input straight away (no wasted tick)
                return;
            }

            // No steering while attacking: any speed from walking or dashing slides off on traction.
            fighter.Velocity.X = Fixed.MoveTowards(fighter.Velocity.X, Fixed.Zero, config.Traction);
                                               // ◀ The hitbox itself is handled by CombatSystem (3.14), not here
        }

        private static void UpdateIdleOrWalk(ref FighterState fighter, in InputEvents input, FighterConfig config)
        {
            // A hard sideways press dashes (like a smash input). A gentle tilt walks.
            if (input.HardPressX != 0)
            {
                StartDash(ref fighter, input.HardPressX, config);
                return;
            }

            if (input.HeldDirectionX == 0)     // ◀ Stick centered
            {
                if (fighter.Action != FighterAction.Idle) SetAction(ref fighter, FighterAction.Idle);
                fighter.Velocity.X = Fixed.MoveTowards(fighter.Velocity.X, Fixed.Zero, config.Traction);   // ◀ Slide to a stop on traction
                return;
            }

            if (fighter.Action != FighterAction.Walk) SetAction(ref fighter, FighterAction.Walk);
            fighter.Facing = (sbyte)input.HeldDirectionX;   // ◀ Walking turns you instantly

            // Walk speed follows how far the stick is tilted: half tilt = half speed.
            Fixed targetSpeed = config.WalkSpeed * input.StickX / InputFrame.StickMax;   // ◀ e.g. stick 50 → 50/100 of walk speed
            fighter.Velocity.X = GroundSteer(fighter.Velocity.X, targetSpeed, input.HeldDirectionX, config);
        }

        private static void StartDash(ref FighterState fighter, int direction, FighterConfig config)
        {
            fighter.Facing = (sbyte)direction;
            SetAction(ref fighter, FighterAction.Dash);    // ◀ ActionTick resets to 0: the trot starts now
            fighter.Velocity.X = config.DashSpeed * direction;   // ◀ Instant burst to full dash speed
        }

        private static void UpdateDash(ref FighterState fighter, in InputEvents input, FighterConfig config)
        {
            // Dash dance: a hard press the other way during the trot instantly dashes that way.
            if (input.HardPressX == -fighter.Facing)
            {
                StartDash(ref fighter, input.HardPressX, config);   // ◀ A brand-new dash the other way (the trot restarts)
                return;
            }

            // The trot is over: keep holding forward to run, otherwise stop.
            if (fighter.ActionTick >= config.TrotTicks)
            {
                if (input.HeldDirectionX == fighter.Facing)
                {
                    SetAction(ref fighter, FighterAction.Run);
                    UpdateRun(ref fighter, input, config);
                }
                else
                {
                    SetAction(ref fighter, FighterAction.Idle);
                    fighter.Velocity.X = Fixed.MoveTowards(fighter.Velocity.X, Fixed.Zero, config.Traction);
                }
                return;
            }

            // ◀ Still in the trot. What the stick does decides the speed:
            if (input.HeldDirectionX == fighter.Facing)
            {
                // Holding forward: back up to full dash speed. Not instant, so steering back to forward
                // after a moonwalk slides around a little instead of snapping.
                fighter.Velocity.X = Fixed.MoveTowards(fighter.Velocity.X, config.DashSpeed * fighter.Facing, config.GroundAcceleration);
            }
            else if (input.HeldDirectionX == -fighter.Facing)
            {
                // Moonwalk: a gentle tilt back (not a hard press, which would dash dance) pushes the character
                // backward while they keep facing forward in the dash. How strongly depends on the character.
                Fixed backwardTarget = config.DashSpeed * input.StickX / InputFrame.StickMax;   // ◀ Further back on the stick = faster backward
                fighter.Velocity.X = Fixed.MoveTowards(fighter.Velocity.X, backwardTarget, config.MoonwalkAcceleration);
                                               // ◀ Facing is NOT changed, which is what makes it a moonwalk
            }
            else
            {
                // Stick let go during the trot: slide to a stop on the character's traction.
                fighter.Velocity.X = Fixed.MoveTowards(fighter.Velocity.X, Fixed.Zero, config.Traction);
                                               // ◀ This is also what happens while the stick passes through Down during the roll
            }
        }

        private static void UpdateRun(ref FighterState fighter, in InputEvents input, FighterConfig config)
        {
            // Past the trot, turning around is the slower skid-and-turn, not a dash dance.
            if (input.HeldDirectionX == -fighter.Facing)
            {
                SetAction(ref fighter, FighterAction.RunTurnaround);
                UpdateRunTurnaround(ref fighter, input, config);
                return;
            }

            if (input.HeldDirectionX == 0)     // ◀ Let go: stop running, slide to a halt
            {
                SetAction(ref fighter, FighterAction.Idle);
                fighter.Velocity.X = Fixed.MoveTowards(fighter.Velocity.X, Fixed.Zero, config.Traction);
                return;
            }

            fighter.Velocity.X = GroundSteer(fighter.Velocity.X, config.RunSpeed * fighter.Facing, fighter.Facing, config);
                                               // ◀ Settle toward run speed (dash speed is usually higher, so this eases down to it)
        }

        private static void UpdateRunTurnaround(ref FighterState fighter, in InputEvents input, FighterConfig config)
        {
            fighter.Velocity.X = Fixed.MoveTowards(fighter.Velocity.X, Fixed.Zero, config.Traction);   // ◀ Skid on traction

            if (fighter.ActionTick >= config.RunTurnaroundTicks)   // ◀ The turnaround's frames are done
            {
                fighter.Facing = (sbyte)-fighter.Facing;           // ◀ Only NOW does the fighter face the other way
                SetAction(ref fighter, input.HeldDirectionX == fighter.Facing ? FighterAction.Run : FighterAction.Idle);
            }
        }

        private static void UpdateCrouchOrCrawl(ref FighterState fighter, in InputEvents input, FighterConfig config, bool mustStayLow)
        {
            // Let go of Down to stand up, unless there's no room above.
            if (!input.HeldDown && !mustStayLow)
            {
                SetAction(ref fighter, FighterAction.Idle);
                UpdateIdleOrWalk(ref fighter, input, config);
                return;
            }

            if (config.CanCrawl && input.HeldDirectionX != 0)   // ◀ Holding sideways too, and this character can crawl
            {
                if (fighter.Action != FighterAction.Crawl) SetAction(ref fighter, FighterAction.Crawl);
                fighter.Facing = (sbyte)input.HeldDirectionX;

                Fixed targetSpeed = config.CrawlSpeed * input.StickX / InputFrame.StickMax;
                fighter.Velocity.X = GroundSteer(fighter.Velocity.X, targetSpeed, input.HeldDirectionX, config);
            }
            else                               // ◀ Just crouching (or can't crawl): stay put
            {
                if (fighter.Action != FighterAction.Crouch) SetAction(ref fighter, FighterAction.Crouch);
                fighter.Velocity.X = Fixed.MoveTowards(fighter.Velocity.X, Fixed.Zero, config.Traction);
            }
        }

        private static void UpdateJumpSquat(ref FighterState fighter, in InputEvents input, FighterConfig config)
        {
            // Smash-style short hop: let go of Jump before the squat ends.
            if (!input.JumpHeld) fighter.ShortHopQueued = true;   // ◀ Once released, it stays a short hop even if pressed again

            if (fighter.ActionTick >= config.JumpSquatTicks)       // ◀ Squat frames are done: leave the ground
            {
                fighter.Velocity.Y = fighter.ShortHopQueued ? config.ShortHopVelocity : config.FullHopVelocity;

                // Momentum: a dash or run carries its speed into the jump (up to the character's limit).
                fighter.Velocity.X = Fixed.Clamp(fighter.Velocity.X, -config.MaxJumpMomentum, config.MaxJumpMomentum);

                fighter.Ground = GroundType.None;
                SetAction(ref fighter, FighterAction.Airborne);
            }
        }

        // ----- Air -----

        private static void UpdateAir(ref FighterState fighter, in InputEvents input, FighterConfig config)
        {
            if (input.JumpPressed && fighter.AirJumpsUsed < config.AirJumps)   // ◀ Air jump, if any are left
            {
                fighter.AirJumpsUsed++;
                fighter.IsFastFalling = false;     // ◀ An air jump cancels fast-fall
                fighter.Velocity.Y = config.AirJumpVelocity;   // ◀ Upward speed is SET (not added), so it's the same every time
            }
            else
            {
                // Fast-fall: a hard press of Down once already falling (at or past the top of the jump).
                if (!fighter.IsFastFalling && input.HardPressDown && fighter.Velocity.Y <= Fixed.Zero)
                {
                    fighter.IsFastFalling = true;
                }

                fighter.Velocity.Y = fighter.IsFastFalling
                    ? -config.FastFallSpeed                                          // ◀ Fast-falling: straight to fast-fall speed
                    : Fixed.Max(fighter.Velocity.Y - config.Gravity, -config.MaxFallSpeed);   // ◀ Otherwise gravity, capped at max fall speed
            }

            UpdateAirControl(ref fighter, input, config);   // ◀ Sideways steering
        }

        private static void UpdateAirControl(ref FighterState fighter, in InputEvents input, FighterConfig config)
        {
            if (input.HeldDirectionX == 0)     // ◀ Stick centered: sideways speed fades slowly
            {
                fighter.Velocity.X = Fixed.MoveTowards(fighter.Velocity.X, Fixed.Zero, config.AirFriction);
                return;
            }

            Fixed targetSpeed = config.AirSpeed * input.StickX / InputFrame.StickMax;
            bool alreadyFaster = input.HeldDirectionX > 0
                ? fighter.Velocity.X > targetSpeed
                : fighter.Velocity.X < targetSpeed;   // ◀ Moving faster than air speed in the held direction (e.g. after a dash jump)?

            Fixed change = alreadyFaster ? config.AirFriction : config.AirAcceleration;
                                               // ◀ If so, only fade gently (keeps momentum). If not, speed up (or brake when holding back).
            fighter.Velocity.X = Fixed.MoveTowards(fighter.Velocity.X, targetSpeed, change);
        }

        private static Fixed GroundSteer(Fixed velocity, Fixed target, int direction, FighterConfig config)
        {                                      // ◀ Like air control, for the ground: speed up with acceleration, bleed off extra with traction
            bool alreadyFaster = direction > 0 ? velocity > target : velocity < target;
            return Fixed.MoveTowards(velocity, target, alreadyFaster ? config.Traction : config.GroundAcceleration);
        }

        private static void Land(ref FighterState fighter)
        {
            fighter.AirJumpsUsed = 0;          // ◀ Air jumps come back
            fighter.IsFastFalling = false;
            SetAction(ref fighter, FighterAction.Idle);   // ◀ Sideways speed is kept, so you slide on landing (traction stops you)
        }

        private static void SetAction(ref FighterState fighter, FighterAction action)
        {                                      // ◀ The ONLY place actions change, so ActionTick always resets with it
            fighter.Action = action;
            fighter.ActionTick = 0;
        }
    }
}
```

**About `ActionTick` timing:** an action starts at `ActionTick = 0` on the tick it begins. `UpdateAfterMove` adds 1 at the end of every tick. So with a 3-frame jump squat, the squat plays on ticks 0, 1 and 2, and Bun leaves the ground on the 4th tick. That's the same as Smash's "jumpsquat 3".

### 3.12 `Scripts/Simulation/Level/LevelGeometry.cs`

**What it is:** the level as the simulation sees it: a list of boxes, each solid, pass-through or fall zone, plus the respawn point. It's built once when Play starts (by `LevelBaker`, 3.21) and never changes, so it isn't part of snapshots.

```csharp
using System.Collections.Generic;

namespace DreamsOfEverus.Simulation
{
    public enum LevelBlockKind                 // ◀ The three kinds of level piece. Shown as the "Kind" dropdown on Level Box.
    {
        Solid,       // Blocks from every side
        PassThrough, // Can be jumped up through and landed on; hard press of Down drops through
        FallZone,    // Touching it sends the fighter back to the respawn point
    }

    public struct LevelBlock                   // ◀ One level piece: a box and its kind
    {
        public FixedBox Box;
        public LevelBlockKind Kind;

        public LevelBlock(FixedBox box, LevelBlockKind kind)
        {
            Box = box;
            Kind = kind;
        }
    }

    public sealed class LevelGeometry
    {
        public readonly List<LevelBlock> Blocks = new List<LevelBlock>();   // ◀ Every level piece
        public FixedVector2 RespawnPoint;                                   // ◀ Where the player starts and comes back to

        public void Add(FixedBox box, LevelBlockKind kind) => Blocks.Add(new LevelBlock(box, kind));

        public bool TouchesFallZone(FixedBox body)  // ◀ Is any part of the body inside any fall zone?
        {
            foreach (LevelBlock block in Blocks)
            {
                if (block.Kind == LevelBlockKind.FallZone && block.Box.Overlaps(body)) return true;
            }
            return false;
        }
    }
}
```

### 3.13 `Scripts/Simulation/Combat/AttackConfig.cs`

**What it is:** one attack's frame data and hitbox in simulation form (built from an `AttackData` asset, 3.19), plus the training dummy's setup.

```csharp
namespace DreamsOfEverus.Simulation
{
    public sealed class AttackConfig
    {
        public int StartupTicks;               // ◀ Windup frames before the hitbox appears
        public int ActiveTicks;                // ◀ Frames the hitbox is out
        public int RecoveryTicks;              // ◀ Frames after, before you can act

        public FixedVector2 HitboxOffset; // Center of the hitbox from the feet. X is forward (flips with facing), Y is up
        public FixedVector2 HitboxSize;

        public int TotalTicks => StartupTicks + ActiveTicks + RecoveryTicks;   // ◀ The whole attack's length

        public bool IsActiveOn(int frame) => frame >= StartupTicks && frame < StartupTicks + ActiveTicks;
                                               // ◀ Frames count from 0. With startup 3 and active 3, frames 3, 4 and 5 have a hitbox
                                               // ◀ (the 4th, 5th and 6th ticks of the attack).
    }

    public sealed class TrainingDummyConfig    // ◀ The dummy's setup, from the Training Dummy object in the scene (3.22)
    {
        public FixedVector2 Position;
        public Fixed HurtboxWidth;
        public Fixed HurtboxHeight;
        public int HitReactTicks;              // ◀ How long it flashes and shakes after a hit
    }
}
```

### 3.14 `Scripts/Simulation/Combat/CombatSystem.cs`

**What it is:** checks attack hitboxes against hurtboxes. Right now the only target is the training dummy, and a hit only makes it react. Damage, Fragility and knockback come in Milestone 4.

```csharp
namespace DreamsOfEverus.Simulation
{
    public static class CombatSystem
    {
        public static bool TryGetActiveHitbox(in FighterState attacker, FighterConfig config, out FixedBox hitbox)
        {                                      // ◀ "Is there a hitbox out right now, and where?" Also used by HitboxView to draw it.
            hitbox = default;                  // ◀ "out" values must always be set, even when returning false
            AttackConfig attack = config.LightAttack;
            if (attacker.Action != FighterAction.LightAttack || attack == null) return false;   // ◀ Not attacking
            if (!attack.IsActiveOn(attacker.ActionTick)) return false;                          // ◀ Attacking, but in startup or recovery

            Fixed centerX = attacker.Position.X + attack.HitboxOffset.X * attacker.Facing;
                                               // ◀ × Facing: the offset points forward, so facing left (-1) puts it on the left side
            Fixed centerY = attacker.Position.Y + attack.HitboxOffset.Y;   // ◀ Up from the feet
            Fixed halfWidth = attack.HitboxSize.X / 2;
            Fixed halfHeight = attack.HitboxSize.Y / 2;

            hitbox = new FixedBox(
                new FixedVector2(centerX - halfWidth, centerY - halfHeight),   // ◀ Box from its center and size
                new FixedVector2(centerX + halfWidth, centerY + halfHeight));
            return true;
        }

        public static bool TryGetHitboxOfLastTick(in FighterState attacker, FighterConfig config, out FixedBox hitbox)
        {                                      // ◀ For visuals only. They read the state AFTER a tick, when ActionTick has already
                                               // ◀ gone up by 1. This looks one frame back, at the frame that was actually checked.
            FighterState lastTick = attacker;  // ◀ A copy (struct), so the real state isn't changed
            lastTick.ActionTick -= 1;
            return TryGetActiveHitbox(lastTick, config, out hitbox);
        }

        public static FixedBox DummyHurtbox(in DummyState dummy, TrainingDummyConfig config)
        {                                      // ◀ The dummy's hittable area: centered on its feet, going up
            Fixed halfWidth = config.HurtboxWidth / 2;
            return new FixedBox(
                new FixedVector2(dummy.Position.X - halfWidth, dummy.Position.Y),
                new FixedVector2(dummy.Position.X + halfWidth, dummy.Position.Y + config.HurtboxHeight));
        }

        public static void Step(ref FighterState attacker, FighterConfig attackerConfig, ref DummyState dummy, TrainingDummyConfig dummyConfig)
        {                                      // ◀ Step 4 of every tick
            if (!dummy.Exists || dummyConfig == null) return;   // ◀ No dummy in this scene

            if (dummy.HitReactTicksLeft > 0) dummy.HitReactTicksLeft--;   // ◀ Reaction counts down by one each tick

            // One hit per attack: once it connects, the rest of its active frames can't hit again.
            if (attacker.AttackHasHit) return;
            if (!TryGetActiveHitbox(attacker, attackerConfig, out FixedBox hitbox)) return;   // ◀ No hitbox out this tick

            if (hitbox.Overlaps(DummyHurtbox(dummy, dummyConfig)))   // ◀ Hitbox touches hurtbox: a hit!
            {
                attacker.AttackHasHit = true;                       // ◀ Remember, so this attack can't hit again
                dummy.HitReactTicksLeft = dummyConfig.HitReactTicks;   // ◀ Start the flash and shake
                dummy.TimesHit++;                                   // ◀ Count it for the readout
            }
        }
    }
}
```

### 3.15 `Scripts/Game/SimulationRunner.cs`

**What it is:** the bridge between Unity and the simulation. Unity draws frames at whatever speed your screen runs (60, 144, 240 Hz...), but the game must tick exactly 60 times a second. Every frame, the runner checks how much real time has passed and runs that many whole ticks. It lives on the **Simulation** object in the scene.

```csharp
using System;
using System.Collections.Generic;
using DreamsOfEverus.Simulation;               // ◀ The gameplay code
using UnityEngine;                             // ◀ Unity itself (MonoBehaviour, Time, Debug...)

namespace DreamsOfEverus.Game                  // ◀ The Unity-side family of code
{
    public interface IInputSource              // ◀ A promise: "I can give you one InputFrame per tick"
    {                                          // ◀ PlayerInputReader keeps this promise. Later, a replay or an AI opponent could too.
        InputFrame ReadInput();
    }

    public class SimulationRunner : MonoBehaviour   // ◀ MonoBehaviour = a component you can put on a GameObject
    {
        [Tooltip("The player character's data asset (body size, movement values...).")]
        [SerializeField] private CharacterData playerCharacter;   // ◀ [SerializeField] shows a private field in the Inspector

        [Tooltip("How stick and button input is read (deadzone, hard press timing...).")]
        [SerializeField] private InputFeelData inputFeel;

        [Tooltip("If the game falls behind (e.g. a lag spike), at most this many ticks run in one frame.")]
        [SerializeField] private int maxTicksPerFrame = 5;

        public CharacterData PlayerCharacter => playerCharacter;          // ◀ Lets other scripts read it (but not swap it)
        public FighterConfig PlayerConfig { get; private set; }           // ◀ Bun's values in simulation form
        public TrainingDummyConfig DummyConfig { get; private set; }
        public GameState State { get; private set; }                      // ◀ The game RIGHT NOW. Visuals read this.
        public GameState PreviousState { get; private set; }              // ◀ The game one tick ago, for smoothing

        public float InterpolationAlpha { get; private set; }             // ◀ 0..1: how far between the last two ticks real time is

        public event Action<InputFrame> TickRan;   // ◀ An announcement after every tick. The snapshot debugger listens to record inputs.

        private const double TickDuration = 1.0 / GameSimulation.TicksPerSecond;   // ◀ 1/60 = 0.01666... seconds

        private GameSimulation simulation;
        private InputFeelConfig inputFeelConfig;
        private IInputSource inputSource;
        private double accumulatedTime;        // ◀ Real time that has passed but hasn't been "spent" on ticks yet

        private void Awake()                   // ◀ Unity calls Awake once, when the scene starts
        {
            if (playerCharacter == null || inputFeel == null)   // ◀ Missing assets: complain clearly and switch off
            {
                Debug.LogError("SimulationRunner: assign both Player Character and Input Feel data.", this);
                enabled = false;
                return;
            }

            PlayerConfig = playerCharacter.ToFighterConfig();   // ◀ Convert the Inspector values to fixed-point
            inputFeelConfig = inputFeel.ToConfig();

            LevelGeometry level = LevelBaker.BakeScene();       // ◀ Read every Level Box cube in the scene (3.21)
            TrainingDummy dummy = FindAnyObjectByType<TrainingDummy>();   // ◀ Is there a dummy in the scene?
            DummyConfig = dummy != null ? dummy.ToConfig() : null;
            simulation = new GameSimulation(level, PlayerConfig, inputFeelConfig, DummyConfig);

            State = simulation.CreateInitialState();   // ◀ Tick 0
            PreviousState = State.Clone();
            inputSource = GetComponent<IInputSource>();   // ◀ Find the PlayerInputReader on the same object
        }

        private void Update()                  // ◀ Unity calls Update once per rendered frame
        {
#if UNITY_EDITOR
            // Live tuning: pick up any values changed in the Inspector during Play.
            // (Editor only: in a real match both players must use identical values.)
            playerCharacter.ApplyTo(PlayerConfig);   // ◀ "#if UNITY_EDITOR" means this code doesn't exist in a built game
            inputFeel.ApplyTo(inputFeelConfig);
#endif

            // Real time only decides HOW MANY ticks to run. The ticks themselves never see it.
            accumulatedTime += Time.unscaledDeltaTime;   // ◀ Add the time this frame took

            int ticksThisFrame = 0;
            while (accumulatedTime >= TickDuration && ticksThisFrame < maxTicksPerFrame)   // ◀ While there's at least one tick's worth of time...
            {
                InputFrame input = inputSource != null ? inputSource.ReadInput() : default;   // ◀ Read input ONCE for this tick
                PreviousState.CopyFrom(State);         // ◀ Remember the old state for smoothing
                simulation.Step(State, input);         // ◀ Run one tick
                TickRan?.Invoke(input);                // ◀ Announce it ("?." = only if someone is listening)

                accumulatedTime -= TickDuration;       // ◀ Spend 1/60 s of the saved-up time
                ticksThisFrame++;
            }
                                               // ◀ On a 144 Hz screen most frames run 0 or 1 tick. On a 30 Hz screen, 2 per frame.

            // Too far behind: drop the backlog instead of trying to catch up forever.
            if (ticksThisFrame == maxTicksPerFrame && accumulatedTime >= TickDuration)
            {
                accumulatedTime = 0;
            }

            InterpolationAlpha = (float)(accumulatedTime / TickDuration);   // ◀ Leftover time as a fraction of a tick, for smooth visuals
        }

        public void RestoreState(GameState snapshot)   // ◀ F9: jump back to a saved snapshot
        {
            State.CopyFrom(snapshot);
            PreviousState.CopyFrom(snapshot); // no smoothing between the old and restored positions
            accumulatedTime = 0;
        }

        public GameState Replay(GameState snapshot, IReadOnlyList<InputFrame> inputs)   // ◀ F10: replay off to the side
        {
            return SnapshotReplay.Replay(simulation, snapshot, inputs);
        }
    }
}
```

**Why smoothing (interpolation) matters:** on a 144 Hz screen, Unity draws about 2.4 frames per tick. Without smoothing, Bun would sit still for 2 frames, then jump forward. `FighterView` instead draws Bun partway between the previous and current tick positions, using `InterpolationAlpha`. That's purely visual: the simulation never sees it.

### 3.16 Input on the Unity side

Three small files turn Unity's Input System into `InputFrame`s.

#### `Scripts/Game/Input/PlayerInputReader.cs`

**What it is:** reads the named actions (Move, Jump, Attack, Walk) from `GameplayControls` and builds one `InputFrame` whenever the runner asks for one. It lives on the **Simulation** object.

```csharp
using DreamsOfEverus.Simulation;
using UnityEngine;
using UnityEngine.InputSystem;                 // ◀ Unity's Input System package

namespace DreamsOfEverus.Game
{
    [DefaultExecutionOrder(-100)]              // ◀ Run this script's Update BEFORE others each frame (lower number = earlier)
    public class PlayerInputReader : MonoBehaviour, IInputSource   // ◀ Keeps the IInputSource promise
    {
        public const string MapName = "Gameplay";      // ◀ Names must match the GameplayControls asset exactly
        public const string MoveAction = "Move";
        public const string JumpAction = "Jump";
        public const string AttackAction = "Attack";
        public const string WalkAction = "Walk";

        [Tooltip("The GameplayControls input actions asset (Assets/Game/Input).")]
        [SerializeField] private InputActionAsset controls;

        [Tooltip("The Input Feel asset (Assets/Game/Data). Sets how far the walk key tilts.")]
        [SerializeField] private InputFeelData inputFeel;

        private InputActionMap gameplayMap;    // ◀ The "Gameplay" group of actions
        private InputAction move;
        private InputAction jump;
        private InputAction attack;
        private InputAction walk;

        private readonly ButtonLatch jumpLatch = new ButtonLatch();     // ◀ Remembers quick taps (see ButtonLatch below)
        private readonly ButtonLatch attackLatch = new ButtonLatch();

        private void Awake()
        {
            if (controls == null || inputFeel == null)
            {
                Debug.LogError("PlayerInputReader: missing assets. Assign the Controls and Input Feel fields.", this);
                enabled = false;
                return;
            }

            gameplayMap = controls.FindActionMap(MapName, throwIfNotFound: true);   // ◀ Look up each action by name once
            move = gameplayMap.FindAction(MoveAction, throwIfNotFound: true);
            jump = gameplayMap.FindAction(JumpAction, throwIfNotFound: true);
            attack = gameplayMap.FindAction(AttackAction, throwIfNotFound: true);
            walk = gameplayMap.FindAction(WalkAction, throwIfNotFound: true);
        }

        private void OnEnable() => gameplayMap?.Enable();    // ◀ Actions only report input while enabled
        private void OnDisable() => gameplayMap?.Disable();

        private void Update()                  // ◀ Every frame: note any button that went down this frame
        {
            if (jump.WasPressedThisFrame()) jumpLatch.NotePress();
            if (attack.WasPressedThisFrame()) attackLatch.NotePress();
        }

        public InputFrame ReadInput()          // ◀ Called by the runner once per tick
        {
            if (!enabled) return default;      // ◀ "default" = an empty InputFrame (nothing held)

            Vector2 stick = InputFrameBuilder.ApplyWalkKey(move.ReadValue<Vector2>(), walk.IsPressed(), inputFeel.keyboardWalkTilt);
                                               // ◀ Read the stick (or keys) and shrink it if Shift is held

            return InputFrameBuilder.Build(
                stick,
                jumpLatch.ConsumeForTick(jump.IsPressed()),      // ◀ Held now, OR tapped since the last tick
                attackLatch.ConsumeForTick(attack.IsPressed()));
        }
    }
}
```

#### `Scripts/Game/Input/ButtonLatch.cs`

**What it is:** makes sure a very quick tap is never lost. Unity can draw several frames between two ticks. If you press and release Jump between ticks, the tick would see "not held" both times and miss it. The latch remembers the press until the next tick reads it.

```csharp
namespace DreamsOfEverus.Game
{
    public sealed class ButtonLatch
    {
        private bool pressedSinceLastTick;     // ◀ The memory: "a press happened and no tick has seen it yet"

        public void NotePress()                // ◀ Called on any frame where the button went down
        {
            pressedSinceLastTick = true;
        }

        public bool ConsumeForTick(bool isHeldNow)
        {
            bool held = isHeldNow || pressedSinceLastTick;   // ◀ Count it as held if it's down now OR was tapped since last tick
            pressedSinceLastTick = false;                    // ◀ This tick has now seen it; forget it
            return held;
        }
    }
}
```

#### `Scripts/Game/Input/InputFrameBuilder.cs`

**What it is:** the one place where Unity's decimal stick values meet the exact simulation. It rounds them to whole numbers from -100 to 100. It also applies the keyboard walk key.

```csharp
using DreamsOfEverus.Simulation;
using UnityEngine;

namespace DreamsOfEverus.Game
{
    public static class InputFrameBuilder
    {
        public static InputFrame Build(Vector2 move, bool jumpHeld, bool attackHeld)
        {
            var buttons = InputButtons.None;
            if (jumpHeld) buttons |= InputButtons.Jump;       // ◀ "|=" switches that button's bit on
            if (attackHeld) buttons |= InputButtons.Attack;

            return new InputFrame
            {
                MoveX = ToStickValue(move.x),
                MoveY = ToStickValue(move.y),
                Held = buttons,
            };
        }

        public static Vector2 ApplyWalkKey(Vector2 move, bool walkHeld, int walkTiltPercent)
        {
            if (!walkHeld) return move;        // ◀ Shift not held: leave input alone
            return move * (walkTiltPercent / 100f); // each axis on its own, so Shift + Down + Right still crawls
                                               // ◀ e.g. D key = 1.0 becomes 0.65: under the 80% press threshold, so it walks
        }

        private static sbyte ToStickValue(float axis)
        {
            return (sbyte)Mathf.RoundToInt(Mathf.Clamp(axis, -1f, 1f) * InputFrame.StickMax);
                                               // ◀ Keep it between -1 and 1, scale to -100..100, round to a whole number
        }
    }
}
```

### 3.17 `Scripts/Game/Data/CharacterData.cs`

**What it is:** the code behind `BunCharacterData` and any future character's data asset. It defines the fields you see in the Inspector, in friendly units, and converts them into a `FighterConfig` for the simulation. **The numbers written in this file are only the defaults for a brand-new asset.** The game uses whatever is saved in the asset itself.

**Unit conversions used here:**
- Speeds: units per second ÷ 60 = units per tick.
- Accelerations: units per second per second ÷ 3600 (60 × 60) = units per tick per tick.
- Jump heights become launch speeds with the physics formula *speed = √(2 × gravity × height)*.
- Trot distance becomes trot length in ticks: distance ÷ dash speed per tick, rounded up.

```csharp
using System;
using DreamsOfEverus.Simulation;
using UnityEngine;
using UnityEngine.Serialization;               // ◀ For FormerlySerializedAs (keeping values when a field is renamed)

namespace DreamsOfEverus.Game
{
    [CreateAssetMenu(menuName = "Dreams of Everus/Character Data", fileName = "CharacterData")]
                                               // ◀ Adds "Create > Dreams of Everus > Character Data" to the Project window's right-click menu
    public class CharacterData : ScriptableObject   // ◀ ScriptableObject = a data asset file, not a component
    {
        private const int TicksPerSecond = GameSimulation.TicksPerSecond;

        [Header("Body (PLACEHOLDER)")]         // ◀ [Header] draws a bold title in the Inspector
        [Tooltip("Width of the collision box, in world units (1 unit = 1 Unity grid square).")]   // ◀ [Tooltip] = hover text
        [Min(0.1f)] public float bodyWidth = 0.8f;   // ◀ [Min] stops the Inspector going below 0.1
        [Tooltip("Height of the collision box, from the feet up, in world units.")]
        [Min(0.1f)] public float bodyHeight = 1.8f;

        [Header("Walk (PLACEHOLDER)")]
        [Tooltip("Top walking speed at full tilt, in units per second. Half tilt walks at half this.")]
        [Min(0f)] public float walkSpeed = 4f;
        [Tooltip("How quickly walking and running speed up, in units per second, per second.")]
        [Min(0f)] public float groundAcceleration = 60f;
        [Tooltip("Traction: how quickly the character sheds speed on the ground, in units per second, per second. Used for stopping, skids, turnarounds and slides. Low = slippery (slides further), high = grippy.")]
        [FormerlySerializedAs("groundFriction")]   // ◀ This field used to be called groundFriction; this keeps old saved values
        [Min(0.1f)] public float traction = 40f;

        [Header("Dash, trot and run (PLACEHOLDER)")]
        [Tooltip("Speed of the initial dash (started by a hard sideways press), in units per second.")]
        [Min(0.1f)] public float dashSpeed = 11f;
        [Tooltip("The trot: how far the initial dash travels, in units. During the trot, a hard press the other way dash dances. After it, the character runs.")]
        [Min(0.05f)] public float trotDistance = 2.2f;
        [Tooltip("Top running speed (after the trot), in units per second.")]
        [Min(0f)] public float runSpeed = 9f;
        [Tooltip("How many frames the run turnaround (turning while running) takes. The character skids on their traction during it.")]
        [Min(1)] public int runTurnaroundFrames = 12;
        [Tooltip("Moonwalk strength: ... 0 = can't moonwalk. Higher = stronger, longer moonwalks.")]   // ◀ (tooltip shortened here)
        [Min(0f)] public float moonwalkAcceleration = 150f;

        [Header("Crouch and crawl (PLACEHOLDER)")]
        [Tooltip("Body height while crouching or crawling, in units. Lets the character fit under low ceilings.")]
        [Min(0.1f)] public float crouchHeight = 1.0f;
        [Tooltip("Whether this character can crawl (move while crouched). Every character can crouch.")]
        public bool canCrawl = false;          // ◀ Off by default for new characters. Ticked in Bun's asset.
        [Tooltip("Top crawling speed at full tilt, in units per second.")]
        [Min(0f)] public float crawlSpeed = 2.5f;

        [Header("Jump (PLACEHOLDER)")]
        [Tooltip("Frames of crouch before leaving the ground. Release Jump during these for a short hop.")]
        [Min(1)] public int jumpSquatFrames = 3;
        [Tooltip("Roughly how high a full jump goes, in units.")]
        [Min(0.1f)] public float fullHopHeight = 4f;
        [Tooltip("Roughly how high a short hop goes, in units.")]
        [Min(0.1f)] public float shortHopHeight = 1.8f;
        [Tooltip("Roughly how high an air jump goes, in units.")]
        [Min(0.1f)] public float airJumpHeight = 3.5f;
        [Tooltip("How many jumps the character can do in the air before landing.")]
        [Min(0)] public int airJumps = 1;
        [Tooltip("Most sideways speed carried from the ground into a jump, in units per second. Higher = dash and run jumps travel further.")]
        [Min(0f)] public float maxJumpMomentum = 11f;

        [Header("Air (PLACEHOLDER)")]
        [Tooltip("How strongly the character is pulled down, in units per second, per second. Also changes how jump heights feel.")]
        [Min(0.1f)] public float gravity = 70f;
        [Tooltip("Top normal falling speed, in units per second.")]
        [Min(0.1f)] public float maxFallSpeed = 18f;
        [Tooltip("Falling speed while fast-falling, in units per second.")]
        [Min(0.1f)] public float fastFallSpeed = 27f;
        [Tooltip("Top sideways speed in the air, in units per second.")]
        [Min(0f)] public float airSpeed = 7f;
        [Tooltip("How quickly the character can steer in the air, in units per second, per second.")]
        [Min(0f)] public float airAcceleration = 40f;
        [Tooltip("How quickly sideways air speed fades with the stick centered, in units per second, per second.")]
        [Min(0f)] public float airFriction = 6f;

        [Header("Attacks")]
        [Tooltip("The attack on Square (Normal attacks).")]
        public AttackData lightAttack;         // ◀ A link to an AttackData asset (drag one in)

        public FighterConfig ToFighterConfig() // ◀ Make a brand-new config (at the start of play)
        {
            var config = new FighterConfig();
            ApplyTo(config);
            return config;
        }

        public void ApplyTo(FighterConfig config)   // ◀ Copy every value into a config. Also called every frame in the Editor for live tuning.
        {
            config.BodyWidth = Fixed.FromFloat(bodyWidth);   // ◀ Distances stay as they are, just converted to Fixed
            config.BodyHeight = Fixed.FromFloat(bodyHeight);

            config.WalkSpeed = PerTick(walkSpeed);           // ◀ Speeds: ÷ 60
            config.GroundAcceleration = PerTickSquared(groundAcceleration);   // ◀ Accelerations: ÷ 3600
            config.Traction = PerTickSquared(traction);
            config.DashSpeed = PerTick(dashSpeed);
            config.TrotTicks = Math.Max(1, (int)Math.Ceiling(trotDistance / (dashSpeed / TicksPerSecond)));
                                               // ◀ How many ticks it takes to cover the trot distance at dash speed. 2.2 ÷ (11/60) = 12 ticks.
            config.RunSpeed = PerTick(runSpeed);
            config.RunTurnaroundTicks = runTurnaroundFrames;  // ◀ Frames are already ticks, no conversion
            config.MoonwalkAcceleration = PerTickSquared(moonwalkAcceleration);

            config.CrouchHeight = Fixed.FromFloat(Math.Min(crouchHeight, bodyHeight));   // ◀ Never taller than standing
            config.CanCrawl = canCrawl;
            config.CrawlSpeed = PerTick(crawlSpeed);

            config.JumpSquatTicks = jumpSquatFrames;
            config.FullHopVelocity = JumpVelocityFor(fullHopHeight);    // ◀ Heights become launch speeds
            config.ShortHopVelocity = JumpVelocityFor(shortHopHeight);
            config.AirJumpVelocity = JumpVelocityFor(airJumpHeight);
            config.AirJumps = airJumps;
            config.MaxJumpMomentum = PerTick(maxJumpMomentum);

            config.Gravity = PerTickSquared(gravity);
            config.MaxFallSpeed = PerTick(maxFallSpeed);
            config.FastFallSpeed = PerTick(fastFallSpeed);
            config.AirSpeed = PerTick(airSpeed);
            config.AirAcceleration = PerTickSquared(airAcceleration);
            config.AirFriction = PerTickSquared(airFriction);

            if (lightAttack == null) config.LightAttack = null;   // ◀ No attack assigned: the simulation won't let you attack
            else lightAttack.ApplyTo(config.LightAttack ??= new AttackConfig());
                                               // ◀ "??=" creates the AttackConfig the first time, then reuses it on every later call
        }

        // Units per second -> units per tick.
        private static Fixed PerTick(float perSecond) => Fixed.FromFloat(perSecond) / TicksPerSecond;

        // Units per second per second -> units per tick per tick.
        private static Fixed PerTickSquared(float perSecondSquared) => Fixed.FromFloat(perSecondSquared) / (TicksPerSecond * TicksPerSecond);

        // Launch speed (per tick) that reaches about this height under this character's gravity: v = sqrt(2 * g * h).
        private Fixed JumpVelocityFor(float height)
        {
            double gravityPerTick = (double)gravity / (TicksPerSecond * TicksPerSecond);
            return Fixed.FromFloat((float)Math.Sqrt(2.0 * gravityPerTick * height));
                                               // ◀ "About" because the game moves in steps of 1/60 s, so the real peak is a touch off
        }
    }
}
```

### 3.18 `Scripts/Game/Data/InputFeelData.cs`

**What it is:** the code behind the `InputFeel` asset: how input is read, shared by every character.

```csharp
using DreamsOfEverus.Simulation;
using UnityEngine;

namespace DreamsOfEverus.Game
{
    [CreateAssetMenu(menuName = "Dreams of Everus/Input Feel", fileName = "InputFeel")]
    public class InputFeelData : ScriptableObject
    {
        [Header("Stick (PLACEHOLDER)")]
        [Tooltip("Stick values smaller than this (out of 100) count as centered.")]
        [Range(0, 99)] public int stickDeadzone = 25;      // ◀ [Range] shows a slider in the Inspector
        [Tooltip("How far (out of 100) the stick must go to count as a press.")]
        [Range(1, 100)] public int pressThreshold = 80;

        [Header("Hard press (PLACEHOLDER)")]
        [Tooltip("A press counts as 'hard', like a smash input (dash, dash dance, drop-through, fast-fall), if the stick gets from not pushed to the press threshold within this many frames. Easing it over slower is just a tilt.")]
        [Min(1)] public int hardPressWindowFrames = 2;

        [Header("Keyboard (PLACEHOLDER)")]
        [Tooltip("While the walk key (Shift) is held, keyboard movement only tilts this far (out of 100). Keep it below the press threshold so it walks instead of dashing. Also how keyboard players moonwalk.")]
        [Range(1, 100)] public int keyboardWalkTilt = 65;   // ◀ Read by PlayerInputReader, not the simulation

        [Header("Crouch (PLACEHOLDER)")]
        [Tooltip("How far down (out of 100) the stick must be held to crouch.")]
        [Range(1, 100)] public int crouchThreshold = 50;

        public InputFeelConfig ToConfig()
        {
            var config = new InputFeelConfig();
            ApplyTo(config);
            return config;
        }

        public void ApplyTo(InputFeelConfig config)   // ◀ These are whole numbers already, so no Fixed conversion is needed
        {
            config.StickDeadzone = stickDeadzone;
            config.PressThreshold = pressThreshold;
            config.HardPressWindowTicks = hardPressWindowFrames;
            config.CrouchThreshold = crouchThreshold;
        }
    }
}
```

### 3.19 `Scripts/Game/Data/AttackData.cs`

**What it is:** the code behind `BunLightAttack`: one attack's frame data and hitbox. Each character's `CharacterData` points to their own attack assets.

```csharp
using DreamsOfEverus.Simulation;
using UnityEngine;

namespace DreamsOfEverus.Game
{
    [CreateAssetMenu(menuName = "Dreams of Everus/Attack Data", fileName = "AttackData")]
    public class AttackData : ScriptableObject
    {
        [Header("Frame data (PLACEHOLDER)")]
        [Tooltip("Frames of windup before the hitbox comes out.")]
        [Min(0)] public int startupFrames = 3;
        [Tooltip("Frames the hitbox is out and can hit.")]
        [Min(1)] public int activeFrames = 3;
        [Tooltip("Frames after the hitbox is gone before the character can act again.")]
        [Min(0)] public int recoveryFrames = 12;

        [Header("Hitbox (PLACEHOLDER)")]
        [Tooltip("Center of the hitbox measured from the character's feet, in units. X is forward (it flips when facing left), Y is up.")]
        public Vector2 hitboxOffset = new Vector2(0.75f, 1.1f);
        [Tooltip("Width and height of the hitbox, in units.")]
        public Vector2 hitboxSize = new Vector2(0.8f, 0.5f);

        public int TotalFrames => startupFrames + activeFrames + recoveryFrames;   // ◀ 3 + 3 + 12 = 18 frames in total

        public void ApplyTo(AttackConfig config)
        {
            config.StartupTicks = startupFrames;
            config.ActiveTicks = activeFrames;
            config.RecoveryTicks = recoveryFrames;
            config.HitboxOffset = new FixedVector2(Fixed.FromFloat(hitboxOffset.x), Fixed.FromFloat(hitboxOffset.y));
            config.HitboxSize = new FixedVector2(Fixed.FromFloat(hitboxSize.x), Fixed.FromFloat(hitboxSize.y));
        }
    }
}
```

### 3.20 `Scripts/Game/Data/FollowCameraData.cs`

**What it is:** the code behind the `FollowCamera` asset. These are camera settings only, so they're plain decimals: the camera never affects gameplay.

```csharp
using UnityEngine;

namespace DreamsOfEverus.Game
{
    [CreateAssetMenu(menuName = "Dreams of Everus/Follow Camera", fileName = "FollowCamera")]
    public class FollowCameraData : ScriptableObject
    {
        [Header("Framing (PLACEHOLDER)")]
        [Tooltip("How far the camera sits from the point it looks at, in units. Bigger = more of the room in view.")]
        [Min(1f)] public float distance = 16f;
        [Tooltip("The camera looks at a point this far above the character's feet, in units.")]
        public float heightOffset = 1.5f;
        [Tooltip("Tilts the camera to look slightly down, in degrees. 0 = perfectly side-on.")]
        [Range(-30f, 30f)] public float pitch = 6f;
        [Tooltip("Field of view, in degrees. Lower = flatter, more 2D-looking perspective.")]
        [Range(10f, 90f)] public float fieldOfView = 40f;

        [Header("Look-ahead (PLACEHOLDER)")]
        [Tooltip("How far ahead of the character (in the direction they face) the camera leans, in units.")]
        [Min(0f)] public float lookAheadDistance = 1.5f;
        [Tooltip("How long the look-ahead takes to swing to the other side after turning, in seconds.")]
        [Min(0.01f)] public float lookAheadSmoothTime = 0.5f;

        [Header("Smoothing (PLACEHOLDER)")]
        [Tooltip("How long the camera takes to catch up sideways, in seconds. Smaller = tighter.")]
        [Min(0.01f)] public float horizontalSmoothTime = 0.15f;
        [Tooltip("How long the camera takes to catch up vertically, in seconds. Lazier than sideways so jumps don't jerk the view.")]
        [Min(0.01f)] public float verticalSmoothTime = 0.3f;
        [Tooltip("If the character suddenly moves further than this (e.g. respawning), the camera jumps straight there, in units.")]
        [Min(1f)] public float snapDistance = 8f;
    }
}
```

### 3.21 Building the level from the scene

You build levels by placing ordinary cubes in the Scene view. When Play starts, `LevelBaker` reads them and builds the simulation's `LevelGeometry`.

#### `Scripts/Game/Level/LevelBox.cs`

**What it is:** a component you put on a cube to make it level collision. Its position and scale become a box in the simulation. Keep these cubes unrotated, because rotation is ignored.

```csharp
using DreamsOfEverus.Simulation;
using UnityEngine;

namespace DreamsOfEverus.Game
{
    [DisallowMultipleComponent]                // ◀ Only one Level Box per object
    public class LevelBox : MonoBehaviour
    {
        [Tooltip("Solid blocks from every side. Pass Through can be jumped up through and dropped through with a hard Down. Fall Zone respawns the player.")]
        [SerializeField] private LevelBlockKind kind = LevelBlockKind.Solid;   // ◀ The "Kind" dropdown in the Inspector

        public LevelBlockKind Kind => kind;

        public LevelBlock ToBlock()            // ◀ Convert this cube into a simulation box
        {
            Vector3 center = transform.position;              // ◀ The cube's middle
            Vector3 halfSize = transform.lossyScale * 0.5f;   // ◀ Half its size (a Unity cube is 1×1×1 at scale 1)

            var min = new FixedVector2(Fixed.FromFloat(center.x - halfSize.x), Fixed.FromFloat(center.y - halfSize.y));   // ◀ bottom-left
            var max = new FixedVector2(Fixed.FromFloat(center.x + halfSize.x), Fixed.FromFloat(center.y + halfSize.y));   // ◀ top-right
            return new LevelBlock(new FixedBox(min, max), kind);
        }

        private void OnDrawGizmos()            // ◀ Draws outlines in the Scene view only (not in the game)
        {
            Gizmos.color = kind switch         // ◀ Pick a color by kind
            {
                LevelBlockKind.PassThrough => new Color(0.3f, 0.7f, 1f),   // ◀ blue
                LevelBlockKind.FallZone => new Color(1f, 0.2f, 0.2f),      // ◀ red
                _ => Color.white,                                          // ◀ "_" = anything else (solid): white
            };
            Vector3 scale = transform.lossyScale;
            Gizmos.DrawWireCube(transform.position, new Vector3(scale.x, scale.y, 0.01f));   // ◀ A flat outline on the gameplay plane
        }
    }
}
```

#### `Scripts/Game/Level/RespawnPoint.cs`

**What it is:** a marker for where the player starts and comes back to. Its position is the feet position.

```csharp
using UnityEngine;

namespace DreamsOfEverus.Game
{
    [DisallowMultipleComponent]
    public class RespawnPoint : MonoBehaviour
    {
        private void OnDrawGizmos()            // ◀ A green circle and line in the Scene view
        {
            Gizmos.color = Color.green;
            Vector3 feet = transform.position;
            Gizmos.DrawWireSphere(feet + Vector3.up * 0.25f, 0.25f);
            Gizmos.DrawLine(feet, feet + Vector3.up * 1.5f);
        }
    }
}
```

#### `Scripts/Game/Level/LevelBaker.cs`

**What it is:** finds every `LevelBox` and the `RespawnPoint` in the open scene and builds the `LevelGeometry`. It's called once by `SimulationRunner.Awake`.

```csharp
using DreamsOfEverus.Simulation;
using UnityEngine;

namespace DreamsOfEverus.Game
{
    public static class LevelBaker
    {
        public static LevelGeometry BakeScene()
        {
            var level = new LevelGeometry();

            foreach (LevelBox box in Object.FindObjectsByType<LevelBox>())   // ◀ Every Level Box in the scene
            {
                level.Add(box.ToBlock().Box, box.Kind);
            }

            RespawnPoint respawn = Object.FindAnyObjectByType<RespawnPoint>();   // ◀ The respawn marker
            if (respawn != null)
            {
                Vector3 p = respawn.transform.position;
                level.RespawnPoint = new FixedVector2(Fixed.FromFloat(p.x), Fixed.FromFloat(p.y));
            }
            else
            {
                Debug.LogWarning("LevelBaker: no RespawnPoint in the scene, using (0, 0).");
            }

            return level;
        }
    }
}
```

**Note:** the order Unity finds the boxes in doesn't matter. The collision code always picks the nearest blocking edge, so the result is the same in any order. That matters for determinism.

### 3.22 `Scripts/Game/Combat/TrainingDummy.cs`

**What it is:** marks where the dummy stands (its position is its feet) and sets its hurtbox size and reaction length. The yellow outline in the Scene view shows the hurtbox.

```csharp
using DreamsOfEverus.Simulation;
using UnityEngine;

namespace DreamsOfEverus.Game
{
    [DisallowMultipleComponent]
    public class TrainingDummy : MonoBehaviour
    {
        [Header("Hurtbox (PLACEHOLDER)")]
        [Tooltip("Width of the area that can be hit, in units.")]
        [Min(0.1f)] public float hurtboxWidth = 0.8f;
        [Tooltip("Height of the area that can be hit, from the feet up, in units.")]
        [Min(0.1f)] public float hurtboxHeight = 1.8f;

        [Header("Reaction (PLACEHOLDER)")]
        [Tooltip("How many frames the dummy flashes and shakes after being hit.")]
        [Min(1)] public int hitReactionFrames = 20;

        public TrainingDummyConfig ToConfig()  // ◀ Convert to simulation form when Play starts
        {
            Vector3 feet = transform.position;
            return new TrainingDummyConfig
            {
                Position = new FixedVector2(Fixed.FromFloat(feet.x), Fixed.FromFloat(feet.y)),
                HurtboxWidth = Fixed.FromFloat(hurtboxWidth),
                HurtboxHeight = Fixed.FromFloat(hurtboxHeight),
                HitReactTicks = hitReactionFrames,
            };
        }

        private void OnDrawGizmos()            // ◀ Yellow hurtbox outline in the Scene view
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + Vector3.up * hurtboxHeight * 0.5f, new Vector3(hurtboxWidth, hurtboxHeight, 0.01f));
        }
    }
}
```

### 3.23 `Scripts/Game/View/FighterView.cs`

**What it is:** moves the orange capsule to where the simulation says Bun is, turns it to show facing, and resizes it when crouching. It reads the state and never changes it. It lives on the **Bun** object.

```csharp
using DreamsOfEverus.Simulation;
using UnityEngine;

namespace DreamsOfEverus.Game
{
    public class FighterView : MonoBehaviour
    {
        [SerializeField] private SimulationRunner runner;   // ◀ Where to read the game state from

        [Tooltip("Turns to show facing. The body and nose are its children.")]
        [SerializeField] private Transform visualRoot;

        [Tooltip("The placeholder body (a capsule). Sized to match the character's collision box.")]
        [SerializeField] private Transform body;

        [Tooltip("The small block that shows which way the character faces.")]
        [SerializeField] private Transform nose;

        private void LateUpdate()              // ◀ LateUpdate runs after every Update, so the ticks for this frame have already happened
        {
            if (runner == null || runner.State == null) return;
            FighterState player = runner.State.Player;

            Vector2 previous = ToVector(runner.PreviousState.Player.Position);   // ◀ Where Bun was last tick
            Vector2 current = ToVector(player.Position);                        // ◀ Where Bun is this tick
            Vector2 shown = Vector2.Lerp(previous, current, runner.InterpolationAlpha);
                                               // ◀ Draw partway between them (Lerp = blend), so movement looks smooth on fast screens
            transform.position = new Vector3(shown.x, shown.y, 0f);   // ◀ z = 0: everything gameplay happens on one flat plane

            if (visualRoot != null) visualRoot.localRotation = Quaternion.Euler(0f, FacingAngle(player), 0f);   // ◀ Turn around the up axis
            MatchBodySize(player);
        }

        // Sizes the placeholder capsule to the collision box: shorter while crouching or crawling.
        private void MatchBodySize(FighterState player)
        {
            if (body == null || runner.PlayerConfig == null) return;

            float width = runner.PlayerConfig.BodyWidth.ToFloat();
            float height = runner.PlayerConfig.HeightFor(player).ToFloat();   // ◀ The SAME height collision uses, so what you see is what collides

            // Unity's capsule is 1 wide and 2 tall at scale 1, with its pivot in the middle.
            body.localScale = new Vector3(width, height * 0.5f, width);       // ◀ Height ÷ 2 because the capsule is 2 tall at scale 1
            body.localPosition = new Vector3(0f, height * 0.5f, 0f);          // ◀ Lift it so its bottom sits at the feet
            if (nose != null) nose.localPosition = new Vector3(width * 0.5f, height * 0.75f, 0f);   // ◀ Nose on the front, near the top
        }

        // 0 degrees faces right, 180 faces left. During a run turnaround the body swings around slowly.
        private float FacingAngle(FighterState player)
        {
            float angle = player.Facing > 0 ? 0f : 180f;
            if (player.Action == FighterAction.RunTurnaround && runner.PlayerConfig != null)
            {
                float progress = Mathf.Clamp01((float)player.ActionTick / runner.PlayerConfig.RunTurnaroundTicks);   // ◀ 0 at start, 1 when done
                angle += 180f * progress;      // ◀ Swing halfway round over the turnaround's frames
            }
            return angle;
        }

        private static Vector2 ToVector(FixedVector2 v) => new Vector2(v.X.ToFloat(), v.Y.ToFloat());   // ◀ Fixed → float, for drawing only
    }
}
```

### 3.24 `Scripts/Game/View/TrainingDummyView.cs`

**What it is:** makes the dummy flash red and shake while it's reacting to a hit. Visual only.

```csharp
using UnityEngine;

namespace DreamsOfEverus.Game
{
    public class TrainingDummyView : MonoBehaviour
    {
        [SerializeField] private SimulationRunner runner;
        [SerializeField] private Transform body;
        [SerializeField] private Renderer bodyRenderer;      // ◀ The part that draws the cube, so its color can change

        [Header("Look (visual only)")]
        [SerializeField] private Color normalColor = new Color(0.85f, 0.85f, 0.8f);
        [SerializeField] private Color hitColor = new Color(1f, 0.25f, 0.2f);
        [Tooltip("How far the dummy shakes sideways when hit, in units.")]
        [SerializeField] private float shakeAmount = 0.12f;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");   // ◀ The URP material's color setting, looked up once
        private MaterialPropertyBlock colors;  // ◀ Changes this one object's color without editing the shared material asset
        private Vector3 bodyRestPosition;      // ◀ Where the body sits when not shaking

        private void Start()
        {
            colors = new MaterialPropertyBlock();
            if (body != null) bodyRestPosition = body.localPosition;
        }

        private void LateUpdate()
        {
            if (runner == null || runner.State == null || runner.DummyConfig == null || body == null) return;

            int ticksLeft = runner.State.Dummy.HitReactTicksLeft;
            float reaction = (float)ticksLeft / runner.DummyConfig.HitReactTicks; // 1 right after the hit, fading to 0

            // Shake side to side, getting smaller as the reaction fades.
            float shake = ticksLeft > 0 ? Mathf.Sin(ticksLeft * 2.5f) * shakeAmount * reaction : 0f;   // ◀ Sin swings between -1 and 1
            body.localPosition = bodyRestPosition + new Vector3(shake, 0f, 0f);

            if (bodyRenderer != null)
            {
                colors.SetColor(BaseColorId, Color.Lerp(normalColor, hitColor, reaction));   // ◀ Fully red at the hit, fading back
                bodyRenderer.SetPropertyBlock(colors);
            }
        }
    }
}
```

### 3.25 `Scripts/Game/View/HitboxView.cs`

**What it is:** draws the attack's hitbox as a see-through red box on the frames it's out. It asks `CombatSystem` for the exact box the game uses, so what you see is exactly what can hit. F2 toggles it.

```csharp
using DreamsOfEverus.Simulation;
using UnityEngine;

namespace DreamsOfEverus.Game
{
    public class HitboxView : MonoBehaviour
    {
        [SerializeField] private SimulationRunner runner;
        [SerializeField] private Renderer box;  // ◀ The red cube
        [SerializeField] private bool visible = true;

        private void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current?.f2Key.wasPressedThisFrame == true) visible = !visible;   // ◀ F2 flips it on/off
        }

        private void LateUpdate()
        {
            if (box == null) return;

            FixedBox hitbox = default;
            bool show = visible && runner != null && runner.State != null
                && CombatSystem.TryGetHitboxOfLastTick(runner.State.Player, runner.PlayerConfig, out hitbox);
                                               // ◀ "Of last tick": the hitbox that was checked during the tick that just ran (see 3.14)
            box.enabled = show;                // ◀ Hide the cube when there's no hitbox
            if (!show) return;

            Vector2 min = new Vector2(hitbox.Min.X.ToFloat(), hitbox.Min.Y.ToFloat());
            Vector2 max = new Vector2(hitbox.Max.X.ToFloat(), hitbox.Max.Y.ToFloat());

            // Slightly in front of everything (toward the camera) so it's never hidden.
            box.transform.position = new Vector3((min.x + max.x) * 0.5f, (min.y + max.y) * 0.5f, -1.2f);   // ◀ Center of the box
            box.transform.localScale = new Vector3(max.x - min.x, max.y - min.y, 0.05f);                     // ◀ Its size, very thin
        }
    }
}
```

**Note:** it draws the hitbox at this tick's exact position rather than smoothing it, so on fast screens it can sit a hair ahead of the smoothed capsule. That's on purpose: it shows the true hitbox.

### 3.26 `Scripts/Game/View/FollowCamera.cs`

**What it is:** the side-on perspective camera that follows Bun. It lives on **Main Camera**. Visual only.

```csharp
using UnityEngine;

namespace DreamsOfEverus.Game
{
    [DefaultExecutionOrder(100)]               // ◀ Run AFTER FighterView has moved Bun this frame, so the camera never lags a frame
    [RequireComponent(typeof(Camera))]         // ◀ Must be on an object with a Camera
    public class FollowCamera : MonoBehaviour
    {
        [SerializeField] private SimulationRunner runner;   // ◀ Only used to read which way Bun faces
        [Tooltip("The character to follow (their view object, which is already smoothed between ticks).")]
        [SerializeField] private Transform target;
        [SerializeField] private FollowCameraData settings;

        private Camera cam;
        private Vector3 lastTargetPosition;    // ◀ Used to spot sudden jumps (respawn)
        private float lookAhead;               // ◀ Current lean, sliding between -lookAheadDistance and +lookAheadDistance
        private float lookAheadSpeed;          // ◀ SmoothDamp's helper values: how fast each one is currently changing
        private float horizontalSpeed;
        private float verticalSpeed;

        private void Start()
        {
            cam = GetComponent<Camera>();
            if (target == null || settings == null) return;

            lookAhead = Facing() * settings.lookAheadDistance;
            SnapToTarget();                    // ◀ Start already in position
        }

        private void LateUpdate()
        {
            if (target == null || settings == null) return;

            cam.fieldOfView = settings.fieldOfView;   // ◀ Applied every frame so live tuning works
            transform.rotation = Quaternion.Euler(settings.pitch, 0f, 0f);

            // A big sudden jump (respawn) snaps the camera instead of sweeping across the level.
            if (Vector3.Distance(target.position, lastTargetPosition) > settings.snapDistance)
            {
                lookAhead = Facing() * settings.lookAheadDistance;
                SnapToTarget();
                return;
            }
            lastTargetPosition = target.position;

            // Lean toward the way the character faces, swinging over smoothly when they turn.
            lookAhead = Mathf.SmoothDamp(lookAhead, Facing() * settings.lookAheadDistance, ref lookAheadSpeed, settings.lookAheadSmoothTime);
                                               // ◀ SmoothDamp eases a value toward a goal over roughly "smooth time" seconds, like a soft spring

            Vector3 desired = DesiredPosition(target.position, lookAhead, settings);   // ◀ Where the camera wants to be
            Vector3 current = transform.position;
            current.x = Mathf.SmoothDamp(current.x, desired.x, ref horizontalSpeed, settings.horizontalSmoothTime);   // ◀ Ease sideways (tight)
            current.y = Mathf.SmoothDamp(current.y, desired.y, ref verticalSpeed, settings.verticalSmoothTime);       // ◀ Ease up/down (lazier)
            current.z = desired.z;             // ◀ Distance changes apply straight away
            transform.position = current;
        }

        public static Vector3 DesiredPosition(Vector3 targetPosition, float lookAhead, FollowCameraData settings)
        {                                      // ◀ "static" and pure math, so the tests and the scene builder can use it too
            Vector3 focus = targetPosition + new Vector3(lookAhead, settings.heightOffset, 0f);   // ◀ The point the camera looks at
            Vector3 viewDirection = Quaternion.Euler(settings.pitch, 0f, 0f) * Vector3.forward;  // ◀ The direction the tilted camera faces
            return focus - viewDirection * settings.distance;   // ◀ Back up from the focus point along that direction
        }

        private void SnapToTarget()
        {
            transform.rotation = Quaternion.Euler(settings.pitch, 0f, 0f);
            transform.position = DesiredPosition(target.position, lookAhead, settings);
            lastTargetPosition = target.position;
            horizontalSpeed = 0f;              // ◀ Reset the easing so it doesn't keep drifting
            verticalSpeed = 0f;
        }

        private float Facing()
        {
            if (runner == null || runner.State == null) return 1f;
            return runner.State.Player.Facing < 0 ? -1f : 1f;
        }
    }
}
```

### 3.27 `Scripts/Game/View/DebugHud.cs`

**What it is:** the white text readout in the top-left corner. F1 toggles it. It lives on the **Simulation** object.

```csharp
using DreamsOfEverus.Simulation;
using UnityEngine;

namespace DreamsOfEverus.Game
{
    public class DebugHud : MonoBehaviour
    {
        [SerializeField] private SimulationRunner runner;
        [SerializeField] private bool visible = true;

        private GUIStyle style;                // ◀ Font settings for the text

        private void Update()
        {
            if (UnityEngine.InputSystem.Keyboard.current?.f1Key.wasPressedThisFrame == true) visible = !visible;
        }

        private void OnGUI()                   // ◀ Unity's simple built-in way to draw debug text on screen
        {
            if (!visible || runner == null || runner.State == null) return;
            style ??= new GUIStyle(GUI.skin.label) { fontSize = 18 };   // ◀ Create the style the first time only

            FighterState p = runner.State.Player;
            float speedX = p.Velocity.X.ToFloat() * GameSimulation.TicksPerSecond;   // ◀ Per tick × 60 = per second, easier to read
            float speedY = p.Velocity.Y.ToFloat() * GameSimulation.TicksPerSecond;

            string text =                      // ◀ Build the lines of text. "\n" = new line.
                $"Action: {p.Action} (frame {p.ActionTick}){(IsMoonwalking(p) ? "  MOONWALKING" : "")}\n" +
                $"Facing: {(p.Facing > 0 ? "Right" : "Left")}\n" +
                $"Speed: {speedX:0.0}, {speedY:0.0} units/s\n" +
                $"Ground: {p.Ground}\n" +
                $"Air jumps used: {p.AirJumpsUsed}\n" +
                $"Fast-falling: {(p.IsFastFalling ? "yes" : "no")}\n" +
                AttackLine(p) + "\n" +
                $"Dummy hits: {runner.State.Dummy.TimesHit}\n" +
                $"Tick: {runner.State.Tick}   (F1 hides this, F2 hides hitboxes)";

            GUI.Label(new Rect(12, 12, 700, 300), text, style);   // ◀ Draw it 12 pixels from the top-left
        }

        // "Light attack: frame 5 / 18 (HITBOX OUT)" while attacking.
        private string AttackLine(FighterState p)
        {
            AttackConfig attack = runner.PlayerConfig?.LightAttack;
            if (p.Action != FighterAction.LightAttack || attack == null) return "Light attack: -";

            // After a tick, ActionTick has already moved on, so the frame that just played is ActionTick - 1
            // (shown counting from 1, like frame data: "frame 1" is the first frame).
            int framePlayed = p.ActionTick - 1;
            string phase = attack.IsActiveOn(framePlayed) ? "HITBOX OUT"
                : (framePlayed < attack.StartupTicks ? "startup" : "recovery");
            return $"Light attack: frame {framePlayed + 1} / {attack.TotalTicks} ({phase}){(p.AttackHasHit ? "  HIT" : "")}";
        }

        // Sliding backward while still dashing forward.
        private static bool IsMoonwalking(FighterState p)
        {
            return p.Action == FighterAction.Dash && Fixed.Sign(p.Velocity.X) == -p.Facing;   // ◀ Dashing, but moving the opposite way to facing
        }
    }
}
```

**Note:** the "Action ... (frame N)" line shows how many ticks the current action has lasted. The attack line shows the frame number from the attack's frame data (frame 1 = first frame).

### 3.28 `Scripts/Game/View/SnapshotDebugger.cs`

**What it is:** the F5 / F9 / F10 debug keys that prove the game is rollback-ready. The message line at the bottom of the screen comes from here. Debug only.

```csharp
using System.Collections.Generic;
using DreamsOfEverus.Simulation;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DreamsOfEverus.Game
{
    public class SnapshotDebugger : MonoBehaviour
    {
        [SerializeField] private SimulationRunner runner;

        private GameState savedSnapshot;                                     // ◀ The F5 snapshot (null until you press F5)
        private readonly List<InputFrame> inputsSinceSave = new List<InputFrame>();   // ◀ Every input since the snapshot, in order
        private string message = "F5 save snapshot   F9 load snapshot   F10 rollback check";
        private Color messageColor = Color.white;
        private GUIStyle style;

        private void OnEnable()
        {
            if (runner != null) runner.TickRan += RecordInput;   // ◀ "+=" = start listening to the runner's after-tick announcement
        }

        private void OnDisable()
        {
            if (runner != null) runner.TickRan -= RecordInput;   // ◀ Stop listening
        }

        private void RecordInput(InputFrame input)   // ◀ Called after every tick
        {
            if (savedSnapshot != null) inputsSinceSave.Add(input);   // ◀ Only record once there's a snapshot to replay from
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || runner == null || runner.State == null) return;

            if (keyboard.f5Key.wasPressedThisFrame) Save();
            if (keyboard.f9Key.wasPressedThisFrame) Load();
            if (keyboard.f10Key.wasPressedThisFrame) RollbackCheck();
        }

        private void Save()
        {
            savedSnapshot = runner.State.Clone();   // ◀ Copy the whole game
            inputsSinceSave.Clear();                // ◀ Start recording from here
            Show($"Snapshot saved at tick {savedSnapshot.Tick}.", Color.white);
        }

        private void Load()
        {
            if (savedSnapshot == null)
            {
                Show("No snapshot yet. Press F5 first.", Color.yellow);
                return;
            }

            runner.RestoreState(savedSnapshot);     // ◀ Rewind the live game
            inputsSinceSave.Clear(); // the game is back at the snapshot, so nothing has happened since
            Show($"Snapshot loaded: back to tick {savedSnapshot.Tick}.", Color.white);
        }

        private void RollbackCheck()
        {
            if (savedSnapshot == null)
            {
                Show("No snapshot yet. Press F5 first.", Color.yellow);
                return;
            }

            GameState replayed = runner.Replay(savedSnapshot, inputsSinceSave);   // ◀ Rewind and replay, off to the side
            if (replayed.SameAs(runner.State))                                    // ◀ Does the replay match the live game exactly?
            {
                Show($"Rollback check MATCH: replayed {inputsSinceSave.Count} ticks from the snapshot and got exactly the same game.", Color.green);
            }
            else
            {
                // In the Editor, changing tuning values after saving will also cause this.
                Show($"Rollback check MISMATCH after {inputsSinceSave.Count} ticks. (Changed any values since saving?)", Color.red);
            }
        }

        private void Show(string text, Color color)
        {
            message = text;
            messageColor = color;
        }

        private void OnGUI()
        {
            style ??= new GUIStyle(GUI.skin.label) { fontSize = 18 };
            style.normal.textColor = messageColor;
            GUI.Label(new Rect(12, Screen.height - 40, 1200, 30), message, style);   // ◀ Bottom-left of the screen
        }
    }
}
```

### 3.29 `Scripts/Editor/MovementSandboxBuilder.cs`

**What it is:** the **Dreams of Everus → Build Movement Sandbox** menu item. It builds the whole test scene from scratch: room, Bun, dummy, camera, and every connection between them. It only exists in the Editor, never in the game. Rebuilding replaces the scene but keeps your data assets and their values.

```csharp
using System.IO;                               // ◀ File and folder paths
using System.Linq;                             // ◀ List helpers like .Any() and .ToList()
using DreamsOfEverus.Game;
using DreamsOfEverus.Simulation;
using UnityEditor;                             // ◀ Editor-only tools (menus, asset database...)
using UnityEditor.SceneManagement;             // ◀ Creating and saving scenes
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;                   // ◀ Blend modes for the see-through hitbox material

namespace DreamsOfEverus.EditorTools
{
    public static class MovementSandboxBuilder
    {
        private const string ScenePath = "Assets/Game/Scenes/MovementSandbox.unity";   // ◀ Where everything is saved
        private const string CharacterDataPath = "Assets/Game/Data/BunCharacterData.asset";
        private const string InputFeelPath = "Assets/Game/Data/InputFeel.asset";
        private const string LightAttackPath = "Assets/Game/Data/BunLightAttack.asset";
        private const string FollowCameraPath = "Assets/Game/Data/FollowCamera.asset";
        private const string ControlsPath = "Assets/Game/Input/GameplayControls.inputactions";
        private const string MaterialFolder = "Assets/Game/Art/Graybox";

        [MenuItem("Dreams of Everus/Build Movement Sandbox")]   // ◀ Adds the menu item at the top of the Unity window
        private static void BuildFromMenu()
        {
            if (File.Exists(ScenePath) && !EditorUtility.DisplayDialog(   // ◀ If the scene exists, ask before replacing it
                    "Rebuild Movement Sandbox?",
                    "This replaces the MovementSandbox scene, so any changes you made to the room will be lost.\n\nBun's character data is kept.",
                    "Rebuild", "Cancel"))
            {
                return;                        // ◀ You clicked Cancel
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;   // ◀ Offer to save whatever scene is open first

            Build();
        }

        public static void Build()             // ◀ Public so it can also be run from the command line (that's how Claude builds it)
        {
            // Make the new scene first: opening a scene unloads assets nothing uses yet,
            // which would empty any references to assets loaded before it.
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);   // ◀ A fresh scene with a camera and a light

            EnsureFolder("Assets/Game/Scenes");
            EnsureFolder("Assets/Game/Data");
            EnsureFolder(MaterialFolder);

            CharacterData bunData = LoadOrCreateAsset<CharacterData>(CharacterDataPath);   // ◀ Use the existing asset, or make one with defaults
            InputFeelData inputFeel = LoadOrCreateAsset<InputFeelData>(InputFeelPath);
            AttackData bunLightAttack = LoadOrCreateAsset<AttackData>(LightAttackPath);
            if (bunData.lightAttack == null)   // ◀ Only link the attack if Bun doesn't already have one
            {
                bunData.lightAttack = bunLightAttack;
                EditorUtility.SetDirty(bunData);   // ◀ "Dirty" = "this asset changed, please save it"
            }
            var controls = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ControlsPath);

            Material solid = LoadOrCreateMaterial("Graybox_Solid", new Color(0.55f, 0.55f, 0.58f));             // ◀ Gray
            Material passThrough = LoadOrCreateMaterial("Graybox_PassThrough", new Color(0.35f, 0.6f, 0.9f));   // ◀ Blue
            Material fallZone = LoadOrCreateMaterial("Graybox_FallZone", new Color(0.6f, 0.12f, 0.12f));         // ◀ Dark red
            Material bunMaterial = LoadOrCreateMaterial("Placeholder_Bun", new Color(1f, 0.72f, 0.3f));          // ◀ Orange
            Material dummyMaterial = LoadOrCreateMaterial("Placeholder_Dummy", new Color(0.85f, 0.85f, 0.8f));   // ◀ Off-white
            Material hitboxMaterial = LoadOrCreateSeeThroughMaterial("Debug_Hitbox", new Color(1f, 0.1f, 0.1f, 0.45f));   // ◀ See-through red

            if (controls == null) Debug.LogError($"Movement Sandbox: controls not found at {ControlsPath}");

            // ----- Room (PLACEHOLDER layout, in world units) -----
            var level = new GameObject("Level").transform;   // ◀ An empty parent object to keep the Hierarchy tidy

            // Floor in two pieces with a 4-unit gap between x = -1 and x = 3. Floor top is y = 0.
            CreateBox("Floor Left", level, new Vector2(-7.5f, -0.5f), new Vector2(13f, 1f), LevelBlockKind.Solid, solid);
                                               // ◀ (name, parent, center, size, kind, material)
            CreateBox("Floor Right", level, new Vector2(8.5f, -0.5f), new Vector2(11f, 1f), LevelBlockKind.Solid, solid);

            CreateBox("Wall Left", level, new Vector2(-14.5f, 6f), new Vector2(1f, 14f), LevelBlockKind.Solid, solid);
            CreateBox("Wall Right", level, new Vector2(14.5f, 6f), new Vector2(1f, 14f), LevelBlockKind.Solid, solid);

            CreateBox("Platform Low (Solid)", level, new Vector2(-9f, 2.25f), new Vector2(4f, 0.5f), LevelBlockKind.Solid, solid);
            CreateBox("Platform Mid (Pass Through)", level, new Vector2(-3f, 4f), new Vector2(4f, 0.3f), LevelBlockKind.PassThrough, passThrough);
            CreateBox("Platform Mid (Pass Through) 2", level, new Vector2(5f, 3f), new Vector2(4f, 0.3f), LevelBlockKind.PassThrough, passThrough);
            CreateBox("Platform High (Solid)", level, new Vector2(10f, 6.25f), new Vector2(4f, 0.5f), LevelBlockKind.Solid, solid);

            // Crawl tunnel: a low solid ceiling 1.3 units above the floor. Too low to stand under, high enough to crawl.
            CreateBox("Crawl Tunnel Ceiling", level, new Vector2(10.5f, 2.15f), new Vector2(4f, 1.7f), LevelBlockKind.Solid, solid);

            // Below the whole room, so anything that drops through the gap lands in it.
            CreateBox("Fall Zone", level, new Vector2(0f, -9f), new Vector2(32f, 4f), LevelBlockKind.FallZone, fallZone);

            var respawn = new GameObject("Respawn Point", typeof(RespawnPoint));
            respawn.transform.position = new Vector3(-6f, 0f, 0f);   // ◀ On the left floor

            // ----- Simulation -----
            var simulationObject = new GameObject("Simulation", typeof(SimulationRunner), typeof(PlayerInputReader), typeof(DebugHud), typeof(SnapshotDebugger));
                                               // ◀ One object holding the runner, the input reader and the debug tools
            var runner = simulationObject.GetComponent<SimulationRunner>();
            SetField(runner, "playerCharacter", bunData);   // ◀ Fill in Inspector fields from code (see SetField below)
            SetField(runner, "inputFeel", inputFeel);
            SetField(simulationObject.GetComponent<DebugHud>(), "runner", runner);
            SetField(simulationObject.GetComponent<SnapshotDebugger>(), "runner", runner);
            SetField(simulationObject.GetComponent<PlayerInputReader>(), "controls", controls);
            SetField(simulationObject.GetComponent<PlayerInputReader>(), "inputFeel", inputFeel);

            // ----- Bun (placeholder capsule) -----
            var bun = new GameObject("Bun", typeof(FighterView));
            bun.transform.position = respawn.transform.position;

            // Visual turns to show facing; body and nose turn with it.
            var visual = new GameObject("Visual").transform;
            visual.SetParent(bun.transform, false);   // ◀ Make it a child of Bun ("false" = keep local position at zero)

            GameObject body = CreateVisualPrimitive(PrimitiveType.Capsule, "Body (Placeholder)", visual, bunMaterial);
            body.transform.localScale = new Vector3(bunData.bodyWidth, bunData.bodyHeight * 0.5f, bunData.bodyWidth);
            body.transform.localPosition = new Vector3(0f, bunData.bodyHeight * 0.5f, 0f);

            // A small block on the front of the face, so you can see which way Bun is facing.
            GameObject nose = CreateVisualPrimitive(PrimitiveType.Cube, "Nose (Facing)", visual, solid);
            nose.transform.localScale = new Vector3(0.3f, 0.2f, 0.2f);
            nose.transform.localPosition = new Vector3(bunData.bodyWidth * 0.5f, bunData.bodyHeight * 0.75f, 0f);

            var view = bun.GetComponent<FighterView>();
            SetField(view, "runner", runner);
            SetField(view, "visualRoot", visual);
            SetField(view, "body", body.transform);
            SetField(view, "nose", nose.transform);

            // ----- Training dummy (stands to Bun's right, within a few steps) -----
            var dummy = new GameObject("Training Dummy", typeof(TrainingDummy), typeof(TrainingDummyView));
            dummy.transform.position = new Vector3(-3f, 0f, 0f);
            var dummyData = dummy.GetComponent<TrainingDummy>();

            GameObject dummyBody = CreateVisualPrimitive(PrimitiveType.Cube, "Body (Placeholder)", dummy.transform, dummyMaterial);
            dummyBody.transform.localScale = new Vector3(dummyData.hurtboxWidth, dummyData.hurtboxHeight, 0.6f);   // ◀ Same size as its hurtbox
            dummyBody.transform.localPosition = new Vector3(0f, dummyData.hurtboxHeight * 0.5f, 0f);

            var dummyView = dummy.GetComponent<TrainingDummyView>();
            SetField(dummyView, "runner", runner);
            SetField(dummyView, "body", dummyBody.transform);
            SetField(dummyView, "bodyRenderer", dummyBody.GetComponent<Renderer>());

            // ----- Hitbox display (red, see-through, only while a hitbox is out) -----
            var hitboxDisplay = new GameObject("Hitbox Display", typeof(HitboxView));
            GameObject hitboxBox = CreateVisualPrimitive(PrimitiveType.Cube, "Hitbox", hitboxDisplay.transform, hitboxMaterial);
            SetField(hitboxDisplay.GetComponent<HitboxView>(), "runner", runner);
            SetField(hitboxDisplay.GetComponent<HitboxView>(), "box", hitboxBox.GetComponent<Renderer>());

            // ----- Camera: side-on perspective, follows Bun -----
            GameObject cameraObject = GameObject.FindWithTag("MainCamera");   // ◀ The camera the new scene came with
            if (cameraObject != null)
            {
                FollowCameraData cameraSettings = LoadOrCreateAsset<FollowCameraData>(FollowCameraPath);
                var follow = cameraObject.AddComponent<FollowCamera>();
                SetField(follow, "runner", runner);
                SetField(follow, "target", bun.transform);
                SetField(follow, "settings", cameraSettings);

                // Start where the camera will be when play begins, so the Scene and Game views match.
                cameraObject.transform.SetPositionAndRotation(
                    FollowCamera.DesiredPosition(bun.transform.position, cameraSettings.lookAheadDistance, cameraSettings),
                    Quaternion.Euler(cameraSettings.pitch, 0f, 0f));
                cameraObject.GetComponent<Camera>().fieldOfView = cameraSettings.fieldOfView;
            }

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);   // ◀ Save the scene file
            AddSceneToBuildSettings();         // ◀ So a built game would include it
            AssetDatabase.SaveAssets();        // ◀ Save any changed assets (like Bun's new attack link)

            Debug.Log($"Movement Sandbox built: {ScenePath}");
        }

        private static GameObject CreateBox(string name, Transform parent, Vector2 center, Vector2 size, LevelBlockKind kind, Material material)
        {                                      // ◀ Makes one level cube with a Level Box component
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            Object.DestroyImmediate(box.GetComponent<Collider>()); // collision comes from LevelBox, not Unity physics
            box.GetComponent<Renderer>().sharedMaterial = material;
            box.transform.SetParent(parent, false);
            box.transform.position = new Vector3(center.x, center.y, 0f);
            box.transform.localScale = new Vector3(size.x, size.y, 2f); // depth is visual only

            var levelBox = box.AddComponent<LevelBox>();
            var serialized = new SerializedObject(levelBox);              // ◀ The Inspector's view of the component
            serialized.FindProperty("kind").enumValueIndex = (int)kind;   // ◀ Set the Kind dropdown
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return box;
        }

        private static void SetField(Object target, string fieldName, Object value)
        {                                      // ◀ Fills in a private [SerializeField], exactly like dragging something into the Inspector
            var serialized = new SerializedObject(target);
            serialized.FindProperty(fieldName).objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject CreateVisualPrimitive(PrimitiveType type, string name, Transform parent, Material material)
        {                                      // ◀ A shape that's only for looks (no collider)
            GameObject shape = GameObject.CreatePrimitive(type);
            shape.name = name;
            Object.DestroyImmediate(shape.GetComponent<Collider>()); // no Unity physics in gameplay
            shape.GetComponent<Renderer>().sharedMaterial = material;
            shape.transform.SetParent(parent, false);
            return shape;
        }

        private static T LoadOrCreateAsset<T>(string path) where T : ScriptableObject
        {                                      // ◀ "T" = works for any data asset type
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;   // ◀ Already exists: keep it (and your tuned values)

            asset = ScriptableObject.CreateInstance<T>();   // ◀ Otherwise make a new one with the default values
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static Material LoadOrCreateSeeThroughMaterial(string name, Color color)
        {                                      // ◀ URP's settings for a transparent, unlit material, set from code
            string path = $"{MaterialFolder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;

            material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Surface", 1f); // Transparent
            material.SetFloat("_Blend", 0f);   // Alpha blending
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Transparent;   // ◀ Drawn after solid objects so you can see through it
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static Material LoadOrCreateMaterial(string name, Color color)
        {
            string path = $"{MaterialFolder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null) return material;

            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));   // ◀ URP's standard lit material
            material.SetColor("_BaseColor", color);
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void EnsureFolder(string path)
        {                                      // ◀ Create a folder (and any missing parent folders)
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);              // ◀ Make sure the parent exists first (this function calls itself)
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        private static void AddSceneToBuildSettings()
        {
            if (EditorBuildSettings.scenes.Any(s => s.path == ScenePath)) return;   // ◀ Already listed
            var scenes = EditorBuildSettings.scenes.ToList();
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));        // ◀ Put it first, so it's the one a build opens
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
```

**If you edit the room by hand** (moving cubes, adding platforms), don't run Build Movement Sandbox again, or your changes are replaced. To add a level piece by hand: create a cube (**GameObject → 3D Object → Cube**), remove its **Box Collider** component, add a **Level Box** component, and choose its **Kind**.

### 3.30 Setup files (not C# code)

| File | What it does |
|---|---|
| `Input/GameplayControls.inputactions` | The named input actions. **Move:** left stick, or A/S/D plus the arrow keys. **Jump:** Triangle/Y, Space or W. **Attack:** Square/X or J. **Walk:** Shift. Double-click it in Unity to view or remap. |
| `Scripts/Simulation/DreamsOfEverus.Simulation.asmdef` | Makes the gameplay code its own package with `"noEngineReferences": true`, which switches Unity off for it. That's what guarantees gameplay never uses Unity physics or time. |
| `Scripts/Game/DreamsOfEverus.Game.asmdef` | The Unity-side package. It can use the simulation and the Input System. |
| `Scripts/Editor/DreamsOfEverus.Editor.asmdef` | Editor-only package (`"includePlatforms": ["Editor"]`), so the builder is never part of the game. |
| `Tests/EditMode/DreamsOfEverus.Tests.EditMode.asmdef` | The test package. It uses NUnit, the testing library built into Unity. |
| `.meta` files | Unity creates one for every file and folder. They hold the ID other things use to link to it. Always commit them with their files. |

---

## Part 4. The tests

The tests are small programs that check the game's rules automatically. There are 98 of them. To run them, open **Window → General → Test Runner**, choose the **EditMode** tab and click **Run All**. Claude runs them from the command line after every change, with Unity closed.

| Test file | What it checks |
|---|---|
| `FixedTests.cs` | The fixed-point math: adding, multiplying, dividing, fractions, negatives, rounding, `MoveTowards`. |
| `SimulationTests.cs` | Ticks count up; the same inputs give the same game; copies are independent; rewind-and-replay matches. |
| `InputTests.cs` | Stick rounding and clamping, the walk key, the button latch keeping quick taps, and the controls asset having the right actions and buttons. |
| `CollisionTests.cs` | Landing, walls on both sides, ceilings, jumping up through and landing on pass-through platforms, dropping through (not from solid ground), fast falls not skipping platforms, walking off ledges, fall zone respawn. |
| `MovementTests.cs` | Walk speed follows tilt; hard press dashes, slow tilt walks; trot length; dash dance; run turnaround; moonwalk, including the half-roll and per-character strength; traction; jump squat, short hop, air jump, landing; jump momentum; fast-fall; drop-through; crouch, crawl, low ceilings. |
| `CombatTests.cs` | Attack startup, active and recovery frames; one hit per attack; facing and range; hitbox flipping; dummy reaction; no air attack; attack commitment; sliding while attacking; the shown hitbox matching the frames that can hit; replaying hits exactly. |
| `FollowCameraTests.cs` | The camera's position math: distance, height, look-ahead side, pitch. |
| `TestWorld.cs` | Not tests: shared helpers. A test fighter with simple numbers, a test dummy, `Box(...)` for level pieces, `In(...)` for inputs. |

**How to read a test.** Here's one, annotated:

```csharp
[Test]                                         // ◀ Marks this function as a test for the Test Runner
public void DashDance_OnePressBackDuringTrot_DashesTheOtherWay()   // ◀ The name says what should happen
{
    HardPress(1);                              // ◀ ARRANGE: flick right → Bun dashes right
    Hold(In(100), 3);                          // ◀ Keep holding right for 3 ticks (still inside the trot)
    Hold(In(-100));                            // ◀ ACT: one hard press left

    Assert.AreEqual(FighterAction.Dash, Player.Action);       // ◀ ASSERT: still dashing...
    Assert.AreEqual(-1, Player.Facing);                       // ◀ ...now facing left...
    Assert.AreEqual(-fighter.DashSpeed, Player.Velocity.X);   // ◀ ...at full dash speed leftward
}                                              // ◀ If any Assert is wrong, the test fails and says which value was off
```

Tests use their own simple numbers from `TestWorld.Fighter()`, not Bun's real values, so tuning Bun in the Inspector never breaks a test.

---

## Part 5. How do I...

**...tune Bun's movement?** Select `Assets/Game/Data/BunCharacterData` and change values in the Inspector. You can do it during Play: changes apply instantly and are kept when you stop, because they live in the asset rather than the scene.

**...make a second character?** In the Project window, right-click → **Create → Dreams of Everus → Character Data**. Name it, set its values, and give it its own attack (**Create → Dreams of Everus → Attack Data**, then drag it into **Light Attack**). For now the sandbox only plays one character: select the **Simulation** object and drag your new asset into **Player Character**.

**...add a new movement value?** This takes three steps in three files:
1. Add the friendly field to `CharacterData.cs`, with a `[Tooltip]` and a PLACEHOLDER default.
2. Add the fixed-point field to `FighterConfig.cs` and convert it in `CharacterData.ApplyTo`.
3. Use it in `FighterMovement.cs`, and add a test in `MovementTests.cs`.

**...add something that changes during play** (a timer, a counter, a new state)? Put it in `FighterState` (or `DummyState`) in `GameState.cs`. Anything that changes and isn't in there won't survive a rollback.

**...add a new action?** Add it to the `FighterAction` list in `GameState.cs`, then add an `Update...` function in `FighterMovement.cs`, a `case` for it in `UpdateGround` (or the air logic), and a way to enter it. Always change actions with `SetAction` so `ActionTick` resets.

**...change the room?** Either edit `CreateBox` lines in `MovementSandboxBuilder.cs` and rebuild, or move and resize cubes by hand in the Scene view and don't rebuild (see 3.29).

**...remap a button?** Double-click `Assets/Game/Input/GameplayControls`, pick the action, and change or add a binding. Keep the action names (Move, Jump, Attack, Walk) the same, because the code looks them up by name.

**...check rollback still works after a change?** Press Play, F5, play for a bit, then F10. You should see a green MATCH. The automatic tests check this too.

**Rules to keep (from CLAUDE.md):**
- Gameplay code uses `Fixed`, never `float`.
- Gameplay never reads `Time.deltaTime` or Unity physics.
- Visuals read the state and never change it.
- Every tuning number lives in a data asset.
- Hits come only from hitboxes and hurtboxes.
