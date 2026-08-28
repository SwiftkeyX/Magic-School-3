using System.Collections.Generic;
using UnityEngine;
using MagicSchool.Contracts;

namespace MagicSchool.Core
{
    public class HeroFormation
    {
        private readonly HeroMover _heroMover;
        private readonly Dictionary<ICombatant, IPlacement> _formation = new Dictionary<ICombatant, IPlacement>();

        public HeroFormation(HeroMover heroMover)
        {
            _heroMover = heroMover;
        }

        // remember the current formation of the consuem team.
        public void Remember(IReadOnlyList<ICombatant> heroesOnBoard, TeamEnum team)
        {
            _formation.Clear();

            foreach (ICombatant hero in heroesOnBoard)
            {
                if (IsGone(hero) || hero.Team != team) continue;

                _formation[hero] = hero.CurrentPlacement;
            }
        }

        // reset every hero to their original formation
        public void Restore()
        {
            foreach (KeyValuePair<ICombatant, IPlacement> stood in _formation)
            {
                if (IsGone(stood.Key) || stood.Value == null) continue;

                // MoveThisHeroTo, not a bare OnUnitPlaced: leaving the old hex is what frees its
                // reservation, and skipping that would leave the board holding tiles nobody is on.
                _heroMover.MoveThisHeroTo(stood.Key, stood.Value);
            }

            _formation.Clear();
        }


        // ASKING: still confuse.
        // A hero can be destroyed between the snapshot and the restore (a wipe on the way into
        // preparation, say). Cast to Object first: `hero == null` on an interface-typed reference
        // is plain reference equality, which misses Unity's destroyed-but-not-null objects.
        private static bool IsGone(ICombatant hero)
        {
            return hero as Object == null;
        }
    }
}
