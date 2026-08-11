namespace MagicSchool
{
    // What ModifierResolver needs from anything that wants to change a hero: which modifier, how
    // much of it, and for how long. A duration of -1 means permanent.
    public interface IModifier
    {
        public float GetAmount();
        public ModifierEnum GetModifierEnum();
        public float GetDuration();
    }
}
