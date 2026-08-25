namespace MagicSchool.Contracts
{
    // IMatchStatusView answer: what should the player be told about the match right now?
    // e.g. "drag heroes on and press space to start the combat", or "blue won, press R to re-start"
    public interface IMatchStatusView
    {
        void ShowPreparation();         // show banner during preparation
        void ShowCombat();              // show banner during combat
        void ShowResult(TeamEnum? winner);  // show banner after combat
    }
}
