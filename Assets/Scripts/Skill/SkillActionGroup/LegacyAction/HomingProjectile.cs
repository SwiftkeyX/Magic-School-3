using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HomingProjectile travels toward the caster's nearest enemy each frame and applies its
/// effects once on contact, then destroys itself. e.g. Jhin/Samira-style bolts.
/// </summary>
public class HomingProjectile : LegacyAction
{
    [SerializeField] private float _lifetime = 0.5f;
    [SerializeField] private float _speed = 8f;

    private Hero _target;

    // ======================================== private ==============================================
    protected override void PlayLegacyAction()
    {
        // set object lifetime - fallback despawn if it never reaches anyone
        _lifetime = _castTime;
        Destroy(gameObject, _lifetime);

        _target = _me.Blackboard.FindNearestEnemy();

        // initialize hitbox: apply effects once, on first contact
        OnContactHitbox onceHitbox = new OnContactHitbox();
        onceHitbox.OnHit += HandleHit;
        _hitbox = onceHitbox;
        _hitbox.Init(_me);
    }

    private void Update()
    {
        if (_target == null || _target.State == HeroStateType.Dead)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, _target.transform.position, _speed * Time.deltaTime);
    }

    private void HandleHit(Hero hero)
    {
        List<Hero> recipients = new List<Hero> { hero };
        foreach (SkillEffect effect in _effects)
        {
            ApplyEffectToRecipients(effect, recipients);
        }

        Destroy(gameObject);
    }
}
