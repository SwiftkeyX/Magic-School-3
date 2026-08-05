// Serialized into HexPlacement (BattleBoard/BattlePlacementSO assets) as a raw int - always
// assign explicit values so inserting a new member later can't silently remap existing assets.
public enum Team
{
    Blue = 0,
    Red = 1,
}

public enum GamePhase { Preparation, Combat, Result }