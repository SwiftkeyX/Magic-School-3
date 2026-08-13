namespace MagicSchool
{
    // Contract for anything that needs a unit fighting on the board, without needing the whole Hero.
    // E.g.
    // BattleBoard asks "what is all the hero on the board"
    // a template action asks "where its caster stands and who to aim at" 
    public interface ICombatant : IPlaceable, IDamageable, ITargeter
    {
        TeamEnum Team { get; }
        HeroStateEnum StateType { get; }

        // A unit should knows which board it's on
        void TrackOnBoard();
        void UntrackFromBoard();
    }
}
