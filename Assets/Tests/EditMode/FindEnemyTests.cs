using NUnit.Framework;
using UnityEngine;
using MagicSchool.Contracts;
using MagicSchool.Combat.Heroes;
using MagicSchool.Combat.Placements;

namespace MagicSchool.Combat.Tests
{
    // FLAGGING: I don't understand any of this. It was library gap? I can't read this.
    // BUT I should though, I'll come back later.
    /// <summary>
    /// Every question ITargeter can be asked, on a board built in code.
    ///
    /// Read the coordinates as (column, row) on the fixture's grid. Each test says what the shape
    /// is, because a hex board's distances don't read off the indices: a same-column neighbour is
    /// 1.0 away and a diagonal one 1.118.
    /// </summary>
    internal class FindEnemyTests
    {
        private BoardFixture _board;

        [SetUp]
        public void SetUp() => _board = new BoardFixture();

        [TearDown]
        public void TearDown() => _board.Dispose();

        private Hex Blue(int column, int row) => _board.HexAt(TeamEnum.Blue, column, row);
        private Hex Red(int column, int row) => _board.HexAt(TeamEnum.Red, column, row);

        // ==================================== FindNearestEnemy ====================================
        [Test]
        public void NearestEnemy_is_the_closer_of_two()
        {
            Hero me = _board.AddHero(TeamEnum.Blue, Blue(0, 3));
            Hero near = _board.AddHero(TeamEnum.Red, Blue(1, 3));      // 1.118 away
            _board.AddHero(TeamEnum.Red, Blue(3, 3));                  // 3.04 away

            Assert.That(me.FindNearestEnemy(), Is.EqualTo(near));
        }

        [Test]
        public void NearestEnemy_ignores_allies_however_close_they_stand()
        {
            Hero me = _board.AddHero(TeamEnum.Blue, Blue(0, 3));
            _board.AddHero(TeamEnum.Blue, Blue(1, 3));                 // ally, nearer
            Hero enemy = _board.AddHero(TeamEnum.Red, Blue(2, 3));     // enemy, further

            Assert.That(me.FindNearestEnemy(), Is.EqualTo(enemy));
        }

        [Test]
        public void NearestEnemy_ignores_an_enemy_that_is_not_on_the_board()
        {
            Hero me = _board.AddHero(TeamEnum.Blue, Blue(0, 3));
            _board.AddBenchedHero(TeamEnum.Red);                       // tracked, but standing nowhere
            Hero onBoard = _board.AddHero(TeamEnum.Red, Blue(3, 3));

            Assert.That(me.FindNearestEnemy(), Is.EqualTo(onBoard));
        }

        [Test]
        public void NearestEnemy_is_null_when_every_enemy_is_off_the_board()
        {
            Hero me = _board.AddHero(TeamEnum.Blue, Blue(0, 3));
            _board.AddBenchedHero(TeamEnum.Red);

            Assert.That(me.FindNearestEnemy(), Is.Null);
        }

        // =================================== FindFurthestEnemy ===================================
        [Test]
        public void FurthestEnemy_is_the_further_of_two()
        {
            Hero me = _board.AddHero(TeamEnum.Blue, Blue(0, 3));
            _board.AddHero(TeamEnum.Red, Blue(1, 3));
            Hero far = _board.AddHero(TeamEnum.Red, Blue(3, 3));

            Assert.That(me.FindFurthestEnemy(), Is.EqualTo(far));
        }

        // =================================== FindCurrentTarget ===================================
        [Test]
        public void CurrentTarget_starts_as_the_nearest_enemy()
        {
            Hero me = _board.AddHero(TeamEnum.Blue, Blue(0, 3));
            Hero near = _board.AddHero(TeamEnum.Red, Blue(1, 3));
            _board.AddHero(TeamEnum.Red, Blue(3, 3));

            Assert.That(me.FindCurrentTarget(), Is.EqualTo(near));
        }

        [Test]
        public void CurrentTarget_sticks_to_who_it_is_engaged_with_when_someone_closer_walks_up()
        {
            Hero me = _board.AddHero(TeamEnum.Blue, Blue(0, 3), range: 3);
            Hero engaged = _board.AddHero(TeamEnum.Red, Blue(2, 3));
            Assert.That(me.FindCurrentTarget(), Is.EqualTo(engaged), "should have engaged the only enemy");

            _board.AddHero(TeamEnum.Red, Blue(1, 3));                  // closer, but we are busy

            Assert.That(me.FindCurrentTarget(), Is.EqualTo(engaged));
        }

        [Test]
        public void CurrentTarget_is_dropped_once_it_walks_out_of_range()
        {
            Hero me = _board.AddHero(TeamEnum.Blue, Blue(0, 3), range: 1);
            Hero engaged = _board.AddHero(TeamEnum.Red, Blue(1, 3));   // adjacent: in range
            Hero other = _board.AddHero(TeamEnum.Red, Blue(2, 3));
            Assert.That(me.FindCurrentTarget(), Is.EqualTo(engaged), "should have engaged the adjacent enemy");

            _board.MoveHero(engaged, Red(3, 6));                       // clean off the other side

            Assert.That(me.FindCurrentTarget(), Is.EqualTo(other));
        }

