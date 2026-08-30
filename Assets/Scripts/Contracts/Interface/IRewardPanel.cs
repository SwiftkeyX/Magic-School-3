namespace MagicSchool.Contracts
{
    // IRewardPanel answer: show the reward panel, that player can choose from
    public interface IRewardPanel
    {
        void ShowReward();              // a stage was won: put the offer on screen
        bool IsChoosing { get; }        // is the player still choosing the reward?
        // FIXNOW: implemnet IPanel, and give it SetShown(), let every have it, 
        // then use this function as a single entry for turn on/off panel
        void SetShown(bool shown);      // close the panel
    }
}
