using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace TestVolumeIntersection
{
    [TestClass]
    public class VolumeIntersectionTest
    {
        [TestMethod]
        public void TestIntersect()
        {
            var vertices = new List<Vertex>()
            {
                new Vertex(0, 0),
                new Vertex(1, 0),
                new Vertex(0, 1)
            };

            var triangles = new List<Tetrahedron>()
            {
                new Tetrahedron(0, 1, 2)
            };

            var generators = new List<Vertex>()
            {
                new Vertex(1, 0) { Index = 0 },
                new Vertex(0, 0) { Index = 1 },
                new Vertex(-1, 0) { Index = 2 },
                new Vertex(0, 1) { Index = 3 },
                new Vertex(0, -1) { Index = 4 },
            };

            var volumeData = VolumeIntersection.VolumeIntersection.Intersect(vertices, triangles, generators);
        }
    }
}
