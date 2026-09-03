using System;
using System.Collections.Generic;
using UnityEngine.UIElements;
using MagicSchool.Contracts;

namespace MagicSchool.UI
{
    /// Shown by ResultState once a round is decided, hidden when the next one starts.
    /// a scoreboard show who done the most dmg, take most dmg
    internal class ScoreboardController : PanelController, IScoreboardPanel
    {
        private readonly struct Column
        {
            public readonly string Header;
            public readonly Func<ICombatRecord, int> Value;
            public readonly bool IsTotal;       // the headline number of its group
            public readonly bool StartsGroup;   // draw the vertical rule before it

            public Column(string header, Func<ICombatRecord, int> value, bool isTotal = false, bool startsGroup = false)
            {
                Header = header;
                Value = value;
                IsTotal = isTotal;
                StartsGroup = startsGroup;
            }
        }

        private static readonly Column[] Columns =
        {
            // what this hero did
            new Column("DEALT",    r => r.DamageDealt,        isTotal: true),
            new Column("AUTO",     r => r.AutoAttackDamage),
            new Column("SKILL",    r => r.SkillDamage),
            new Column("OVERKILL", r => r.Overkill),

            // what was done to it
            new Column("TAKEN",    r => r.DamageTaken,        isTotal: true, startsGroup: true),
            new Column("BLOCKED",  r => r.DamageMitigated),

            // healing, both directions
            new Column("HEALED",   r => r.HealingDone,        isTotal: true, startsGroup: true),
            new Column("OVERHEAL", r => r.Overhealing),
            new Column("RECEIVED", r => r.HealingReceived),
            new Column("WOUND",    r => r.HealingLostToWound),
        };

        private VisualElement _table;

        // =================================== IScoreboardPanel ===================================
        public void ShowScores(IReadOnlyList<ScoreRow> rows)
        {
            if (_table == null) return;

            _table.Clear();
            _table.Add(BuildHeader());

            for (int i = 0; i < rows.Count; i++)
            {
                _table.Add(BuildRow(rows[i], striped: i % 2 == 1));
            }

            SetShown(true);
        }

        // =================================== Life cycle ===================================
        protected override void OnMounted(VisualElement panel)
        {
            _table = panel.Q<VisualElement>("ScoreTable");

            // nothing to show until a round has actually been fought
            SetShown(false);
        }

        // =================================== building ===================================
        private static VisualElement BuildHeader()
        {
            VisualElement header = NewRow();
            header.AddToClassList("row--header");

            header.Add(Cell("HERO", "cell--name", "cell--header"));

            foreach (Column column in Columns)
            {
                Label cell = Cell(column.Header, "cell--header");
                if (column.StartsGroup) cell.AddToClassList("cell--group");

                header.Add(cell);
            }

            return header;
        }

        private static VisualElement BuildRow(ScoreRow row, bool striped)
        {
            VisualElement line = NewRow();
            if (striped) line.AddToClassList("row--stripe");

            Label name = Cell(row.Name, "cell--name");
            name.AddToClassList(row.Team == TeamEnum.Blue ? "name--blue" : "name--red");
            if (!row.IsAlive) name.AddToClassList("name--dead");
            line.Add(name);

            foreach (Column column in Columns)
            {
                int value = row.Record == null ? 0 : column.Value(row.Record);

                Label cell = Cell(value.ToString());
                if (column.IsTotal) cell.AddToClassList("cell--total");
                else cell.AddToClassList("cell--sub");

                // a zero says "this hero did none of that", which is worth reading but not worth
                // the same weight as a real number
                if (value == 0) cell.AddToClassList("cell--zero");
                if (column.StartsGroup) cell.AddToClassList("cell--group");

                line.Add(cell);
            }

            return line;
        }

        private static VisualElement NewRow()
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("row");
            row.pickingMode = PickingMode.Ignore;

            return row;
        }

        private static Label Cell(string text, params string[] extraClasses)
        {
            Label cell = new Label(text);
            cell.AddToClassList("cell");
            cell.pickingMode = PickingMode.Ignore;

            foreach (string extra in extraClasses) cell.AddToClassList(extra);

            return cell;
        }
    }
}
