namespace DreamsOfEverus.Simulation
{
    /// <summary>
    /// The movement rules: walking, dashing, trot and dash dance, running and turning around,
    /// jump squat, short hop, air jump, air control, gravity, fast-fall and drop-through.
    /// Each tick: UpdateBeforeMove sets the velocity, the KinematicMover moves the fighter,
    /// then UpdateAfterMove handles landing and walking off ledges.
    /// </summary>
    public static class FighterMovement
    {
        public static void UpdateBeforeMove(ref FighterState fighter, in InputEvents input, FighterConfig config)
        {
            if (fighter.Action == FighterAction.Airborne) UpdateAir(ref fighter, input, config);
            else UpdateGround(ref fighter, input, config);
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

        private static void UpdateGround(ref FighterState fighter, in InputEvents input, FighterConfig config)
        {
            fighter.Velocity.Y = Fixed.Zero;

            // Jumping and dropping through work from any ground action except jump squat itself.
            if (fighter.Action != FighterAction.JumpSquat)
            {
                if (input.JumpPressed)
                {
                    SetAction(ref fighter, FighterAction.JumpSquat);
                    fighter.ShortHopQueued = false;
                }
                else if (input.HardPressDown && KinematicMover.TryDropThrough(ref fighter))
                {
                    SetAction(ref fighter, FighterAction.Airborne);
                    return;
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
                case FighterAction.JumpSquat:
                    UpdateJumpSquat(ref fighter, input, config);
                    break;
            }
        }

        private static void UpdateIdleOrWalk(ref FighterState fighter, in InputEvents input, FighterConfig config)
        {
            if (input.DoubleTapX)
            {
                StartDash(ref fighter, input.PressedDirectionX, config);
                return;
            }

            if (input.HeldDirectionX == 0)
            {
                if (fighter.Action != FighterAction.Idle) SetAction(ref fighter, FighterAction.Idle);
                fighter.Velocity.X = Fixed.MoveTowards(fighter.Velocity.X, Fixed.Zero, config.GroundFriction);
                return;
            }

            if (fighter.Action != FighterAction.Walk) SetAction(ref fighter, FighterAction.Walk);
            fighter.Facing = (sbyte)input.HeldDirectionX;

            // Walk speed follows how far the stick is tilted: half tilt = half speed.
            Fixed targetSpeed = config.WalkSpeed * input.StickX / InputFrame.StickMax;
            fighter.Velocity.X = Fixed.MoveTowards(fighter.Velocity.X, targetSpeed, config.GroundAcceleration);
        }

        private static void StartDash(ref FighterState fighter, int direction, FighterConfig config)
        {
            fighter.Facing = (sbyte)direction;
            SetAction(ref fighter, FighterAction.Dash);
            fighter.Velocity.X = config.DashSpeed * direction;
        }

        private static void UpdateDash(ref FighterState fighter, in InputEvents input, FighterConfig config)
        {
            // Dash dance: one press the other way during the trot instantly dashes that way.
            if (input.PressedDirectionX == -fighter.Facing)
            {
                StartDash(ref fighter, input.PressedDirectionX, config);
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
                    fighter.Velocity.X = Fixed.MoveTowards(fighter.Velocity.X, Fixed.Zero, config.GroundFriction);
                }
                return;
            }

            fighter.Velocity.X = config.DashSpeed * fighter.Facing;
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
                fighter.Velocity.X = Fixed.MoveTowards(fighter.Velocity.X, Fixed.Zero, config.GroundFriction);
                return;
            }

            fighter.Velocity.X = Fixed.MoveTowards(fighter.Velocity.X, config.RunSpeed * fighter.Facing, config.GroundAcceleration);
        }

        private static void UpdateRunTurnaround(ref FighterState fighter, in InputEvents input, FighterConfig config)
        {
            fighter.Velocity.X = Fixed.MoveTowards(fighter.Velocity.X, Fixed.Zero, config.RunTurnaroundDeceleration);

            if (fighter.ActionTick >= config.RunTurnaroundTicks)
            {
                fighter.Facing = (sbyte)-fighter.Facing;
                SetAction(ref fighter, input.HeldDirectionX == fighter.Facing ? FighterAction.Run : FighterAction.Idle);
            }
        }

        private static void UpdateJumpSquat(ref FighterState fighter, in InputEvents input, FighterConfig config)
        {
            // Smash-style short hop: let go of Jump before the squat ends.
            if (!input.JumpHeld) fighter.ShortHopQueued = true;

            if (fighter.ActionTick >= config.JumpSquatTicks)
            {
                fighter.Velocity.Y = fighter.ShortHopQueued ? config.ShortHopVelocity : config.FullHopVelocity;
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

            // Air control: steer toward the stick's direction, or drift to a stop when it's centered.
            if (input.HeldDirectionX != 0)
            {
                Fixed targetSpeed = config.AirSpeed * input.StickX / InputFrame.StickMax;
                fighter.Velocity.X = Fixed.MoveTowards(fighter.Velocity.X, targetSpeed, config.AirAcceleration);
            }
            else
            {
                fighter.Velocity.X = Fixed.MoveTowards(fighter.Velocity.X, Fixed.Zero, config.AirFriction);
            }
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
