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

        Aatrox = 1,
        Cassiopeia = 2,
        Galio = 3,
        Garen = 4,
        Jhin = 5,
        Karma = 6,
        Samira = 7,
        Teemo = 8,
        Warwick = 9,
        Sona = 10,
        JarvanIV = 11,
    }
}
