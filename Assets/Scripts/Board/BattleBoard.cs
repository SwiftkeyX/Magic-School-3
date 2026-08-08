using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MagicSchool
{

    /// <summary>
    /// FLAGGING: I have been skeptical about circular dependency between: BattleBoard & Hero.
    /// Because I don't think Hero should know about BattleBoard, the sound of it doesn't make sense to me.
    /// I try to fix it BUT I found that it's already good and don't need any fix. 
    /// => if I move the part where hero need to ask Battleboard out, into somewhere else, 
    /// that just doesn't fix anything, I'm just doing BattleBoard v2 but with different name.
    /// => if we set both as same module, then that would justified its deep coupling.
    /// Let's move them into same folder called "Combat Module"
    /// </summary>
    public class BattleBoard : MonoBehaviour
    {
        // ======================== Runtime data ============================
        // track every hex
        private Dictionary<HexNumber, Hex> _hexs = new Dictionary<HexNumber, Hex>();

        // track every hero on the battle board
        private List<ICombatant> _heroesOnBoard = new List<ICombatant>();
        private Dictionary<Hex, ICombatant> _reservedBy = new Dictionary<Hex, ICombatant>();

        // ======================== Setter & Getter ========================
        public IReadOnlyDictionary<HexNumber, Hex> Hexs => _hexs;
        public IReadOnlyList<ICombatant> HeroesOnBoard => _heroesOnBoard;

        void Awake()
        {
            InitializeHex();
        }

        // =================================== initialzie ===================================
        // BattleBoard find reference to each hex
        void InitializeHex()
        {
            var allHexes = new List<Hex>(GetComponentsInChildren<Hex>(true));

            foreach (var sideGroup in allHexes.GroupBy(h => h.transform.parent.name))
            {
                Team side = sideGroup.Key == "BlueSideHex" ? Team.Blue : Team.Red;

                var columns = sideGroup
                    .GroupBy(h => Mathf.RoundToInt(h.transform.localPosition.x * 10f))
                    .OrderBy(g => g.Key)
                    .ToList();

                int columnIndex = 0;
                foreach (var column in columns)
                {
                    var sortedRows = column.OrderByDescending(h => h.transform.localPosition.y).ToList();
                    int rowIndex = 0;
                    foreach (var hex in sortedRows)
                    {
                        HexNumber hexKey = new HexNumber(side, columnIndex, rowIndex);
                        _hexs[hexKey] = hex;
                        hex.Init(this, hexKey);
                        rowIndex++;
                    }
                    columnIndex++;
                }
            }
        }

        // Every hero need to be tracked on the board
        // If they didn't get tracked, those heroes will be invisible to other hero.
        public void TrackThisHero(ICombatant hero)
        {
            if (!_heroesOnBoard.Contains(hero)) _heroesOnBoard.Add(hero);
        }

        // Counterpart to TrackThisHero - a hero leaving its hex for a non-hex placement
        // (e.g. back to the bench) is no longer on the battlefield.
        public void UntrackThisHero(ICombatant hero)
        {
            _heroesOnBoard.Remove(hero);
        }

        // =================================== Hex reservation ===================================
        // Called by Hero.SetReservedHex, which is the only place a reservation changes.
        public void UpdateReservation(ICombatant hero, Hex previous, Hex next)
        {
            // Only clear the old entry if this hero still owns it. Two heroes can't hold the same
            // hex, but a stale `previous` would otherwise evict whoever legitimately holds it now.
            if (previous != null && _reservedBy.TryGetValue(previous, out ICombatant owner) && owner == hero)
            {
                _reservedBy.Remove(previous);
            }

            if (next != null) _reservedBy[next] = hero;
        }

        // Who currently holds this hex, or null if it's free.
        public ICombatant WhoReservedThisHex(Hex hex)
        {
            if (hex == null || !_reservedBy.TryGetValue(hex, out ICombatant hero)) return null;

            return hero.IsAlive ? hero : null;
        }

        // "Is this hex taken by someone other than me?"
        public bool IsReservedByOther(Hex hex, ICombatant asker)
        {
            ICombatant reserver = WhoReservedThisHex(hex);
            return reserver != null && reserver != asker;
        }
    }
}
