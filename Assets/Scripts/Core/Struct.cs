using System;

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

[Serializable]
public struct HeroPlacement
{
    public HeroDataSO dataSO;
    public HexPlacement hexPlacement;

    public HeroPlacement(HeroDataSO data, HexPlacement hexPlacement)
    {
        this.dataSO = data;
        this.hexPlacement = hexPlacement;
    }
}