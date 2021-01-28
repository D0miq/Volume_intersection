using System;
using System.Collections.Generic;

namespace VolumeIntersection
{
    /// <summary>
    /// Volumetric data.
    /// </summary>
    /// <typeparam name="TVector">Vector type</typeparam>
    public interface IVolumeData
    {
        void FromTriangulation<TVector, TCell>(List<TVector> vertices, List<TCell> cells) where TVector : IVector where TCell : ITriangleCell;

        void FromVoronoi<TVector>(List<TVector> generators) where TVector : IIndexedVector;
    }
}