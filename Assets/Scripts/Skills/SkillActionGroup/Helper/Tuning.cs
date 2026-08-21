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
    /// stays editable in the Inspector. What the action LOOKS like - sprite, collider shape, the
    /// jump's curve - stays on the prefab, where it can be seen while it is edited. Size is the one
    /// that crosses over: a blast's scale is how far it reaches, so it is a balance number that
    /// happens to be visible, and the aim that looks for the best spot to put it can be handed the
    /// same constant instead of a second field hoping to agree with it.
    /// </summary>
    /// 
    /// FIXLATER: let tuning know what type of template action we use, then use only the variable that matter for that template action
    public class Tuning
    {
        public float? CastTime;          // how long the caster is locked out of auto attacking
        public int? Range;               // in hexes: how far a jump crosses, how far a shot looks
        public float? Size;              // how wide the action is, as a scale on the prefab's collider
        public float? Duration;          // how long the action itself takes
        public float? EffectRange;       // the reach of the thing this action sets off
        public float? LaneHalfWidth;     // half the width of what it sweeps on the way
        public bool? Sticky;             // does it ride whatever it spawned on, or stay put?
    }
}
