using System.Collections.Generic;
using System.Linq;
using MagicSchool.Contracts;
using MagicSchool.CombatRecording;
using MagicSchool.Combat.Heroes;
using UnityEngine.Profiling;

namespace MagicSchool.Core.States
{
    /// <summary>
    /// The combat is over, annouce the winner.
    /// </summary>
    internal class ResultState : GameState
    {
        public override GamePhaseEnum StateType => GamePhaseEnum.Result;

        public ResultState(GameManager game) : base(game) { }

        public override void OnEnter()
        {
            _game.Board.SetBattleOn(false);

            _game.Hint?.ShowResult(_game.Winner, _game.StageNumber, _game.StageCount, _game.IsRunCleared);
            _game.Banner?.ShowResult(_game.Winner, _game.StageNumber, _game.StageCount, _game.IsRunCleared);
            _game.Scoreboard?.ShowScores(BuildScores());

            // if player win, show reward panel
            if (_game.Winner == TeamEnum.Blue) _game.Reward?.ShowReward();
        }

        public override void OnExit()
        {
            // close reward panel
            _game.Reward?.SetShown(false);
            _game.Scoreboard?.SetShown(false);
        }

        // if continue is requested, go to preparation state.
        // 1) if player won, go to next stage
        // 2) if player lose, restart this stage
        protected override void CheckSwitchState()
        {
            // polling for player's continue request
            if (!_game.ConsumeContinueRequest()) return;

            bool playerWon = _game.Winner == TeamEnum.Blue;
            bool runCleared = _game.IsRunCleared;

            // if player won, give reward, and go to next stage.
            // if player lose, repeat this stage.
            if (playerWon)
            {
                // give player a reward 
                // if player still choosing the reward, let player finish choosing first, before continue
                if (_game.Reward != null && _game.Reward.IsChoosing)
                {
                    _game.Reward.SetShown(true);
                    return;
                }

                _game.SetStageIndex(runCleared ? 0 : _game.StageIndex + 1);

                // FLAGGING: temporarily, this is to expand hero limit easily for demo version
                // clearing the run loops back to stage 1, so the team it is fought with
                // loops back too - otherwise the replay starts oversized against stage 1.

                // when player clear the game, reset team size
                if (runCleared) _game.ResetHeroLimit();
                // when player not clear the game, but pass the stage, increase team size 
                else _game.GrowHeroLimit();
            }

            // go preparation state
            _game.ChangeState(GamePhaseEnum.Preparation);
        }

        private IReadOnlyList<ScoreRow> BuildScores()
        {
            CombatRecorder recorder = _game.Recorder;

            // guard
            if (recorder == null) return new List<ScoreRow>();

            // return a list of ScoreRow that was order by who do the most dmg.
            return _game.Board.HeroesOnBoard
                .Where(hero => hero != null)
                // get data from this hero
                .Select(hero => GetHeroScoreData(hero, recorder))
                // order by who do the most dmg 
                .OrderByDescending(row => row.Record.DamageDealt)
                // order again by who do the most healing 
                .ThenByDescending(row => row.Record.HealingDone)
                .ToList();
        }

        private ScoreRow GetHeroScoreData(ICombatant hero, CombatRecorder recorder)
        {
            return new ScoreRow(
                    name: (hero as IInspectable)?.DisplayName ?? hero.transform.name,
                    team: hero.Team,
                    isAlive: hero.StateType != HeroStateEnum.Dead,
                    record: recorder.RoundOf(hero));
        }
    }
}
