using MIConvexHull;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolumeIntersection
{
    public interface IVector : IVertex
    {
        /// <summary>
        /// Gets or sets a copy of positions.
        /// </summary>
        new double[] Position { get; set; }

        int Index { get; }
    }
}
