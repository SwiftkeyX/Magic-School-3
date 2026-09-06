using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MagicSchool.Contracts;

namespace MagicSchool.UI
{
    // This is the dev tool to have a quick look at scoreboard panel.
    internal partial class ScoreboardPreview : MonoBehaviour
    {
        // =================================== helper ===================================
        private IReadOnlyList<ScoreRow> BuildRows()
        {
            return _rows
                .Where(row => row != null)
                .Select(row => new ScoreRow(row.Name, row.Team, row.IsAlive, new FakeRecord(row)))
                .ToList();
        }

        // in scoreboard preview, class for author a new row 
        [Serializable]
        private class Row
        {
            public string Name = "Hero";
            public TeamEnum Team = TeamEnum.Blue;
            public bool IsAlive = true;

            [Header("dealt")]
            public int Auto;
            public int Skill;
            public int Overkill;

            [Header("taken")]
            public int Taken;
            public int Blocked;

            [Header("healing done")]
            public int Healed;
            public int Omnivamp;
            public int Overheal;

            [Header("healing taken")]
            public int Received;
            public int Wound;
        }

        // in scoreboard preview, class for author a new fake record for each hero
        private class FakeRecord : ICombatRecord
        {
            private readonly Row _row;

            public FakeRecord(Row row) => _row = row;

            public int DamageDealt => _row.Auto + _row.Skill;
            public int AutoAttackDamage => _row.Auto;
            public int SkillDamage => _row.Skill;
            public int Overkill => _row.Overkill;
            public int HealingDone => _row.Healed + _row.Omnivamp;
            public int SkillHealing => _row.Healed;
            public int OmnivampHealing => _row.Omnivamp;
            public int Overhealing => _row.Overheal;

            public int DamageTaken => _row.Taken;
            public int DamageMitigated => _row.Blocked;
            public int HealingReceived => _row.Received;
            public int HealingLostToWound => _row.Wound;
        }
    }
}
