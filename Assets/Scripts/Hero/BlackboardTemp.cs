using UnityEngine;

// Code architecture begin to be really messy, and some of them don't have its obvious home yet.
// So let just put all of them here so it not scatter around and be confusing.
public class BlackboardTemp
{
    private readonly Hero _me;
    private readonly SpriteRenderer _sprite;
    private readonly SkillTrigger _skillTrigger = new SkillTrigger();

    private static GameObject _skillCastTextPrefab;
    private static readonly Color BlueSkillTextColor = new Color(0.4f, 0.85f, 1f);
    private static readonly Color RedSkillTextColor = new Color(1f, 0.55f, 0.3f);

    public BlackboardTemp(Hero hero, SpriteRenderer sprite)
    {
        _me = hero;
        _sprite = sprite;
    }

    // When hero die, set his sprite transparent to indicate that he is dead.
    public void SetDeadVisual()
    {
        Color c = _sprite.color;
        c.a = 0.3f;
        _sprite.color = c;
    }

    // Floating skill-name text above the hero when its skill actually casts
    public void PlaySkillCastEffect(string skillName)
    {
        if (_skillCastTextPrefab == null) _skillCastTextPrefab = Resources.Load<GameObject>("VFX/SkillCastText");
        if (_skillCastTextPrefab == null) return;

        GameObject instance = Object.Instantiate(_skillCastTextPrefab);
        Color color = _me.Team == Team.Blue ? BlueSkillTextColor : RedSkillTextColor;
        instance.GetComponent<FloatingText>().Show(skillName, color, _me.transform.position);
    }

    // ============================================ skill ============================================
    public bool TriggerSkill(SkillSO skill, SkillStep currentStep, bool isManaCapped)
    {
        bool success = false;
        if (currentStep.Trigger == TriggerEnum.OnCast) success = _skillTrigger.OnCast(isManaCapped, skill, currentStep, _me);
        return success;
    }
}
