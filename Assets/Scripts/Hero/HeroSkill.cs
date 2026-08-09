using System;

namespace MagicSchool
{

    /// <summary>
    /// HeroSkill contain trigger condition for each hero skill.
    /// </summary>
    public class HeroSkill
    {
        private readonly Hero _me;
        private readonly SkillSO _skill;
        private float _castTime;
        
        // ============================================== getter ==============================================
        public float GetCastTime() => _castTime;

        // Some heroes (e.g. generic dummy/tank archetypes) have no SkillSO assigned.
        public bool HasSkill => _skill != null && _skill.Steps.Count > 0;

        public HeroSkill(Hero hero, SkillSO skill)
        {
            _me = hero;
            _skill = skill;
        }

        // mana is full, skill is trigger. Returns true if the skill cast successfully.
        public bool TriggerOnCastSkill(bool isManaCapped, out float castTime)
        {
            castTime = 0f;

            if (!isManaCapped || !HasSkill) return false;

            if (_skill.Steps[0].Trigger != TriggerEnum.OnCast) return false;

            // OnCast skill is always step 0
            if (!FireStep(0)) return false;

            // read straight after step 0 played - a later OnExpired step fires long after we return
            castTime = _castTime;

            // if skill success, spend all mana
            _me.SpendMana();

            return true;
        }

        // ============================================== Trigger condition ==============================================
        // on kill enemy, OnKill step is trigger
        private void TriggerOnKillStep(int hp, SkillStep step)
        {
            // bool isTargetDead = hp <= 0;

            // if (isTargetDead) FireAction(IndexOfStep(step));
        }

        // If current template action expired, run next step
        private Action TriggerOnExpiredStep(int nextIndex)
        {
            // guard 
            if (nextIndex >= _skill.Steps.Count) return null;

            // if this step trigger by OnExpired, continue
            if (_skill.Steps[nextIndex].Trigger != TriggerEnum.OnExpired) return null;

            return () => FireStep(nextIndex);
        }

        // ============================================== helper ==============================================
        // Fire skill in "step" order. Returns whether this step actually played anything.
        private bool FireStep(int stepIndex)
        {
            if (stepIndex < 0 || stepIndex >= _skill.Steps.Count) return false;

            // step can have several template action, only template action was choose base on condition
            return PlayAction(stepIndex);
        }

        // Play every template action in this step.
        private bool PlayAction(int stepIndex)
        {
            // hand the follow-up to the template action before it plays - a short one can expire inside Play()
            Action onExpired = TriggerOnExpiredStep(stepIndex + 1);

            // Play 1 template action
            var actionGroups = _skill.Steps[stepIndex].ActionGroups;
            foreach (SkillActionGroup actionGroup in actionGroups)
            {
                // try play the skill
                bool played = TemplateAction.TryPlay(actionGroup.TemplateAction, actionGroup.Source, actionGroup.Target, _me, actionGroup.Effects, onExpired);

                // if one of the template action is played, stop
                if (played)
                {
                    _castTime = actionGroup.TemplateAction.CastTime;
                    return true;
                }
            }

            return false;
        }
    }
}
