namespace VolumeIntersection
{
    /// <summary>
    /// Edge describes a two dimensional half space that separates two cells. 
    /// The normal has to point inside the source cell.
    /// 
    /// The half space represents a line in the standard (implicit) form.
    /// For example ax + by >= -c, where (a, b) is a normal.
    /// </summary>
    public class Edge2D : Edge<Vector2D, Cell2D>
    {
    }
}