namespace DreamsOfEverus.Simulation
{
    /// <summary>
    /// The movement rules: walking, dashing, trot and dash dance, running and turning around,
    /// crouching and crawling, jump squat, short hop, air jump, air control, gravity, fast-fall and drop-through.
    /// Each tick: UpdateBeforeMove sets the velocity, the KinematicMover moves the fighter,
    /// then UpdateAfterMove handles landing and walking off ledges.
    /// </summary>
    public static class FighterMovement
    {
        public static void UpdateBeforeMove(ref FighterState fighter, in InputEvents input, FighterConfig config, LevelGeometry level)
        {
            if (fighter.Action == FighterAction.Airborne) UpdateAir(ref fighter, input, config);
            else UpdateGround(ref fighter, input, config, level);
        }

        public static void UpdateAfterMove(ref FighterState fighter)
        {
            fighter.ActionTick++;

            bool inAir = fighter.Action == FighterAction.Airborne;
            if (inAir && fighter.IsGrounded && fighter.Velocity.Y <= Fixed.Zero)
            {
                Land(ref fighter);
            }
            else if (!inAir && !fighter.IsGrounded)
            {
                SetAction(ref fighter, FighterAction.Airborne); // walked or slid off a ledge
            }
        }

        /// <summary>Puts the fighter in the right action after being placed somewhere (start of play, respawn).</summary>
        public static void ResetAfterTeleport(ref FighterState fighter)
        {
            fighter.Velocity = FixedVector2.Zero;
            fighter.AirJumpsUsed = 0;
            fighter.IsFastFalling = false;
            SetAction(ref fighter, fighter.IsGrounded ? FighterAction.Idle : FighterAction.Airborne);
        }

        // ----- Ground -----

        private static void UpdateGround(ref FighterState fighter, in InputEvents input, FighterConfig config, LevelGeometry level)
        {
            fighter.Velocity.Y = Fixed.Zero;

            // Under a low ceiling the fighter has to stay crouched: no standing up, no jumping.
            bool roomToStand = KinematicMover.HasRoomToStand(fighter, config, level);

            // Jumping works from any ground action except jump squat itself.
            // Dropping through and crouching don't work during the trot: rolling the stick through Down
            // mid-dash is how you moonwalk, so Down is ignored there.
            bool trotting = fighter.Action == FighterAction.Dash;
            if (fighter.Action != FighterAction.JumpSquat)
            {
                if (input.JumpPressed && roomToStand)
                {
                    SetAction(ref fighter, FighterAction.JumpSquat);
                    fighter.ShortHopQueued = false;
                }
                else if (!trotting && input.HardPressDown && KinematicMover.TryDropThrough(ref fighter))
                {
                    SetAction(ref fighter, FighterAction.Airborne);
                    return;
                }
                else if (!trotting && input.HeldDown && !fighter.IsLow)
                {
                    SetAction(ref fighter, FighterAction.Crouch);
                }
            }

            switch (fighter.Action)
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
            }
        }

        private static void UpdateIdleOrWalk(ref FighterState fighter, in InputEvents input, FighterConfig config)
        {
            // A hard sideways press dashes (like a smash input). A gentle tilt walks.
            if (input.HardPressX != 0)
            {
                StartDash(ref fighter, input.HardPressX, config);
                return;
            }

            if (input.HeldDirectionX == 0)
            {
                if (fighter.Action != FighterAction.Idle) SetAction(ref fighter, FighterAction.Idle);
                fighter.Velocity.X = Fixed.MoveTowards(fighter.Velocity.X, Fixed.Zero, config.Traction);
                return;
            }

            if (fighter.Action != FighterAction.Walk) SetAction(ref fighter, FighterAction.Walk);
            fighter.Facing = (sbyte)input.HeldDirectionX;

            // Walk speed follows how far the stick is tilted: half tilt = half speed.
            Fixed targetSpeed = config.WalkSpeed * input.StickX / InputFrame.StickMax;
            fighter.Velocity.X = GroundSteer(fighter.Velocity.X, targetSpeed, input.HeldDirectionX, config);
        }

        private static void StartDash(ref FighterState fighter, int direction, FighterConfig config)
        {
            fighter.Facing = (sbyte)direction;
            SetAction(ref fighter, FighterAction.Dash);
            fighter.Velocity.X = config.DashSpeed * direction;
        }

