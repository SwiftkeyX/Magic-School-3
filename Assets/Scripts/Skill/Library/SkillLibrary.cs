using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MagicSchool
{
    /// <summary>
    /// Where a hero's skill comes from.
    /// </summary>
    public static class SkillLibrary
    {
        // a pair of TemplateAction & SkillDefinition
        // TemplateAction is a skill prefab used by hero, but hero don't know how this TemplateAction work.
        // How the TemplateAction work was put inside SkillDefinition.
        private static readonly Dictionary<SkillIdEnum, System.Func<TemplateActionRegistrySO, SkillDefinition>> Builders =
            new Dictionary<SkillIdEnum, System.Func<TemplateActionRegistrySO, SkillDefinition>>
            {
                { SkillIdEnum.Aatrox, AatroxSkill.Build },
                { SkillIdEnum.Cassiopeia, CassiopeiaSkill.Build },
                { SkillIdEnum.Garen, GarenSkill.Build },
                { SkillIdEnum.Jhin, JhinSkill.Build },
                { SkillIdEnum.Samira, SamiraSkill.Build },
            };

        /// <summary>
        /// Return a skill that match hero's TemplateAction
        /// </summary>
        public static SkillDefinition Resolve(HeroDataSO data, TemplateActionRegistrySO registry)
        {
            if (data == null) return null;

            if (data.SkillId != SkillIdEnum.None)
            {
                if (!Builders.TryGetValue(data.SkillId, out var build))
                {
                    Debug.LogError($"[SkillLibrary] {data.Name} asks for {data.SkillId}, which nothing builds yet.", data);
                    return null;
                }

                if (registry == null)
                {
                    Debug.LogError($"[SkillLibrary] {data.Name} needs the template action registry to build {data.SkillId}, " +
                                   "but none was supplied - check GameManager.", data);
                    return null;
                }

                return build(registry);
            }

            return FromAsset(data.Skill);
        }

        // TRANSITIONAL: a SkillSO dressed as a SkillDefinition, so a hero that has not been ported
        // still runs while the rest catch up.
        private static SkillDefinition FromAsset(SkillSO skill)
        {
            if (skill == null) return null;

            return new SkillDefinition(
                skill.SkillName,
                skill.ActiveSteps.ToList(),
                skill.PassiveSteps.ToList());
        }
    }
}
