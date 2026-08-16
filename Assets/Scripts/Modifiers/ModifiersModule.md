# Modifiers Module - it is a module that build considering asmdef boundary
MagicSchool.Modifiers is a module that:
1) reference to MagicSchool.Contracts
2) was referenced by MagicSchool.Skills

A modifier is not a skill thing. A skill is only what grants one today - traits, items or
augments would grant the same StatModifier and StatusModifier, and none of them should have to
reference MagicSchool.Skills (and so TemplateAction, SkillDefinition and every hero builder) to
say "+20 armour". That is the whole reason this is its own module rather than a folder.

Combat does not reference it: ModifierResolver and Hero only ever touch ICustomModifier and
IModifier, which are in Contracts. The concrete types stay on this side of the line.
