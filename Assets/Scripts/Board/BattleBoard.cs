using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MagicSchool
{
    public class BattleBoard : MonoBehaviour
    {
        // ======================== Runtime data ============================
        // track every hex
        private Dictionary<HexPlacement, Hex> _hexs = new Dictionary<HexPlacement, Hex>();

        // track every hero on the battle board
        private List<Hero> _heroesOnBoard = new List<Hero>();

        // Reverse lookup of Hero.ReservedHex: which hero has claimed which hex.
        // The hero's own field stays the source of truth for "which hex did I claim" - this only
        // answers the opposite question, "who claimed this hex", without scanning the roster.
        private Dictionary<Hex, Hero> _reservedBy = new Dictionary<Hex, Hero>();

        // ======================== Setter & Getter ========================
        public IReadOnlyDictionary<HexPlacement, Hex> Hexs => _hexs;
        public IReadOnlyList<Hero> HeroesOnBoard => _heroesOnBoard;

        void Awake()
        {
            InitializeHex();
        }

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
                        HexPlacement hexKey = new HexPlacement(side, columnIndex, rowIndex);
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
        public void TrackThisHero(Hero hero)
        {
            if (!_heroesOnBoard.Contains(hero)) _heroesOnBoard.Add(hero);
        }

        // Counterpart to TrackThisHero - a hero leaving its hex for a non-hex placement
        // (e.g. back to the bench) is no longer on the battlefield.
        public void UntrackThisHero(Hero hero) => _heroesOnBoard.Remove(hero);


        // ======================== Hex reservation ========================
        // Called by Hero.SetReservedHex, which is the only place a reservation changes.
        public void UpdateReservation(Hero hero, Hex previous, Hex next)
        {
            // Only clear the old entry if this hero still owns it. Two heroes can't hold the same
            // hex, but a stale `previous` would otherwise evict whoever legitimately holds it now.
            if (previous != null && _reservedBy.TryGetValue(previous, out Hero owner) && owner == hero)
            {
                _reservedBy.Remove(previous);
            }

            if (next != null) _reservedBy[next] = hero;
        }

        // Who currently holds this hex, or null if it's free.
        public Hero ReserverOf(Hex hex)
        {
            if (hex == null || !_reservedBy.TryGetValue(hex, out Hero hero)) return null;

            // Destroyed heroes shouldn't hold a hex either - `hero == null` is Unity's fake-null,
            // so this catches a destroyed GameObject before we touch a member on it.
            if (hero == null) return null;

            // Dead heroes don't hold their hex - otherwise a corpse would block that hex forever.
            // Filtered on read rather than cleared on death, so nothing has to hook the transition.
            return hero.StateType == HeroStateType.Dead ? null : hero;
        }

        // "Is this hex taken by someone other than me?"
        public bool IsReservedByOther(Hex hex, Hero asker)
        {
            Hero reserver = ReserverOf(hex);
            return reserver != null && reserver != asker;
        }
    }
}
