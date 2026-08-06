using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class LegacyAction : MonoBehaviour
{
    [SerializeField] protected float _castTime;                 // the duration skill was cast
    protected Hero _me;
    protected List<SkillEffect> _effects;
    protected Hitbox _hitbox;

    protected Vector3 _source;
    protected Vector3 _aimTarget;

    // ==================================== getter ====================================
    public float CastTime => _castTime;

    // ==================================== public method ====================================
    // Hero that this would want is also Hero currentTarget, Hero furthestTarget and etc...
    // that'll be resolve later
    public void TriggerSkill(ActionSourceEnum source, AimTargetEnum aimTarget, Hero caster, List<SkillEffect> effects)
    {
        // _legacyAction fields are prefab references, not live scene objects.
        // So instantiate first so every resolve/spawn step below runs on the real instance instead of the prefab.
        LegacyAction instance = Instantiate(this);
        instance.Init(caster, effects);

        // find position using source/aim enum
        instance.ResolveSource(source);

        // What I want is => hero have mana full => check if target found
        // found => consume mana => use skill => instantiate skill
        // not found => not consume mana => skill was never used or instantiate
        // FIXLATER: BUT okay, let leave this, we can fix this later.
        // no valid target (e.g. Current-targeted with no enemy left) - skip this cast, don't spawn anything
        if (!instance.ResolveAimTarget(aimTarget))
        {
            Destroy(instance.gameObject);
            return;
        }

        instance.transform.position = instance.GetSpawnPosition();
        instance.PlayLegacyAction();
    }

    private void Init(Hero caster, List<SkillEffect> effects)
    {
        _me = caster;
        _effects = effects;
    }


    // ==================================== abstract method ====================================
    // Each legacy action child have a dirrent way to resolve how their skill was spawn/aim at.
    // read ResolveSource&ResolveAimTarget in each different's child for more detail
    protected abstract void ResolveSource(ActionSourceEnum source);

    // returns false if no valid target could be resolved (e.g. Current-targeted with no enemy left) - caller skips the cast
    protected abstract bool ResolveAimTarget(AimTargetEnum aimTarget);

    // where this action's instance should sit once source/aim are resolved.
    // e.g. an AOE spawns at the aim target, 
    // a projectile spawns at its source and travels from there.
    protected abstract Vector3 GetSpawnPosition();

    protected abstract void PlayLegacyAction();


    // ==================================== Hitbox ====================================
    protected void OnTriggerEnter2D(Collider2D other) => _hitbox.OnTriggerEnter2D(other);
    protected void OnTriggerExit2D(Collider2D other) => _hitbox.OnTriggerExit2D(other);


    // ==================================== Effect & Recipient ====================================
    // apply effect to the recipients
    protected void ApplyEffectToRecipients(SkillEffect effect, List<Hero> recipients)
    {
        if (effect.Recipient == EffectRecipientEnum.Self) effect.ApplyEffect(new List<Hero> { _me });

        else if (effect.Recipient == EffectRecipientEnum.SameToAimTarget) effect.ApplyEffect(recipients);

        else if (effect.Recipient == EffectRecipientEnum.EnemiesInArea) effect.ApplyEffect(recipients);
   
        // else if
    }   

    // Cadence Tick are use by several legacy action
    // so we unified thing by move it here. 
    // FIXLATER: But it should be move later since not all legacy action need it.
    protected IEnumerator CadenceTick(HealSkillEffect effect, List<Hero> recipients)
    {
        WaitForSeconds wait = new WaitForSeconds(effect.Cadence.cadenceInterval);
        float elapsed = 0f;

        while (elapsed < effect.Duration)
        {
            yield return wait;
            elapsed += effect.Cadence.cadenceInterval;

            effect.ApplyEffect(recipients);
        }

        Destroy(gameObject);
    }
}

