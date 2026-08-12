using System.Collections.Generic;
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
                { SkillIdEnum.Galio, GalioSkill.Build },
                { SkillIdEnum.Garen, GarenSkill.Build },
                { SkillIdEnum.Jhin, JhinSkill.Build },
                { SkillIdEnum.Karma, KarmaSkill.Build },
                { SkillIdEnum.Samira, SamiraSkill.Build },
                { SkillIdEnum.Teemo, TeemoSkill.Build },
            };

        /// <summary>
        /// Return a skill that match hero's TemplateAction
        /// </summary>
        public static SkillDefinition Resolve(HeroDataSO data, TemplateActionRegistrySO registry)
        {
            if (data == null) return null;

            // no skill at all is normal - a dummy has none
            if (data.SkillId == SkillIdEnum.None) return null;

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
    }
}
