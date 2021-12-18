using System;

namespace PlatformerGameServer.Utils
{
    public class Location
    {
        public double X, Y;
        public int Direction = 1;

        public Location()
        {
        }

        public Location(double x, double y, int direction)
        {
            X = x;
            Y = y;
            Direction = direction;
        }

        public Location Set(double x, double y)
        {
            X = x;
            Y = y;

            return this;
        }

        public double DistancePow(Location loc) => Math.Pow(loc.X - X, 2) + Math.Pow(loc.Y - Y, 2);

        public Location Clone() => new Location(X, Y, Direction);

        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }
}