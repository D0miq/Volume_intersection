using System.Collections.Generic;
using VolumeIntersectionLibrary.DataStructures;

namespace VolumeIntersectionLibrary.Intersection.HalfSpaceRemoval
{
    /// <summary>
    /// Removes half spaces and creates an intersection.
    /// </summary>
    /// <typeparam name="TVector">Vector type.</typeparam>
    /// <typeparam name="TCell">Cell type.</typeparam>
    /// <typeparam name="TEdge">Edge type.</typeparam>
    public interface IHalfSpaceRemoval<TVector, TCell, TEdge> where TCell : Cell<TVector, TEdge>, new() where TEdge : Edge<TVector, TCell>
    {
        /// <summary>
        /// Removes redundant half spaces and creates a minimal intersection of two cells.
        /// </summary>
        /// <param name="triangleCell">Triangle cell.</param>
        /// <param name="voronoiCell">Voronoi cell.</param>
        /// <param name="intersectionPoints">Intersection points.</param>
        /// <returns>List of half spaces.</returns>
        List<TEdge> RemoveHalfSpaces(TCell triangleCell, TCell voronoiCell, out List<TVector> intersectionPoints);
    }
}
