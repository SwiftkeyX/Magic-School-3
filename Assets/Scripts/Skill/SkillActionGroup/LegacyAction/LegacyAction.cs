using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class LegacyAction : MonoBehaviour
{
    [SerializeField] protected float _castTime;                 // the duration skill was cast
    protected Hero _me;
    protected List<SkillEffect> _effects;
    protected Hitbox _hitbox;

    // ==================================== getter ====================================
    public float CastTime => _castTime;

    // ==================================== public method ====================================
    // Hero that this would want is also Hero currentTarget, Hero furthestTarget and etc...
    // that'll be resolve later
    public void TriggerSkill(ActionSourceEnum source, AimTargetEnum aimTarget, Hero caster, List<SkillEffect> effects)
    {
        Init(caster, null);

        // find position using source/aim enum
        ResolveSource(source);
        ResolveAimTarget(aimTarget);

        // init the prefab using source/aim vector3
        SpawnPrefab(caster, effects);
    }

    private void Init(Hero caster, List<SkillEffect> effects)
    {
        _me = caster;
        _effects = effects;
    }

    // ==================================== override method ====================================
    // Each legacy action child have a dirrent way to resolve how their skill was spawn/aim at.
    // read ResolveSource&ResolveAimTarget in each different's child for more detail
    protected abstract void ResolveSource(ActionSourceEnum source);
    protected abstract void ResolveAimTarget(AimTargetEnum aimTarget);

    // spawn effect prefab using source, aim
    protected abstract void SpawnPrefab(Hero caster, List<SkillEffect> effects);

    protected abstract void PlayLegacyAction();

    // ==================================== Prefab -> scene instance ====================================
    // _legacyAction fields are prefab references, not live scene objects 
    // - they can't run physics or be destroyed. 
    // This resolves prefab into a real instance awhich fix the problem.
    protected void SpawnInstanceAt(Vector3 position, Hero caster, List<SkillEffect> effects)
    {
        LegacyAction instance = Instantiate(this, position, Quaternion.identity);
        instance.Init(caster, effects);
        instance.PlayLegacyAction();
    }

    // ==================================== Hitbox ====================================

    protected void OnTriggerEnter2D(Collider2D other) => _hitbox.OnTriggerEnter2D(other);

    protected void OnTriggerExit2D(Collider2D other) => _hitbox.OnTriggerExit2D(other);

    // ==================================== Effect & Recipient ====================================
    // apply effect to the recipients
    protected void ApplyEffectToRecipients(SkillEffect effect, List<Hero> recipients)
    {
        if (effect.Recipient == EffectRecipientEnum.Self) effect.ApplyEffect(new List<Hero> { _me });

        else if (effect.Recipient == EffectRecipientEnum.EnemiesInArea) effect.ApplyEffect(recipients);
    }

    // Cadence Tick are use by several legacy action
    // so we unified thing by move it here. 
    // But it should be move later since not all legacy action need it.
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

