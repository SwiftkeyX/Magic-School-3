namespace MagicSchool.Contracts
{
    // IHeroCountPanel answers: show how many heroes has the player fielded?
    public interface IHeroCountPanel
    {
        void ShowHeroCount(int placed, int limit);  
        void HideHeroCount();                       
    }
}