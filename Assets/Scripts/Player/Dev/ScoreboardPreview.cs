using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MagicSchool.Contracts;

namespace MagicSchool.Player
{
    /// <summary>
    /// Dev tool: puts the scoreboard on screen filled with numbers you typed, so the panel can be
    /// looked at without fighting a round to produce one.
    ///
    /// It lives here rather than in a scene of its own on purpose. A second scene would have to be
    /// kept in sync with Board every time the UI grows a dependency, and it would exercise a
    /// different mount than the real one - this drives the same UIDocument, the same
    /// PanelMounter slot, the same controller. Nothing here is referenced by the game: the panel
    /// is reached through IScoreboardPanel, so this compiles away to a component nobody calls.
    ///
    /// Note this does NOT make Play mode faster to enter - that cost is the domain reload, and it
    /// is the same in any scene. What it buys is controlled numbers.
    /// </summary>
    internal class ScoreboardPreview : MonoBehaviour
    {
        /// One hero's line, as ten numbers you can type in the Inspector.
        /// Serializable class rather than the real CombatRecord because that one only grows through
        /// AddDealt/AddTaken as a fight happens - there is no way to simply state a total.
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
            public int Overheal;

            [Header("healing taken")]
            public int Received;
            public int Wound;
        }

        /// A Row seen through the interface the scoreboard actually reads. DamageDealt is derived
        /// rather than typed, because the real record derives it too - typing a total that did not
        /// match its parts would draw a bar that cannot happen in a fight.
        private class FakeRecord : ICombatRecord
        {
            private readonly Row _row;

            public FakeRecord(Row row) => _row = row;

            public int DamageDealt => _row.Auto + _row.Skill;
            public int AutoAttackDamage => _row.Auto;
            public int SkillDamage => _row.Skill;
            public int Overkill => _row.Overkill;
            public int HealingDone => _row.Healed;
            public int Overhealing => _row.Overheal;

            public int DamageTaken => _row.Taken;
            public int DamageMitigated => _row.Blocked;
            public int HealingReceived => _row.Received;
            public int HealingLostToWound => _row.Wound;
        }

        // Seeded with a spread that exercises the layout rather than five similar heroes: a carry
        // that splits auto and skill, a dead hero, a healer so the healing tracks are not empty, a
        // hero who did nothing so the zero styling shows, and a tank whose taken bar is the longest
        // in its column and so defines that column's scale.
        [SerializeField]
        private List<Row> _rows = new List<Row>
        {
            new Row { Name = "Vharn",   Team = TeamEnum.Blue, Auto = 1210, Skill = 1270, Overkill = 145, Taken = 1890, Blocked = 620, Received = 340 },
            new Row { Name = "Sithra",  Team = TeamEnum.Red,  IsAlive = false, Auto = 640, Skill = 1335, Taken = 2310, Blocked = 410 },
            new Row { Name = "Solace",  Team = TeamEnum.Blue, Auto = 430, Taken = 760, Blocked = 240, Healed = 1180, Overheal = 295, Received = 120, Wound = 60 },
            new Row { Name = "Pip",     Team = TeamEnum.Blue, Taken = 90, Blocked = 12 },
            new Row { Name = "Bulwark", Team = TeamEnum.Red,  Auto = 180, Skill = 90, Overkill = 20, Taken = 3120, Blocked = 2400, Received = 900, Wound = 220 },
        };

        private IScoreboardPanel _scoreboard;
        private bool _shown;

        // Same lookup PlayerController uses: the panel is a MonoBehaviour somewhere in the scene
        // and the only thing worth knowing about it is that it answers IScoreboardPanel.
        private void Start()
        {
            _scoreboard = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .OfType<IScoreboardPanel>()
                .FirstOrDefault();
        }

        private void Update()
        {
            if (!PlayerInputSystem.PreviewScoreboardPressedThisFrame) return;

            Toggle();
        }

        /// Also on the component's context menu, so the panel can be raised from the Inspector
        /// while the pointer is busy hovering a bar.
        [ContextMenu("Toggle preview")]
        private void Toggle()
        {
            if (_scoreboard == null) return;

            _shown = !_shown;

            // ShowScores shows the panel itself, so there is only a hide to do on the way back
            if (_shown) _scoreboard.ShowScores(BuildRows());
            else _scoreboard.SetShown(false);
        }

        private IReadOnlyList<ScoreRow> BuildRows()
        {
            return _rows
                .Where(row => row != null)
                .Select(row => new ScoreRow(row.Name, row.Team, row.IsAlive, new FakeRecord(row)))
                .ToList();
        }
    }
}
