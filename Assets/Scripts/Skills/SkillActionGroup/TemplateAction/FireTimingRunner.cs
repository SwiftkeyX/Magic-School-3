using System;
using System.Collections;
using System.Collections.Generic;
using MagicSchool.Contracts;
using UnityEngine;

namespace MagicSchool.Skills
{
    /// <summary>
    /// A template action whose only job is to repeat another template action several times.
    /// e.g. Akshan's 6 sequential shots, Ashe's 8 arrows at once => those are projectile being repeated several time
    /// </summary>
    internal class FireTimingRunner : TemplateAction<FireTimingRunnerTuning>
    {
        [SerializeField] private TemplateAction _innerPrefab;

        private ActionSourceEnum _innerSource;
        private AimTargetEnum _innerAimTarget;
        private Action<SkillStepContext> _onExpired;
        private Action<SkillStepContext> _onHit;

        // ======================================= tune able =======================================
        private FireTimingModeEnum _mode = FireTimingModeEnum.AtOnce;
        private int _count = 1;
        private float _interval;
        private Tuning _innerTuning;
        private int _randomPoolRadius;

        // ======================================= Random pool =======================================
        // Pseudo-random
        private List<ICombatant> _randomPool;
        private int _randomPoolIndex;

        // ======================================= Event =======================================
        protected override void SubscribeTriggers(Action<SkillStepContext> onExpired, Action<SkillStepContext> onHit)
        {
            _onExpired = onExpired;
            _onHit = onHit;
        }


        // ======================================= override =======================================
        protected override void ApplyTypedTuning(FireTimingRunnerTuning tuning)
        {
            if (tuning.Count.HasValue) _count = tuning.Count.Value;
            if (tuning.Mode.HasValue) _mode = tuning.Mode.Value;
            if (tuning.Interval.HasValue) _interval = tuning.Interval.Value;
            if (tuning.InnerTuning != null) _innerTuning = tuning.InnerTuning;
            if (tuning.RandomPoolRadius.HasValue) _randomPoolRadius = tuning.RandomPoolRadius.Value;
        }

        protected override void Play()
        {
            // play once
            if (_mode == FireTimingModeEnum.AtOnce)
            {
                for (int i = 0; i < _count; i++) FireOnce();
                DestroyMe();
            }

            // repeat [count] time
            else if (_mode == FireTimingModeEnum.Sequence)
            {
                StartCoroutine(FireSequence());
            }
        }

        // FireTimingRunner has no source/aim of its own - it only remembers what it was told,
        // so each inner shot can resolve the real thing independently.
        protected override bool ResolveSource(ActionSourceEnum source)
        {
            _innerSource = source;
            return true;
        }

        protected override bool ResolveAimTarget(AimTargetEnum aimTarget)
        {
            _innerAimTarget = aimTarget;
            return true;
        }

        protected override Vector3 GetSpawnPosition() => _me.transform.position;


        // ======================================= main function =======================================
        private void FireOnce()
        {
            AimTargetEnum shotAimTarget = _innerAimTarget;
            SkillStepContext shotContext = _fromPreviousStep;
            // if aim = random, FireTimingRunner have to be the organizer for picking the random unit.
            if (_innerAimTarget == AimTargetEnum.Random)
            {
                ICombatant chosen = NextRandomTarget();
                if (chosen == null) return;   

                shotAimTarget = AimTargetEnum.Assigned;
                shotContext = new SkillStepContext(chosen);
            }

            // copy/paste to create a desired template action
            SkillActionGroup innerGroup = new SkillActionGroup(
                source: _innerSource,
                templateAction: _innerPrefab,
                target: shotAimTarget,
                effects: _effects,
                tuning: _innerTuning
            );

            TemplateAction.TryPlay(innerGroup, _me, _onExpired, _onHit, shotContext);
        }

        private IEnumerator FireSequence()
        {
            for (int i = 0; i < _count; i++)
            {
                FireOnce();

                bool isLastShot = (i == _count - 1);
                if (!isLastShot) yield return new WaitForSeconds(_interval);
            }

            // after fire everything, destroy itself
            DestroyMe();
        }

        // ====================================== random ======================================
        // FIXLATER: move this out to its own class
        // To organize the pseudo random Fire 
        // e.g. Jinx's fire at nearest enemies to current target.
        private ICombatant NextRandomTarget()
        {
            // guard, and build the random pool
            if (_randomPool == null)
            {
                ICombatant currentTarget = _me.FindCurrentTarget();
                _randomPool = new List<ICombatant>(_me.FindEnemiesNear(currentTarget, _randomPoolRadius));
                _randomPoolIndex = _randomPool.Count;   // force the first call below to shuffle
            }

            if (_randomPool.Count == 0) return null;

            // once the pool is all used, reset the pool, and re-shuffle the pool
            if (_randomPoolIndex >= _randomPool.Count)
            {
                Shuffle(_randomPool);
                _randomPoolIndex = 0;
            }

            // return one of the random unit
            return _randomPool[_randomPoolIndex++];
        }

        private static void Shuffle(List<ICombatant> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
