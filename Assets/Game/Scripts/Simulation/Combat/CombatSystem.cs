namespace DreamsOfEverus.Simulation
{
    /// <summary>
    /// Hitboxes against hurtboxes. Hits come only from these boxes, never from a model's shape.
    /// For now the only target is the training dummy, and a hit just makes it react (no damage or knockback yet).
    /// </summary>
    public static class CombatSystem
    {
        /// <summary>
        /// Gives the attacker's hitbox in world space if their attack has a hitbox out on this frame.
        /// The offset is mirrored when facing left.
        /// </summary>
        public static bool TryGetActiveHitbox(in FighterState attacker, FighterConfig config, out FixedBox hitbox)
        {
            hitbox = default;
            AttackConfig attack = config.LightAttack;
            if (attacker.Action != FighterAction.LightAttack || attack == null) return false;
            if (!attack.IsActiveOn(attacker.ActionTick)) return false;

            Fixed centerX = attacker.Position.X + attack.HitboxOffset.X * attacker.Facing;
            Fixed centerY = attacker.Position.Y + attack.HitboxOffset.Y;
            Fixed halfWidth = attack.HitboxSize.X / 2;
            Fixed halfHeight = attack.HitboxSize.Y / 2;

            hitbox = new FixedBox(
                new FixedVector2(centerX - halfWidth, centerY - halfHeight),
                new FixedVector2(centerX + halfWidth, centerY + halfHeight));
            return true;
        }

        public static FixedBox DummyHurtbox(in DummyState dummy, TrainingDummyConfig config)
        {
            Fixed halfWidth = config.HurtboxWidth / 2;
            return new FixedBox(
                new FixedVector2(dummy.Position.X - halfWidth, dummy.Position.Y),
                new FixedVector2(dummy.Position.X + halfWidth, dummy.Position.Y + config.HurtboxHeight));
        }

        /// <summary>Counts down the dummy's reaction, then checks the attacker's hitbox against it.</summary>
        public static void Step(ref FighterState attacker, FighterConfig attackerConfig, ref DummyState dummy, TrainingDummyConfig dummyConfig)
        {
            if (!dummy.Exists || dummyConfig == null) return;

            if (dummy.HitReactTicksLeft > 0) dummy.HitReactTicksLeft--;

            // One hit per attack: once it connects, the rest of its active frames can't hit again.
            if (attacker.AttackHasHit) return;
            if (!TryGetActiveHitbox(attacker, attackerConfig, out FixedBox hitbox)) return;

            if (hitbox.Overlaps(DummyHurtbox(dummy, dummyConfig)))
            {
                attacker.AttackHasHit = true;
                dummy.HitReactTicksLeft = dummyConfig.HitReactTicks;
                dummy.TimesHit++;
            }
        }
    }
}
