namespace MagicSchool.Contracts
{
    // IBannerPanel answer: what should the player be told about the match right now?
    // e.g. "drag heroes on and press space to start the combat", or "blue won, press SPACE for stage 2"
    public interface IBannerPanel
    {
        void ShowPreparation(int stage, int stageCount);        // show banner during preparation
        void ShowCombat(int stage, int stageCount);             // show banner during combat
        void ShowResult(TeamEnum? winner, int stage, int stageCount, bool runCleared);  // show banner after combat
    }
}
