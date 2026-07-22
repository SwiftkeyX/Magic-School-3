using System;

public enum Team
{
    Blue,
    Red
}

[Serializable]
public struct HexPlacement
{
    public Team team;
    public int column;
    public int row;

    public HexPlacement(Team team, int column, int row)
    {
        this.team = team;
        this.column = column;
        this.row = row;
    }
}