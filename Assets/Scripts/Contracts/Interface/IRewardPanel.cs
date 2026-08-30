namespace MagicSchool.Contracts
{
    // IRewardPanel answer: show the reward panel, that player can choose from
    public interface IRewardPanel
    {
        void ShowReward();              // a stage was won: put the offer on screen
        bool IsChoosing { get; }        // is the player still choosing the reward?
        void Hide();                    // close the panel
    }
}
