using MIConvexHull;
using System;
using System.Collections.Generic;
using System.Text;

namespace VolumeIntersection
{
    public interface IIndexedVertex : IVertex
    {
        /// <summary>
        /// Gets index of this vector.
        /// Should be used when the vector is used to compute a volumetric data from voronoi generators.
        /// </summary>
        int Index { get; }
    }
}
