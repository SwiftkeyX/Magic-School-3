using System.Collections.Generic;
using UnityEngine;

namespace MagicSchool
{
    /// <summary>
    /// AOE type for TemplateAction
    /// </summary>
    public abstract class AOE : TemplateAction
    {
        // ======================================= protected =======================================
        protected override void Play()
        {
            // starting position
            transform.position = GetSpawnPosition();

            SetLifeTime();
        }
        
        /// source = 100% of the time doesn't mean anything in the spawn/aim term. BUT still important it tell us "who use this AOE"
        protected override void ResolveSource(ActionSourceEnum source)
        {
            _source = _me.transform.position;
        }

        /// aim = where the AOE spawn. 
        /// if not target was found, return (don't use skill yet if target not found)
        protected override bool ResolveAimTarget(AimTargetEnum aimTarget)
        {
            // aim skill at self
            if (aimTarget == AimTargetEnum.Self)
            {
                _aimTarget = _me.transform.position;
            }

            // aim skill at current target
            else if (aimTarget == AimTargetEnum.Current)
            {
                ICombatant target = _me.FindNearestEnemy();
                if (target == null) return false;
                _aimTarget = target.transform.position;
            }

            // else if () ...

            // fallback
            else _aimTarget = _me.transform.position;

            return true;
        }

        protected override Vector3 GetSpawnPosition() => _aimTarget;

        // ======================================== private ==============================================
        private void SetLifeTime()
        {
            Destroy(gameObject, _lifetime);
        }
    }
}
