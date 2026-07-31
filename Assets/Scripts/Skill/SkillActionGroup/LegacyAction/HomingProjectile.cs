using System.Collections.Generic;
using UnityEngine;

public class HomingProjectile : LegacyAction
{
    [SerializeField] private float _lifetime = 0.5f;

    private readonly HashSet<(SkillEffect effect, Hero hero)> _triggeredOnce = new HashSet<(SkillEffect, Hero)>();

    // ======================================== private ==============================================
    protected override void PlayLegacyAction()
    {
        // initialize local variable
        _lifetime = _castTime;

        // set object lifetime
        Destroy(gameObject, _lifetime);

        // initialize hitbox: dispatch once-or-cadence per hero, on their first contact only
        OnContactHitbox onceHitbox = new OnContactHitbox();
        onceHitbox.OnHit += HandleFirstHit;
        _hitbox = onceHitbox;
        _hitbox.Init(_me);
    }

    /// <summary>
    /// When a hero who was hit on first contact:
    /// 1) Apply effect once if not cadence
    /// 2) Apply effect over time if cadence
    /// </summary>
    private void HandleFirstHit(Hero hero)
    {
        List<Hero> recipients = new List<Hero> { hero };
        foreach (SkillEffect effect in _effects)
        {
            if (!effect.Cadence.isCadence)
            {
                ApplyEffectToRecipients(effect, recipients);
            }

        }
    }
}