        // =================================== FindClusteredLaser ===================================
        [Test]
        public void ClusteredLaser_aims_down_the_line_that_passes_through_the_most_enemies()
        {
            Hero me = _board.AddHero(TeamEnum.Blue, Blue(0, 3));

            // three enemies on one straight line out of my hex, one enemy off on its own
            Hero onTheLine = _board.AddHero(TeamEnum.Red, Blue(1, 3));
            _board.AddHero(TeamEnum.Red, Blue(2, 2));
            _board.AddHero(TeamEnum.Red, Blue(3, 2));
            _board.AddHero(TeamEnum.Red, Blue(0, 0));                  // straight up, alone

            // the three on the line tie at two others caught, so the tie-break takes the nearest
            Assert.That(me.FindClusteredLaser(beamHalfWidth: 0.3f), Is.EqualTo(onTheLine));
        }

        // ================================== FindClusteredLanding ==================================
        [Test]
        public void ClusteredLanding_moves_to_the_hex_whose_blast_catches_the_pair()
        {
            Hero me = _board.AddHero(TeamEnum.Blue, Blue(1, 3));
            _board.AddHero(TeamEnum.Red, Blue(3, 2));
            _board.AddHero(TeamEnum.Red, Blue(3, 3));                  // the two stand 1.0 apart

            // Blue(2,2) sits 1.118 from both - the reachable hex that covers the pair, and it is
            // empty, which is the answer a list of enemies could never have given.
            IPlacement landing = me.FindClusteredLanding(reachRange: 2, blastRadius: 1.2f, isJump: true);

            Assert.That(landing, Is.EqualTo(Blue(2, 2)));
        }

        [Test]
        public void ClusteredLanding_is_null_when_nothing_in_reach_catches_anyone()
        {
            Hero me = _board.AddHero(TeamEnum.Blue, Blue(0, 3));
            _board.AddHero(TeamEnum.Red, Blue(3, 2));
            _board.AddHero(TeamEnum.Red, Blue(3, 3));

            // one hop only: every hex it could reach is 2.0 or more from either enemy
            IPlacement landing = me.FindClusteredLanding(reachRange: 1, blastRadius: 1.2f, isJump: true);

            Assert.That(landing, Is.Null, "holding the cast beats jumping somewhere that hits nobody");
        }

        [Test]
        public void ClusteredLanding_reaches_the_cluster_once_the_range_is_long_enough()
        {
            Hero me = _board.AddHero(TeamEnum.Blue, Blue(0, 3));
            _board.AddHero(TeamEnum.Red, Blue(3, 2));
            _board.AddHero(TeamEnum.Red, Blue(3, 3));

            // the same board as the null case above, two hops instead of one
            IPlacement landing = me.FindClusteredLanding(reachRange: 2, blastRadius: 1.2f, isJump: true);

            Assert.That(landing, Is.EqualTo(Blue(2, 2)));
        }

        [Test]
        public void ClusteredLanding_never_lands_on_a_hex_someone_is_standing_on()
        {
            Hero me = _board.AddHero(TeamEnum.Blue, Blue(1, 3));
            Hero first = _board.AddHero(TeamEnum.Red, Blue(3, 2));
            Hero second = _board.AddHero(TeamEnum.Red, Blue(3, 3));
            Hero third = _board.AddHero(TeamEnum.Red, Blue(3, 4));     // a column of three

            IPlacement landing = me.FindClusteredLanding(reachRange: 3, blastRadius: 1.2f, isJump: true);

            Assert.That(landing, Is.Not.Null);
            Assert.That(landing, Is.Not.EqualTo(first.CurrentPlacement));
            Assert.That(landing, Is.Not.EqualTo(second.CurrentPlacement));
            Assert.That(landing, Is.Not.EqualTo(third.CurrentPlacement));
        }

        [Test]
        public void ClusteredLanding_breaks_a_tie_by_taking_the_shorter_jump()
        {
            Hero me = _board.AddHero(TeamEnum.Blue, Blue(0, 3));
            Hero near = _board.AddHero(TeamEnum.Red, Blue(2, 3));
            _board.AddHero(TeamEnum.Red, Red(1, 3));                   // same prize, far side of the board

            // one enemy caught either way, so the near one wins
            IPlacement landing = me.FindClusteredLanding(reachRange: 4, blastRadius: 1.2f, isJump: true);

            Assert.That(landing, Is.Not.Null);
            float toNear = Vector3.Distance(landing.transform.position, near.transform.position);
            Assert.That(toNear, Is.LessThanOrEqualTo(1.2f), "should have jumped at the enemy it could reach sooner");
        }
    }
}
