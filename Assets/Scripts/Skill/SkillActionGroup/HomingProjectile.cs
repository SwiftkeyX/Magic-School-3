using System.Collections.Generic;
using UnityEngine;

public class HomingProjectile : Projectile
{
    private float _lifetime = 0.5f;
    [SerializeField] private float _speed = 8f;

    // ======================================== override method ==============================================
    protected override void Play()
    {
        // set object lifetime - fallback despawn if it never reaches anyone
        _lifetime = _castTime;
        Destroy(gameObject, _lifetime);

        // initialize hitbox: apply effects once, on first contact
        OnContactHitbox onceHitbox = new OnContactHitbox();
        onceHitbox.OnHit += HandleHit;
        _hitbox = onceHitbox;
        _hitbox.Init(_me);
    }


    // ======================================== private ==============================================
    private void Update()
    {
        if (_aimTarget == null) { Destroy(gameObject); return; }

        transform.position = Vector3.MoveTowards(transform.position, _aimTarget, _speed * Time.deltaTime);
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
