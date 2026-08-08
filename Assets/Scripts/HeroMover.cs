namespace MagicSchool
{
    // There's 2 ways, hero can move:
    // 1) Hero move on their own while combat
    // 2) Hero got move by the system

    /// <summary>
    /// This is the said system that move hero e.g. move hero while seeding hero on the board
    /// </summary>
    public class HeroMover
    {
        public void MoveThisHeroTo(ICombatant hero, Placement placement)
        {
            // exit old placement
            Placement oldPlacement = hero.CurrentPlacement;
            if (oldPlacement != null) oldPlacement.OnHeroUnplaced(hero);

            // enter new placement
            placement.OnHeroPlaced(hero);

            // ASKING: Could we move this inside placement instead now?
            if (hero.IsInCombat) hero.TrackOnBoard();
            else hero.UntrackFromBoard();
        }
    }
}