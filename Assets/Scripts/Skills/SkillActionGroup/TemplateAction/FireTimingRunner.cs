using System;
using System.Collections;
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


        // ======================================= private =======================================
        private void FireOnce()
        {
            // copy/paste to create a desired template action
            SkillActionGroup innerGroup = new SkillActionGroup(
                source: _innerSource,
                templateAction: _innerPrefab,
                target: _innerAimTarget,
                effects: _effects,
                tuning: _innerTuning
            );

            TemplateAction.TryPlay(innerGroup, _me, _onExpired, _onHit, _fromPreviousStep);
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
    }
}
