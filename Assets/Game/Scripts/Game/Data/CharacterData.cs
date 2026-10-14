using System;
using DreamsOfEverus.Simulation;
using UnityEngine;

namespace DreamsOfEverus.Game
{
    /// <summary>
    /// One character's tunable values, edited in the Inspector. Each character gets its own asset.
    /// Values are typed in friendly units (units per second, frames, heights) and converted to
    /// fixed-point per-tick values for the simulation. In the Editor, changes apply live during Play.
    /// The numbers below are only the defaults for a brand-new asset; the asset itself is what the game uses.
    /// ALL VALUES ARE PLACEHOLDERS until tuned by feel.
    /// </summary>
    [CreateAssetMenu(menuName = "Dreams of Everus/Character Data", fileName = "CharacterData")]
    public class CharacterData : ScriptableObject
    {
        private const int TicksPerSecond = GameSimulation.TicksPerSecond;

        [Header("Body (PLACEHOLDER)")]
        [Tooltip("Width of the collision box, in world units (1 unit = 1 Unity grid square).")]
        [Min(0.1f)] public float bodyWidth = 0.8f;
        [Tooltip("Height of the collision box, from the feet up, in world units.")]
        [Min(0.1f)] public float bodyHeight = 1.8f;

        [Header("Walk (PLACEHOLDER)")]
        [Tooltip("Top walking speed at full tilt, in units per second. Half tilt walks at half this.")]
        [Min(0f)] public float walkSpeed = 4f;
        [Tooltip("How quickly walking and running speed up, in units per second, per second.")]
        [Min(0f)] public float groundAcceleration = 60f;
        [Tooltip("How quickly the character slides to a stop with the stick centered, in units per second, per second.")]
        [Min(0f)] public float groundFriction = 40f;

        [Header("Dash, trot and run (PLACEHOLDER)")]
        [Tooltip("Speed of the initial dash (started by a hard sideways press), in units per second.")]
        [Min(0.1f)] public float dashSpeed = 11f;
        [Tooltip("The trot: how far the initial dash travels, in units. During the trot, a hard press the other way dash dances. After it, the character runs.")]
        [Min(0.05f)] public float trotDistance = 2.2f;
        [Tooltip("Top running speed (after the trot), in units per second.")]
        [Min(0f)] public float runSpeed = 9f;
        [Tooltip("How many frames the run turnaround (turning while running) takes.")]
        [Min(1)] public int runTurnaroundFrames = 12;
        [Tooltip("How quickly the character skids to a stop during the run turnaround, in units per second, per second.")]
        [Min(0f)] public float runTurnaroundDeceleration = 60f;

        [Header("Crouch and crawl (PLACEHOLDER)")]
        [Tooltip("Body height while crouching or crawling, in units. Lets the character fit under low ceilings.")]
        [Min(0.1f)] public float crouchHeight = 1.0f;
        [Tooltip("Whether this character can crawl (move while crouched). Every character can crouch.")]
        public bool canCrawl = false;
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

        public FighterConfig ToFighterConfig()
        {
            var config = new FighterConfig();
            ApplyTo(config);
            return config;
        }

        /// <summary>Copies these values into an existing config. Used for live tuning in the Editor.</summary>
        public void ApplyTo(FighterConfig config)
        {
            config.BodyWidth = Fixed.FromFloat(bodyWidth);
            config.BodyHeight = Fixed.FromFloat(bodyHeight);

            config.WalkSpeed = PerTick(walkSpeed);
            config.GroundAcceleration = PerTickSquared(groundAcceleration);
            config.GroundFriction = PerTickSquared(groundFriction);
            config.DashSpeed = PerTick(dashSpeed);
            config.TrotTicks = Math.Max(1, (int)Math.Ceiling(trotDistance / (dashSpeed / TicksPerSecond)));
            config.RunSpeed = PerTick(runSpeed);
            config.RunTurnaroundTicks = runTurnaroundFrames;
            config.RunTurnaroundDeceleration = PerTickSquared(runTurnaroundDeceleration);

            config.CrouchHeight = Fixed.FromFloat(Math.Min(crouchHeight, bodyHeight));
            config.CanCrawl = canCrawl;
            config.CrawlSpeed = PerTick(crawlSpeed);

            config.JumpSquatTicks = jumpSquatFrames;
            config.FullHopVelocity = JumpVelocityFor(fullHopHeight);
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
        }
    }
}
