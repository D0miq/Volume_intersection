using MIConvexHull;

namespace VolumeIntersection
{
    internal class MIVertex : IVertex
    {
        public double[] Position { get; set; }

        public MIVertex(float x, float y)
        {
            Position = new double[] { x, y };
        }

        public MIVertex(float x, float y, float z)
        {
            Position = new double[] { x, y, z };
        }
    }
}
