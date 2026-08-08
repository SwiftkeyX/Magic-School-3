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
        public void MoveThisHeroTo(Hero hero, Placement placement)
        {
            // exit old placement
            Placement oldPlacement = hero.CurrentPlacement;
            if (oldPlacement != null) oldPlacement.OnHeroUnplaced(hero);

            // enter new placement
            placement.OnHeroPlaced(hero);

            // ASKING: I don't like MoveThisHeroTo() to access inside hero like this. I want it to use contract IPlaceable.
            // Keep the board's roster in step with where the hero ended up. IsInCombat means
            // "my placement is a Hex", so this covers bench -> board and board -> bench both ways
            // without either Placement having to know what a roster is.
            if (hero.IsInCombat) hero.TrackOnBoard();
            else hero.UntrackFromBoard();
        }
    }
}