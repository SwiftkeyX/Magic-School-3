namespace MagicSchool.Contracts
{
    // IHeroStats answer: how much is hero stat?
    public interface IHeroStats
    {
        int CurrentHP { get; }
        int MaxHP { get; }
        int CurrentMana { get; }
        int MaxMana { get; }
        int AttackDamage { get; }
        float AttackSpeed { get; }
        int Range { get; }
        float GetStat(StatEnum type);
    }
}