        private static void UpdateDash(ref FighterState fighter, in InputEvents input, FighterConfig config)
        {
            // Dash dance: a hard press the other way during the trot instantly dashes that way.
            if (input.HardPressX == -fighter.Facing)
            {
                StartDash(ref fighter, input.HardPressX, config);
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
                Fixed backwardTarget = config.DashSpeed * input.StickX / InputFrame.StickMax;
                fighter.Velocity.X = Fixed.MoveTowards(fighter.Velocity.X, backwardTarget, config.MoonwalkAcceleration);
            }
            else
            {
                // Stick let go during the trot: slide to a stop on the character's traction.
                fighter.Velocity.X = Fixed.MoveTowards(fighter.Velocity.X, Fixed.Zero, config.Traction);
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

            if (input.HeldDirectionX == 0)
            {
                SetAction(ref fighter, FighterAction.Idle);
                fighter.Velocity.X = Fixed.MoveTowards(fighter.Velocity.X, Fixed.Zero, config.Traction);
                return;
            }

            fighter.Velocity.X = GroundSteer(fighter.Velocity.X, config.RunSpeed * fighter.Facing, fighter.Facing, config);
        }

        private static void UpdateRunTurnaround(ref FighterState fighter, in InputEvents input, FighterConfig config)
        {
            fighter.Velocity.X = Fixed.MoveTowards(fighter.Velocity.X, Fixed.Zero, config.Traction);

            if (fighter.ActionTick >= config.RunTurnaroundTicks)
            {
                fighter.Facing = (sbyte)-fighter.Facing;
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

            if (config.CanCrawl && input.HeldDirectionX != 0)
            {
                if (fighter.Action != FighterAction.Crawl) SetAction(ref fighter, FighterAction.Crawl);
                fighter.Facing = (sbyte)input.HeldDirectionX;

                Fixed targetSpeed = config.CrawlSpeed * input.StickX / InputFrame.StickMax;
                fighter.Velocity.X = GroundSteer(fighter.Velocity.X, targetSpeed, input.HeldDirectionX, config);
            }
            else
            {
                if (fighter.Action != FighterAction.Crouch) SetAction(ref fighter, FighterAction.Crouch);
                fighter.Velocity.X = Fixed.MoveTowards(fighter.Velocity.X, Fixed.Zero, config.Traction);
            }
        }

        private static void UpdateJumpSquat(ref FighterState fighter, in InputEvents input, FighterConfig config)
        {
            // Smash-style short hop: let go of Jump before the squat ends.
            if (!input.JumpHeld) fighter.ShortHopQueued = true;

            if (fighter.ActionTick >= config.JumpSquatTicks)
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
            if (input.JumpPressed && fighter.AirJumpsUsed < config.AirJumps)
            {
                fighter.AirJumpsUsed++;
                fighter.IsFastFalling = false;
                fighter.Velocity.Y = config.AirJumpVelocity;
            }
            else
            {
                // Fast-fall: a hard press of Down once already falling (at or past the top of the jump).
                if (!fighter.IsFastFalling && input.HardPressDown && fighter.Velocity.Y <= Fixed.Zero)
                {
                    fighter.IsFastFalling = true;
                }

                fighter.Velocity.Y = fighter.IsFastFalling
                    ? -config.FastFallSpeed
                    : Fixed.Max(fighter.Velocity.Y - config.Gravity, -config.MaxFallSpeed);
            }

            UpdateAirControl(ref fighter, input, config);
        }

        /// <summary>
        /// Steering in the air. Holding a direction speeds you up toward air speed, but never slows down
        /// momentum you already have in that direction (from a dash or run jump); that only fades with air friction.
        /// Holding the other way brakes and turns you around.
        /// </summary>
        private static void UpdateAirControl(ref FighterState fighter, in InputEvents input, FighterConfig config)
        {
            if (input.HeldDirectionX == 0)
            {
                fighter.Velocity.X = Fixed.MoveTowards(fighter.Velocity.X, Fixed.Zero, config.AirFriction);
                return;
            }

            Fixed targetSpeed = config.AirSpeed * input.StickX / InputFrame.StickMax;
            bool alreadyFaster = input.HeldDirectionX > 0
                ? fighter.Velocity.X > targetSpeed
                : fighter.Velocity.X < targetSpeed;

            Fixed change = alreadyFaster ? config.AirFriction : config.AirAcceleration;
            fighter.Velocity.X = Fixed.MoveTowards(fighter.Velocity.X, targetSpeed, change);
        }

        /// <summary>
        /// Ground steering toward a target speed. Speeding up uses ground acceleration, but if the fighter is
        /// already going faster than the target in that direction (e.g. landing from a dash jump), the extra
        /// speed bleeds off on traction, so slippery characters slide further.
        /// </summary>
        private static Fixed GroundSteer(Fixed velocity, Fixed target, int direction, FighterConfig config)
        {
            bool alreadyFaster = direction > 0 ? velocity > target : velocity < target;
            return Fixed.MoveTowards(velocity, target, alreadyFaster ? config.Traction : config.GroundAcceleration);
        }

        private static void Land(ref FighterState fighter)
        {
            fighter.AirJumpsUsed = 0;
            fighter.IsFastFalling = false;
            SetAction(ref fighter, FighterAction.Idle);
        }

        private static void SetAction(ref FighterState fighter, FighterAction action)
        {
            fighter.Action = action;
            fighter.ActionTick = 0;
        }
    }
}
