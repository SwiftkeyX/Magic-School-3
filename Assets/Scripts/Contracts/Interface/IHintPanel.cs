namespace MagicSchool.Contracts
{
    // IHintPanel answer: what should the player do next?
    // The line along the bottom of the board. Unlike the banner it is never taken down - every
    // phase has something to tell the player, so all three of these set text.
    public interface IHintPanel
    {
        void ShowPreparation(int stage, int stageCount);
        void ShowCombat(int stage, int stageCount);
        void ShowResult(TeamEnum? winner, int stage, int stageCount, bool runCleared);
    }
}
