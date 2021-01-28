using System.Collections.Generic;
using System.Numerics;

namespace VolumeIntersection
{
    /// <summary>
    /// Edge describes a half space that separates two cells. 
    /// The normal has to point inside the source cell.
    /// 
    /// The half space represents a plane in the standard (implicit) form.
    /// For example ax + by + cz >= d or ax + by + cz <= d, where (a,b,c) is a normal.
    /// </summary>
    public class Edge3D : Edge<Vector3, Cell3D>
    {
    }
}
