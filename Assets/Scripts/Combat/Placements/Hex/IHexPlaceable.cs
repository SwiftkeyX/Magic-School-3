using MagicSchool.Contracts;

namespace MagicSchool.Placements
{
    // Anything that can stand on a board hex
    public interface IHexPlaceable : IPlaceable
    {
        Hex ReservedHex { get; }

        void SetReservedHex(Hex hex);
    }

    public static class PlaceableHexExtensions
    {
        // A hero's Placement is a Hex only while it's on the battlefield - null on the bench.
        // Mirrors what Hero.CurrentHex does, but reachable through the interface.
        public static Hex CurrentHex(this IPlaceable placeable) => placeable?.CurrentPlacement as Hex;
    }
}
