namespace MagicSchool.Skills
{
    /// <summary>
    /// Which skill in SkillLibrary a hero uses. Set on HeroDataSO.
    ///
    /// Serialized into those assets as a raw int - always assign explicit values so inserting a new
    /// member later can't silently remap which skill an existing hero points at.
    ///
    /// None means "not ported to C# yet" and falls back to the hero's SkillSO.
    /// </summary>
    public enum SkillIdEnum
    {
        None = 0,

        Vharn = 1,
        Sithra = 2,
        Bulwark = 3,
        Roland = 4,
        Quatre = 5,
        Solace = 6,
        Vesper = 7,
        Pip = 8,
        Fang = 9,
        Lyra = 10,
        Aldric = 11,
        Grimm = 12,
        Lumen = 13,  
        Reyn = 14,
        Sparks = 15,
        Mira = 16,
        Verity = 17,
    }
}
