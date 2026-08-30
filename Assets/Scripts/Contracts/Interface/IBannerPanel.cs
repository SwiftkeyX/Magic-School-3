namespace MagicSchool.Contracts
{
    public interface IBannerPanel
    {
        void ShowResult(TeamEnum? winner, int stage, int stageCount, bool runCleared);
        void SetShown(bool shown);
    }
}
