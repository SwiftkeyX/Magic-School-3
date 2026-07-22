using System;

public enum Team
{
    Blue,
    Red
}

public class HexPlacement
{
    public Team team;
    public int column;
    public int row;

    public HexPlacement(Team team, int column, int row)
    {
        team = this.team;
        column = this.column;
        row = this.row;
    }
}

public static class HexEnumExtensions
{
    // Matches the lowercase "blue"/"red" prefix already used in hex names (e.g. "blue-column0-row0").
    public static string ToNameString(this Team side)
    {
        return side switch
        {
            Team.Blue => "blue",
            Team.Red => "red",
            _ => throw new ArgumentOutOfRangeException(nameof(side), side, null)
        };
    }
}