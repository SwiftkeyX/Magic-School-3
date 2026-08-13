using System.Collections.Generic;

namespace MagicSchool
{

    /// <summary>
    /// SkillActionGroup contain all the data neccessary to play TemplateAction.
    /// </summary>
    public class SkillActionGroup
    {
        private ActionSourceEnum _source;
        private TemplateAction _templateAction;
        private AimTargetEnum _target;
        private List<SkillCondition> _conditions;

        // public Offset _offset;
        // ...

        private List<SkillEffect> _effects;    // 1 template action = have several effect

        public SkillActionGroup(ActionSourceEnum source, TemplateAction templateAction, AimTargetEnum target,
                                List<SkillCondition> conditions = null, List<SkillEffect> effects = null)
        {
            _source = source;
            _templateAction = templateAction;
            _target = target;

            // empty rather than null, matching what the serialiser writes for an unused list
            _conditions = conditions ?? new List<SkillCondition>();
            _effects = effects ?? new List<SkillEffect>();
        }

        // ============================================ Init ============================================
        // Inject caster into class that need it.
        public void Init(IDamageable caster)
        {
            foreach (SkillCondition condition in _conditions) condition?.Init(caster);

            foreach (SkillEffect effect in _effects) effect?.Init(caster);
        }

        // ============================================ Getter ============================================
        public ActionSourceEnum Source => _source;
        public TemplateAction TemplateAction => _templateAction;
        public AimTargetEnum Target => _target;
        public List<SkillCondition> Conditions => _conditions;
        public List<SkillEffect> Effects => _effects;
    }
}
