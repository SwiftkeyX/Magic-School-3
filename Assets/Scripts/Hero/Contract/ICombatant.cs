namespace MagicSchool
{
    // Contract for BattleBoard to talk to Hero
    // BattleBoard need to answer 1 thing "What is all the hero on the board"
    // So it need talk to only talk to "some part" of the Hero via this contract.
    public interface ICombatant : IPlaceable, IDamageable
    {
        TeamEnum Team { get; }
        HeroStateEnum StateType { get; }

        // A unit should knows which board it's on
        void TrackOnBoard();
        void UntrackFromBoard();
    }
}
