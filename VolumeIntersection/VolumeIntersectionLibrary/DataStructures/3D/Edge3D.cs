namespace VolumeIntersectionLibrary.DataStructures._3D
{
    /// <summary>
    /// Edge describes a three dimensional half space that separates two cells. 
    /// The normal has to point inside the source cell.
    /// 
    /// The half space represents a plane in the standard (implicit) form.
    /// For example ax + by + cz >= -d, where (a,b,c) is a normal.
    /// </summary>
    public class Edge3D : Edge<Vector3D, Cell3D>
    {
    }
}
