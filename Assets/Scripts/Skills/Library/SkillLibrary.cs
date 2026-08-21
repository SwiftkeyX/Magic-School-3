using System;
using System.Collections.Generic;
using UnityEngine;

namespace MagicSchool.Skills
{
    /// <summary>
    /// Where a hero's skill comes from.
    /// </summary>
    public static class SkillLibrary
    {
        // a pair of HeroEnum & SkillDefinition
        // TemplateAction is a skill prefab used by hero, but hero don't know how this TemplateAction work.
        // How the TemplateAction work was put inside SkillDefinition.
        private static readonly Dictionary<SkillIdEnum, Func<TemplateActionRegistrySO, SkillDefinition>> Builders =
            new Dictionary<SkillIdEnum, Func<TemplateActionRegistrySO, SkillDefinition>>
            {
                { SkillIdEnum.Aatrox    , AatroxSkill.Build }    ,
                { SkillIdEnum.Cassiopeia, CassiopeiaSkill.Build },
                { SkillIdEnum.Galio     , GalioSkill.Build }     ,
                { SkillIdEnum.Garen     , GarenSkill.Build }     ,
                { SkillIdEnum.Jhin      , JhinSkill.Build }      ,
                { SkillIdEnum.Karma     , KarmaSkill.Build }     ,
                { SkillIdEnum.Samira    , SamiraSkill.Build }    ,
                { SkillIdEnum.Teemo     , TeemoSkill.Build }     ,
                { SkillIdEnum.Warwick   , WarwickSkill.Build }   ,
                { SkillIdEnum.Sona      , SonaSkill.Build }      ,
                { SkillIdEnum.JarvanIV  , JarvanIVSkill.Build }  ,
                { SkillIdEnum.Sion      , SionSkill.Build }      ,
                { SkillIdEnum.Aphelios  , ApheliosSkill.Build }  ,
                { SkillIdEnum.Akshan    , AkshanSkill.Build }    ,
                { SkillIdEnum.Jinx      , JinxSkill.Build }      ,
                { SkillIdEnum.Gwen      , GwenSkill.Build }      ,
            };

        /// <summary>
        /// Return a skill that match skillID's TemplateAction.
        /// </summary>
        public static SkillDefinition Resolve(SkillIdEnum skillID, TemplateActionRegistrySO registry)
        {
            // no skill at all is normal - a dummy has none
            if (skillID == SkillIdEnum.None) return null;

            if (!Builders.TryGetValue(skillID, out var build))
            {
                Debug.LogError($"[SkillLibrary] {skillID} doesn't exist in the Library.");
                return null;
            }

            if (registry == null)
            {
                Debug.LogError($"[SkillLibrary] {skillID} didn't registry in TemplateActionRegistrySO yet.");
                return null;
            }

            return build(registry);
        }
    }
}
