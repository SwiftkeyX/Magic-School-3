using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;
using MagicSchool.Contracts;

namespace MagicSchool.UI
{
    internal class ScoreboardController : PanelController, IScoreboardPanel
    {
        /// One coloured piece of a bar.
        private readonly struct Segment
        {
            public readonly string Name;                     // what the legend and the tooltip call it
            public readonly string Class;                    // its colour, from Scoreboard.uss
            public readonly Func<ICombatRecord, int> Value;
            public readonly bool IsGhost;                    // the part that did not count, see Tracks

            public Segment(string name, string cssClass, Func<ICombatRecord, int> value, bool isGhost = false)
            {
                Name = name;
                Class = cssClass;
                Value = value;
                IsGhost = isGhost;
            }
        }

        /// One column of bars e.g. damage dealt column, damage taken column
        private readonly struct Track
        {
            public readonly string Header;
            public readonly Segment[] Segments;

            public Track(string header, params Segment[] segments)
            {
                Header = header;
                Segments = segments;
            }
        }

        // initialize each tracks
        // e.g. damage dealt, damage taken, healing done, healing taken
        private static readonly Track[] Tracks =
        {
            new Track("DAMAGE DEALT",
                new Segment("auto",     "seg--auto",     r => r.AutoAttackDamage),
                new Segment("skill",    "seg--skill",    r => r.SkillDamage),
                new Segment("overkill", "seg--ghost",    r => r.Overkill,             isGhost: true)),

            new Track("DAMAGE TAKEN",
                new Segment("taken",    "seg--taken",    r => r.DamageTaken),
                new Segment("blocked",  "seg--ghost",    r => r.DamageMitigated,      isGhost: true)),

            new Track("HEALING DONE",
                new Segment("healed",   "seg--healed",   r => r.HealingDone),
                new Segment("overheal", "seg--ghost",    r => r.Overhealing,          isGhost: true)),

            new Track("HEALING TAKEN",
                new Segment("received", "seg--received", r => r.HealingReceived),
                new Segment("wound",    "seg--ghost",    r => r.HealingLostToWound,   isGhost: true)),
        };

        private VisualElement _table;

        // === tooltip ===
        // the tooltip that appear when hovering on one of a chart. it show how much number that chart actually is.
        private const float TooltipGap = 4f;
        private Label _tooltip;
        private VisualElement _tooltipOwner;

        // =================================== IScoreboardPanel ===================================
        public void ShowScores(IReadOnlyList<ScoreRow> rows)
        {
            if (_table == null) return;

            int[] longest = LongestBarPerTrack(rows);

            HideSegmentTooltip(_tooltipOwner);

            _table.Clear();
            _table.Add(BuildHeader());

            for (int i = 0; i < rows.Count; i++)
            {
                _table.Add(BuildRow(rows[i], longest, striped: i % 2 == 1));
            }

            SetShown(true);
        }

        // =================================== Life cycle ===================================
        protected override void OnMounted(VisualElement panel)
        {
            _table = panel.Q<VisualElement>("ScoreTable");
            _tooltip = NewTooltip(panel);

            // nothing to show until a round has actually been fought
            SetShown(false);
        }

        // =================================== chart width scale ===================================
        // context: those longest value will be used to set chart width in their own track.
        // e.g. style.width = value * 100f / longest[damage_max] 
        
        // get the highest number of all the track 
        // e.g. Damage dealt, Damage taken, etc...
        // e.g. longest = [damage_max, taken_max, etc...]
        private static int[] LongestBarPerTrack(IReadOnlyList<ScoreRow> rows)
        {
            int[] longest = new int[Tracks.Length];

            for (int t = 0; t < Tracks.Length; t++)
            {
                // lowest value set to 1, so it's always drawn in UI
                longest[t] = 1;

                foreach (ScoreRow row in rows)
                {
                    int total = 0;
                    foreach (Segment segment in Tracks[t].Segments) total += ValueOf(row.Record, segment);

                    if (total > longest[t]) longest[t] = total;
                }
            }

            return longest;
        }

        // =================================== building ===================================
        // ====== 1. header ======
        // add header to the scoreboard
        private static VisualElement BuildHeader()
        {
            VisualElement header = NewRow();
            header.AddToClassList("row--header");
            header.Add(Cell("HERO", "cell--name", "cell--header"));

            // each track have its own header
            foreach (Track track in Tracks) header.Add(BuildTrackHead(track));

            return header;
        }

        // build header for a track
        // the header contain title and legend 
        private static VisualElement BuildTrackHead(Track track)
        {
            // head = contain title and legend
            VisualElement head = new VisualElement();
            head.AddToClassList("cell");
            head.AddToClassList("track-head");
            head.pickingMode = PickingMode.Ignore;

            // title = DAMAMGE DEALT, DAMAGE TAKEN, etc...
            Label title = new Label(track.Header);
            title.AddToClassList("track-head__title");
            title.pickingMode = PickingMode.Ignore;
            head.Add(title);

            // legend = ■ auto  ■ skill  ■ overkill 
            VisualElement legend = new VisualElement();
            legend.AddToClassList("track-head__legend");
            legend.pickingMode = PickingMode.Ignore;

            // build a legend
            foreach (Segment segment in track.Segments) legend.Add(BuildKey(segment));

            head.Add(legend);

            return head;
        }

