namespace DreamsOfEverus.Simulation
{
    /// <summary>
    /// Moves a fighter by its velocity and stops it at level boxes. No Unity physics.
    /// Moves sideways first, then up/down. Each move is checked along its whole path
    /// (not just the end point), so a fast fighter can't skip through a thin platform.
    /// </summary>
    public static class KinematicMover
    {
        /// <summary>How far below a platform's top the feet are placed when dropping through (1/256 unit, invisible).</summary>
        private static readonly Fixed DropThroughNudge = Fixed.FromFraction(1, 256);

        /// <summary>The fighter's collision box right now (shorter while crouching or crawling).</summary>
        public static FixedBox BodyBox(in FighterState fighter, FighterConfig config)
        {
            return BoxAt(fighter.Position, config.HalfWidth, config.HeightFor(fighter));
        }

        /// <summary>False when something solid is in the way of standing up (e.g. crawling under a low ceiling).</summary>
        public static bool HasRoomToStand(in FighterState fighter, FighterConfig config, LevelGeometry level)
        {
            FixedBox standing = BoxAt(fighter.Position, config.HalfWidth, config.BodyHeight);
            foreach (LevelBlock block in level.Blocks)
            {
                if (block.Kind == LevelBlockKind.Solid && block.Box.Overlaps(standing)) return false;
            }
            return true;
        }

        private static FixedBox BoxAt(FixedVector2 feet, Fixed halfWidth, Fixed height)
        {
            return new FixedBox(
                new FixedVector2(feet.X - halfWidth, feet.Y),
                new FixedVector2(feet.X + halfWidth, feet.Y + height));
        }

        public static void Move(ref FighterState fighter, FighterConfig config, LevelGeometry level)
        {
            MoveHorizontal(ref fighter, config, level);
            MoveVertical(ref fighter, config, level);
            UpdateGround(ref fighter, config, level);
        }

        /// <summary>
        /// Drops through the pass-through platform the fighter stands on.
        /// Returns false (and does nothing) when standing on solid ground or in the air.
        /// </summary>
        public static bool TryDropThrough(ref FighterState fighter)
        {
            if (fighter.Ground != GroundType.PassThrough) return false;

            // Putting the feet just under the top means the platform no longer counts as "below" them,
            // so it won't catch the fighter while falling.
            fighter.Position.Y -= DropThroughNudge;
            fighter.Ground = GroundType.None;
            return true;
        }

        /// <summary>Works out what the fighter is standing on, if anything.</summary>
        public static void UpdateGround(ref FighterState fighter, FighterConfig config, LevelGeometry level)
        {
            Fixed left = fighter.Position.X - config.HalfWidth;
            Fixed right = fighter.Position.X + config.HalfWidth;
            fighter.Ground = GroundType.None;

            foreach (LevelBlock block in level.Blocks)
            {
                if (block.Kind == LevelBlockKind.FallZone) continue;
                if (block.Box.Max.Y != fighter.Position.Y) continue;
                if (!(left < block.Box.Max.X && right > block.Box.Min.X)) continue;

                // Solid ground wins: you can't drop through if one foot is on something solid.
                if (block.Kind == LevelBlockKind.Solid) fighter.Ground = GroundType.Solid;
                else if (fighter.Ground == GroundType.None) fighter.Ground = GroundType.PassThrough;
            }
        }

        private static void MoveHorizontal(ref FighterState fighter, FighterConfig config, LevelGeometry level)
        {
            Fixed velocityX = fighter.Velocity.X;
            if (velocityX == Fixed.Zero) return;

            Fixed half = config.HalfWidth;
            Fixed bottom = fighter.Position.Y;
            Fixed top = bottom + config.HeightFor(fighter);
            Fixed oldX = fighter.Position.X;
            Fixed targetX = oldX + velocityX;
            bool blocked = false;

            foreach (LevelBlock block in level.Blocks)
            {
                if (block.Kind != LevelBlockKind.Solid) continue; // only solids block sideways
                FixedBox box = block.Box;
                if (!(bottom < box.Max.Y && top > box.Min.Y)) continue; // not at the same height

                if (velocityX > Fixed.Zero)
                {
                    // Right edge was left of the wall, and would end up past it.
                    if (oldX + half <= box.Min.X && targetX + half > box.Min.X)
                    {
                        targetX = box.Min.X - half;
                        blocked = true;
                    }
                }
                else if (oldX - half >= box.Max.X && targetX - half < box.Max.X)
                {
                    targetX = box.Max.X + half;
                    blocked = true;
                }
            }

            fighter.Position.X = targetX;
            if (blocked) fighter.Velocity.X = Fixed.Zero;
        }

        private static void MoveVertical(ref FighterState fighter, FighterConfig config, LevelGeometry level)
        {
            Fixed velocityY = fighter.Velocity.Y;
            if (velocityY == Fixed.Zero) return;

            Fixed left = fighter.Position.X - config.HalfWidth;
            Fixed right = fighter.Position.X + config.HalfWidth;
            Fixed oldY = fighter.Position.Y;
            Fixed targetY = oldY + velocityY;
            Fixed height = config.HeightFor(fighter);
            bool blocked = false;

            foreach (LevelBlock block in level.Blocks)
            {
                if (block.Kind == LevelBlockKind.FallZone) continue;
                FixedBox box = block.Box;
                if (!(left < box.Max.X && right > box.Min.X)) continue; // not above or below this box

                if (velocityY < Fixed.Zero)
                {
                    // Falling: land on top of solids AND pass-through platforms,
                    // but only if the feet started at or above the top.
                    if (oldY >= box.Max.Y && targetY < box.Max.Y)
                    {
                        targetY = box.Max.Y;
                        blocked = true;
                    }
                }
                else if (block.Kind == LevelBlockKind.Solid)
                {
                    // Rising: only solids stop the head. Pass-through platforms let you jump up through.
                    Fixed oldTop = oldY + height;
                    if (oldTop <= box.Min.Y && targetY + height > box.Min.Y)
                    {
                        targetY = box.Min.Y - height;
                        blocked = true;
                    }
                }
            }

            fighter.Position.Y = targetY;
            if (blocked) fighter.Velocity.Y = Fixed.Zero;
        }
    }
}
