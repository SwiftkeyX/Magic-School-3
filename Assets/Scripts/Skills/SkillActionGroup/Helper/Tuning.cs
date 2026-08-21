namespace MagicSchool.Skills
{
    /// <summary>
    /// What one hero's version of a template action is worth: ranges, durations, cast time.
    /// </summary>
    public class Tuning
    {
        public float? CastTime;          // how long the caster is locked out of auto attacking
    }

    public class AOETuning : Tuning
    {
        public float? Duration;          // how long the blast stays up before it expires
        public float? Size;              // how wide the action is, as a scale on the prefab's collider
        public bool? Sticky;             // does it ride whatever it spawned on, or stay put?
        public AOEOffsetEnum? Offset;    // is the source the blast's centre, or its tip?
    }

    public class MoveTuning : Tuning
    {
        public int? Range;               // how many hexes the jump may cross
        public float? Duration;          // the whole trip, however far it is
        public float? Spread;            // ClusteredCircle's landing radius, or ClusteredLaser's lane half-width
    }

    public class ProjectileTuning : Tuning
    {
        public int? Range;               // how far out, in hexes, a hex-aimed shot may look
        public float? Spread;            // ClusteredCircle's blast radius, or ClusteredLaser's beam half-width
    }
}
