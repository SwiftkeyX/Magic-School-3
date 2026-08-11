using System;

namespace MagicSchool
{
    [Serializable]
    public struct HexNumber
    {
        public TeamEnum team;
        public int column;
        public int row;

        public HexNumber(TeamEnum team, int column, int row)
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
        public HexNumber hexPlacement;

        public HeroPlacement(HeroDataSO data, HexNumber hexPlacement)
        {
            this.dataSO = data;
            this.hexPlacement = hexPlacement;
        }
    }
}
