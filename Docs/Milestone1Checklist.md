# Milestone 1 Test Checklist: Movement Sandbox V0.1

Work through this with a controller (PS4 or Xbox), then spot-check keyboard. Tick each box as it passes. If something fails, note what you did and what happened.

## Setup

- [ ] Open the project in Unity Hub. The Console (**Window → General → Console**) shows no red errors.
- [ ] Open `Assets/Game/Scenes/MovementSandbox` and press **Play**.
- [ ] The readout in the top-left shows `Action: Idle`, and the hint line at the bottom shows the F5 / F9 / F10 keys.
- [ ] Optional: **Window → General → Test Runner → EditMode → Run All**. All 98 tests pass.

## The room

- [ ] Gray floor with a gap in the middle, gray walls on both sides.
- [ ] Platforms at different heights: two gray (solid) and two blue (pass-through).
- [ ] Dark red fall zone below the gap. A low gray block on the right floor forms a crawl tunnel.
- [ ] The orange capsule (Bun) stands on the left floor, with its nose showing which way it faces. The gray dummy stands a few steps to the right.

## Ground movement

- [ ] **Walk:** ease the stick partway. Bun walks slowly (`Walk`). Further tilt walks faster.
- [ ] **Dash:** flick the stick hard to the side. `Dash` at a burst of speed.
- [ ] **Run:** flick and keep holding. After the trot, `Run`.
- [ ] **Dash dance:** flick back and forth quickly during the trot. Bun turns instantly each time.
- [ ] **Run turnaround:** while running, hold back. `RunTurnaround`: Bun skids, swings around slowly, then runs the other way.
- [ ] **Stop:** let go while running. Bun slides to a stop.
- [ ] **Moonwalk:** dash, then roll the stick down and around to the back without flicking. Bun slides backward while still facing forward, and the readout shows `MOONWALKING`.
- [ ] **No crouch while trotting:** during the dash, pressing Down does not crouch.
- [ ] **Crouch:** hold Down while standing. The capsule shrinks (`Crouch`). Let go and it stands back up.
- [ ] **Crawl:** hold Down plus a side. `Crawl`. Crawl into the tunnel on the right floor.
- [ ] **Low ceiling:** under the tunnel, let go of Down. Bun stays crouched, and Jump does nothing until Bun is out.
- [ ] **Blocked standing:** walk at the tunnel standing up. Bun stops at its edge.

## Jumping and air

- [ ] **Full jump:** hold Jump (Triangle). Bun crouches for a few frames (`JumpSquat`), then jumps high.
- [ ] **Short hop:** tap Jump quickly. A noticeably lower jump.
- [ ] **Air jump:** press Jump again in the air for one more jump. A third press does nothing.
- [ ] **Landing:** the air jump comes back after landing (`Air jumps used: 0`).
- [ ] **Momentum:** a dash or run jump travels clearly further than a walk jump.
- [ ] **Air steering:** you can drift left and right in the air.
- [ ] **Fast-fall:** after the top of a jump, flick Down. `Fast-falling: yes` and Bun drops faster.
- [ ] **Platforms:** jump up through a blue platform from below and land on top. Gray platforms block from below.
- [ ] **Drop through:** standing on a blue platform, flick Down. Bun drops through. Holding Down slowly just crouches.

## Attack and dummy

- [ ] Walk up to the dummy and press **Square**. A see-through red box flashes in front of Bun.
- [ ] The dummy flashes red and shakes. `Dummy hits` goes up by 1.
- [ ] The readout shows the attack's frames, for example `frame 4 / 18 (HITBOX OUT)  HIT`. The red box is visible exactly on the HITBOX OUT frames.
- [ ] Holding Square doesn't repeat the attack. Each press is one attack and at most one hit.
- [ ] Facing away or standing too far away misses.
- [ ] Dash into the dummy and attack: Bun slides into the hit.
- [ ] **F2** hides and shows the hitbox display.

## Fall and respawn

- [ ] Walk or drop into the gap. Bun touches the red zone and reappears at the respawn point on the left floor.
- [ ] The camera snaps back to Bun instead of sweeping across the room.

## Camera

- [ ] Side-on perspective that follows Bun smoothly, leaning slightly ahead of the way Bun faces.
- [ ] Jumping doesn't jerk the view. Dash dancing doesn't whip the camera back and forth.

## Tuning (the Inspector, during Play)

- [ ] `Assets/Game/Data/BunCharacterData`: change **Gravity** or **Dash Speed**. The change is felt immediately.
- [ ] Change **Traction** to something low like 10. Bun slides much further when stopping.
- [ ] Change **Moonwalk Acceleration** to 0 (no moonwalk), then to 300 (strong moonwalk).
- [ ] `Assets/Game/Data/BunLightAttack`: change **Startup Frames** or **Hitbox Size**. The attack changes immediately.
- [ ] `Assets/Game/Data/FollowCamera`: change **Distance** or **Field Of View**. The camera changes immediately.
- [ ] Stop Play. The values you changed in the data assets are still there.

## Keyboard

- [ ] **A / D** (or the arrow keys) dash and run. **Shift + A / D** walks.
- [ ] **Space** or **W** jumps, **J** attacks, **S** crouches and fast-falls.
- [ ] **Shift + S + D** crawls.
- [ ] Moonwalk: dash with D, then hold Shift and press A.

## Rollback proof

- [ ] Press **F5**: "Snapshot saved at tick ...".
- [ ] Move, jump, attack and moonwalk for a few seconds, then press **F10**: a green **MATCH**.
- [ ] Press **F9**: Bun snaps back to exactly where they were when you pressed F5.
- [ ] Press F5 mid-jump or mid-attack, then F9 later. Bun resumes from that exact moment.

## Done when (from the design doc)

- [ ] You press Play, move and jump between platforms with a controller, throw a light attack that visibly hits the dummy, fall, respawn, and tune values.
