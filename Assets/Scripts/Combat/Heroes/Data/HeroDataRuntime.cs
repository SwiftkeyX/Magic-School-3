using MagicSchool.Contracts;
using MagicSchool.Combat.Heroes.Stats;
using MagicSchool.Combat.Placements;

namespace MagicSchool.Combat.Heroes
{
    /// <summary>
    /// Hold every runtime data for a hero e.g. stat, placement.
    /// Runtime data = data that was can be changed in game dynamically. There MUST NOT have static variable here.
    ///
    /// This class should only contain data and its getter & setter only.
    /// NO LOGIC.
    /// </summary>
    public class HeroDataRuntime
    {
        // ==================================== Description (name, skill desc, etc...) ====================================
        // Temporary, tagged once at its source - see the FIXLATER on HeroDataSO._isDummy.
        private bool _isDummy;

        // ==================================== Stat ====================================
        private Stat _stat;

        // ==================================== Position ====================================
        private IPlacement _currentPlacement;    // placement hero stand on e.g. hex, benchslot
        private Hex _reservedHex;               // hex that hero reserved. use while battle
        private ICombatant _currentTarget;      // who me is engaging


        // ==================================== getter ====================================
        // === stat ===
        public Stat Stat => _stat;  // allow blackboard to read its stat directly

        // === placement ===
        public IPlacement CurrentPlacement => _currentPlacement;
        public Hex ReservedHex => _reservedHex;
        public ICombatant CurrentTarget => _currentTarget;

        // === etc ===
        public bool IsDummy => _isDummy;


        // ==================================== setter ====================================
        // === Placement ===
        public void SetCurrentPlacement(IPlacement placement) => _currentPlacement = placement;
        public void SetReservedHex(Hex hex) => _reservedHex = hex;
        public void SetCurrentTarget(ICombatant hero) => _currentTarget = hero;


        public HeroDataRuntime(HeroDataSO dataSO)
        {
            _isDummy = dataSO.IsDummy;
            _stat = new Stat(dataSO);
        }
    }
}