        // build a legend to look like this => ■ auto
        private static VisualElement BuildKey(Segment segment)
        {
            VisualElement key = new VisualElement();
            key.AddToClassList("key");
            key.pickingMode = PickingMode.Ignore;

            // add class for ■
            VisualElement swatch = new VisualElement();
            swatch.AddToClassList("key__swatch");
            swatch.AddToClassList(segment.Class);
            swatch.pickingMode = PickingMode.Ignore;
            key.Add(swatch);

            // add name e.g. auto, skill, overkill, etc...
            Label name = new Label(segment.Name);
            name.AddToClassList("key__label");
            name.pickingMode = PickingMode.Ignore;
            key.Add(name);

            return key;
        }

        // ====== 2. row ======
        private VisualElement BuildRow(ScoreRow row, int[] longest, bool striped)
        {
            VisualElement line = NewRow();
            if (striped) line.AddToClassList("row--stripe");

            // add hero sprite to this line
            line.Add(HeroCell(row));

            // add chart to this line
            for (int t = 0; t < Tracks.Length; t++)
            {
                line.Add(BuildTrack(row.Record, Tracks[t], longest[t]));
            }

            return line;
        }

        // add chart to the row
        // e.g. damage dealt chart, damage taken chart, etc... 
        private VisualElement BuildTrack(ICombatRecord record, Track track, int longest)
        {
            // add rail - a chart 
            VisualElement rail = new VisualElement();
            rail.AddToClassList("track__rail");
            rail.pickingMode = PickingMode.Ignore;

            int total = 0;

            for (int i = 0; i < track.Segments.Length; i++)
            {
                Segment segment = track.Segments[i];
                int value = ValueOf(record, segment);

                if (!segment.IsGhost) total += value;
                if (value <= 0) continue;

                // a actual chart added here
                rail.Add(NewSegment(segment, value, longest));  
            }

            // add cell - to occupy the rail
            VisualElement cell = new VisualElement();
            cell.AddToClassList("cell");
            cell.AddToClassList("track");
            cell.pickingMode = PickingMode.Ignore;

            // chart added
            cell.Add(rail);

            // total number added
            cell.Add(TrackValue(total));

            return cell;
        }

        // a actual chart added here
        // a whole bar's width was scaling accordingly to the track's longest bar.
        private VisualElement NewSegment(Segment segment, int value, int longest)
        {
            VisualElement bar = new VisualElement();
            bar.AddToClassList("seg");
            bar.AddToClassList(segment.Class);

            // the whole bar is measured against the track's longest
            bar.style.width = Length.Percent(value * 100f / longest);

            // add event: the segment could be hovering on
            bar.pickingMode = PickingMode.Position;
            bar.RegisterCallback<PointerEnterEvent>(_ =>
            {
                bar.AddToClassList("seg--hover");
                ShowSegmentTooltip(bar, segment, value);
            });
            bar.RegisterCallback<PointerLeaveEvent>(_ =>
            {
                bar.RemoveFromClassList("seg--hover");
                HideSegmentTooltip(bar);
            });

            return bar;
        }

        private static Label TrackValue(int total)
        {
            Label value = new Label(Format(total));
            value.AddToClassList("track__value");
            value.pickingMode = PickingMode.Ignore;

            if (total == 0) value.AddToClassList("track__value--zero");

            return value;
        }

        // add visual element that contain hero sprite
        private static VisualElement HeroCell(ScoreRow row)
        {
            // create cell container
            VisualElement cell = new VisualElement();
            cell.AddToClassList("cell");
            cell.AddToClassList("cell--name");
            cell.pickingMode = PickingMode.Ignore;

            // create hero's sprite (now sprite is the placeholder)
            VisualElement sprite = new VisualElement();
            sprite.AddToClassList("hero");
            sprite.pickingMode = PickingMode.Ignore;

            // put hero name into the sprite (placeholder)
            Label name = new Label(row.Name);
            name.AddToClassList("hero__name");
            name.pickingMode = PickingMode.Ignore;
            sprite.Add(name);

            // separate team by colour
            sprite.AddToClassList(row.Team == TeamEnum.Blue ? "hero--blue" : "hero--red");
            if (!row.IsAlive) sprite.AddToClassList("hero--dead");

            // add hero's sprite to a cell container
            cell.Add(sprite);

            return cell;
        }

        // =================================== tooltip ===================================
        // this tooltip show how much this segment does in number
        
        // initialize a tooltip for segment
        // e.g. auto-attack = 250 damage, skill = 400 damage
        private static Label NewTooltip(VisualElement panel)
        {
            Label tooltip = new Label();
            tooltip.AddToClassList("seg-tooltip");
            tooltip.AddToClassList(HiddenClass);

            tooltip.pickingMode = PickingMode.Ignore;
            panel.Add(tooltip);

            return tooltip;
        }

        // show tooltip on top of the segment
        private void ShowSegmentTooltip(VisualElement bar, Segment segment, int value)
        {
            if (_tooltip == null) return;

            _tooltipOwner = bar;
            _tooltip.text = segment.Name + "  " + Format(value);

            // move tooltip to on top of segment
            Rect bounds = bar.worldBound;
            Vector2 top = _tooltip.parent.WorldToLocal(new Vector2(bounds.center.x, bounds.yMin));
            _tooltip.style.left = top.x;
            _tooltip.style.top = top.y - TooltipGap;

            // show tooltip
            _tooltip.RemoveFromClassList(HiddenClass);
        }

        // hide tooltip
        private void HideSegmentTooltip(VisualElement bar)
        {
            if (_tooltip == null || _tooltipOwner != bar) return;

            _tooltipOwner = null;
            _tooltip.AddToClassList(HiddenClass);
        }

        // =================================== helper ===================================
        private static int ValueOf(ICombatRecord record, Segment segment)
        {
            return record == null ? 0 : segment.Value(record);
        }

        private static string Format(int value) => value.ToString("N0", CultureInfo.InvariantCulture);

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
