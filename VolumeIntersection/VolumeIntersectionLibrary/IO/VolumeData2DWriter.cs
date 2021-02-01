using System.Collections.Generic;
using System.IO;
using VolumeIntersectionLibrary.DataStructures._2D;

namespace VolumeIntersectionLibrary.IO
{
    /// <summary>
    /// Writes 2D data to a text file.
    /// </summary>
    public class VolumeData2DWriter
    {
        /// <summary>
        /// Separator of different values.
        /// </summary>
        private char separator;

        /// <summary>
        /// Creates a new writer.
        /// Values are separated by ','.
        /// </summary>
        public VolumeData2DWriter()
        {
            separator = ',';
        }

        /// <summary>
        /// Creates a new writer with a separator.
        /// </summary>
        /// <param name="separator">Value separator.</param>
        public VolumeData2DWriter(char separator)
        {
            this.separator = separator;
        }

        /// <summary>
        /// Writes 2D data to a file.
        /// </summary>
        /// <param name="path">Path of the file.</param>
        /// <param name="volumeData">Data.</param>
        public void Write(string path, VolumeData2D volumeData)
        {
            StreamWriter writer = new StreamWriter(path);

            writer.WriteLine("# centroid.X" + separator + "centroid.Y" + separator + "triangleIndex" + separator + "voronoiIndex" + separator + "weight" + separator + "visited" + separator + "edgeIndices");

            var edgeList = new List<Edge2D>();

            foreach(var cell in volumeData.Cells)
            {
                writer.Write(cell.Centroid.X);
                writer.Write(separator);
                writer.Write(cell.Centroid.Y);
                writer.Write(separator);
                writer.Write(cell.TriangleIndex);
                writer.Write(separator);
                writer.Write(cell.VoronoiIndex);
                writer.Write(separator);
                writer.Write(cell.Weight);
                writer.Write(separator);
                writer.Write(cell.Visited);

                for(int i = 0; i < cell.Edges.Count; i++)
                {
                    writer.Write(separator);
                    writer.Write(edgeList.Count);

                    edgeList.Add(cell.Edges[i]);
                }

                writer.WriteLine();
            }

            writer.WriteLine("# normal.x" + separator + "normal.y" + separator + "c" + separator + "sourceIndex" + separator + "targetIndex");
            foreach(var edge in edgeList)
            {
                writer.Write(edge.Normal.X);
                writer.Write(separator);
                writer.Write(edge.Normal.Y);
                writer.Write(separator);
                writer.Write(edge.C);
                writer.Write(separator);

                int sourceIndex = -1;
                int targetIndex = -1;

                for(int i = 0; i < volumeData.Cells.Count; i++)
                {
                    if(volumeData.Cells[i] == edge.Source)
                    {
                        sourceIndex = i;
                    }

                    if(volumeData.Cells[i] == edge.Target)
                    {
                        targetIndex = i;
                    }

                    if(sourceIndex != -1 && targetIndex != -1)
                    {
                        break;
                    }
                }

                writer.Write(sourceIndex);
                writer.Write(separator);
                writer.Write(targetIndex);

                writer.WriteLine();
            }

            writer.Close();
        }
    }
}
