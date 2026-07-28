using UnityEngine;

// Grab-bag for hero logic that doesn't have an obvious home yet. Kept here, behind Blackboard.Temp, 
// instead of scattered as ad hoc methods reached into directly (Hero used to carry these) 
// so "we don't know where this goes" stays contained and easy to find later,
// rather than disguised as real architecture elsewhere.
public class BlackboardTemp
{
    private readonly Hero _me;
    private readonly SpriteRenderer _sprite;

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
    public bool TriggerSkill(SkillStep currentStep, bool isManaCapped)
    {
        bool success = false;
        if (currentStep.Trigger == TriggerEnum.OnCast) success = SkillTrigger.OnCast(isManaCapped, currentStep);
        return success;
    }
}
