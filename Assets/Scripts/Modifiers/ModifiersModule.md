# Modifiers Module - it is a module that build considering asmdef boundary
MagicSchool.Modifiers is a module that:
1) reference to MagicSchool.Contracts and MagicSchool.StatScaling - a StatModifier holds its own
   StatRatio list and calls the static Scaling to add them up.

Naming: an `IModifier` is the whole grant - one duration, one thing a skill or item hands out.
It is a list of `IModifierPiece`, each piece feeding one stat or status. `Modifier` is the
bundle, `StatModifier`/`StatusModifier` are the pieces, and `ActiveModifier` is one bundle
running on a hero with its timer.
2) was referenced by MagicSchool.Skills and MagicSchool.Combat

