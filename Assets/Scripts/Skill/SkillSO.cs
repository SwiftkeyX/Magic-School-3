using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MagicSchool
{
    // 1) SkillSO = SO for the skill system. Skill are data-driven which is suitable for using scritable object.
    // 2) SkillSO contains several "SkillStep".
    // 2.1) SkillStep are small skill that can work independently on its own e.g. do AOE damage, shoot projectile, etc...
    // 3) So SkillSO are the bigger skill that construct by several SkillStep and actually was used by the actual hero.
    [CreateAssetMenu(fileName = "Skill", menuName = "Magic School 3/Skill Definition")]
    public class SkillSO : ScriptableObject
    {
        [SerializeField] private string _skillName = "Skill";

        // triggered by OnCast once mana caps
        [FormerlySerializedAs("_steps")]
        [SerializeField] private List<SkillStep> _activeSteps = new List<SkillStep>();

        // triggered by whatever the hero does (attack, combat start, ...)
        [SerializeField] private List<SkillStep> _passiveSteps = new List<SkillStep>();

        // ================================== getter ==================================
        public string SkillName => _skillName;
        public IReadOnlyList<SkillStep> ActiveSteps => _activeSteps;
        public IReadOnlyList<SkillStep> PassiveSteps => _passiveSteps;

        // ================================== setter ==================================
        public void SetSkillName(string skillName) => _skillName = skillName;
        public void SetActiveSteps(List<SkillStep> steps) => _activeSteps = steps;
        public void SetPassiveSteps(List<SkillStep> steps) => _passiveSteps = steps;
    }
}
