namespace MagicSchool
{

    /// <summary>
    /// Some hero skill need a combo counter. e.g. Aatrox 3 hit combo.
    /// ComboTracker dedicate itself for being a counter.  
    /// </summary>
    public class ComboTracker
    {
        private readonly int _length;
        private readonly TriggerEnum _advanceOn;

        public int Beat { get; private set; } = 1;

        public ComboTracker(int length, TriggerEnum advanceOn = TriggerEnum.OnAttack)
        {
            _length = length < 1 ? 1 : length;
            _advanceOn = advanceOn;
        }

        public void Count(TriggerEnum trigger)
        {
            if (trigger != _advanceOn) return;

            // 1, 2, ... length, then back to 1
            Beat = Beat % _length + 1;
        }
    }
}
