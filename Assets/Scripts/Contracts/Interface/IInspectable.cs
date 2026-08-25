namespace MagicSchool.Contracts
{
    // IInspectable answer: what does a unit show when the player looks at it?
    // e.g. the hero panel asks for a name, the stat block, and the ability text
    public interface IInspectable : IHeroStats
    {
        string HeroName { get; }
        TeamEnum Team { get; }
        bool HasSkill { get; }
        string SkillName { get; }
        string SkillDescription { get; }
        bool HasPassive { get; }
        string PassiveDescription { get; }
        bool IsAlive { get; }
    }
}
