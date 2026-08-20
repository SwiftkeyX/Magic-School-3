namespace MagicSchool.Skills
{
    /// <summary>
    /// What one hero's version of a template action is worth: ranges, durations, cast time.
    ///
    /// These are the same kind of number as a skill's damage ratios and stun lengths, so they read
    /// better next to them in the builder than on a prefab. It also lets two steps of one skill
    /// share a value - a charge and the hitbox riding it must agree on a width, and that agreement
    /// is a constant used twice here rather than two Inspector fields nothing keeps in step.
    ///
    /// Anything left null keeps whatever the prefab says, so a value nobody has an opinion about
    /// stays editable in the Inspector. What the action LOOKS like - sprite, scale, collider, the
    /// jump's curve - stays on the prefab, where it can be seen while it is edited.
    /// </summary>
    public class Tuning
    {
        public float? CastTime;          // how long the caster is locked out of auto attacking
        public int? Range;               // in hexes: how far a jump crosses, how far a shot looks
        public float? Duration;          // how long the action itself takes
        public float? EffectRange;       // the reach of the thing this action sets off
        public float? LaneHalfWidth;     // half the width of what it sweeps on the way
    }
}
