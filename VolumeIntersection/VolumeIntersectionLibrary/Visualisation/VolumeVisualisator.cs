using System;
using System.Drawing;
using System.Drawing.Imaging;
using VolumeIntersectionLibrary.DataStructures._2D;
using VolumeIntersectionLibrary.DataStructures._3D;

namespace VolumeIntersectionLibrary.Visualisation
{
    /// <summary>
    /// Visualisation of a volumetric data.
    /// </summary>
    public static class VolumeVisualisator
    {
        /// <summary>
        /// Number of bytes needed for a pixel.
        /// </summary>
        private const int BytesPerPixel = 4;

        /// <summary>
        /// Visualises 2D data to a bitmap.
        /// </summary>
        /// <param name="bitmap">Bitmap.</param>
        /// <param name="volume">2D data.</param>
        /// <param name="volumeBoundingBox">Bounding box.</param>
        public static void Visualise2D(Bitmap bitmap, VolumeData2D volume, BoundingBox2D volumeBoundingBox)
        {
            var colors = SetupColors(volume.Cells.Count);

            // Access bitmap data
            int width = bitmap.Width;
            int height = bitmap.Height;
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            unsafe
            {
                // Get pointer to bitmap data
                byte* pointer = (byte*)bitmapData.Scan0;

                // Setup a point
                var point = new Vector2D();

                // For each pixel of a bitmap find a cell and color the pixel.
                for (int i = 0; i < height; i++)
                {
                    // Remap pixel to a position in 3D space.
                    point.Y = MathUtils.Remap(i, 0, height, volumeBoundingBox.Max.Y, volumeBoundingBox.Min.Y);

                    for (int j = 0; j < width; j++)
                    {
                        // Remap pixel to a position in 3D space
                        point.X = MathUtils.Remap(j, 0, width, volumeBoundingBox.Min.X, volumeBoundingBox.Max.X);

                        // For each cell check if it contains the point
                        for (int k = 0; k < volume.Cells.Count; k++)
                        {
                            if (volume.Cells[k].Contains(point))
                            {
                                // Color the pixel
                                pointer[0] = colors[k].B;
                                pointer[1] = colors[k].G;
                                pointer[2] = colors[k].R;
                                pointer[3] = colors[k].A;
                                break;
                            }
                        }

                        pointer += BytesPerPixel;
                    }
                }
            }

            bitmap.UnlockBits(bitmapData);
        }

        /// <summary>
        /// Visualises a slice of a 3D volumetric data.
        /// </summary>
        /// <param name="bitmap">Bitmap that contains the visualisation.</param>
        /// <param name="zValue">Slicing value.</param>
        /// <param name="volume">3D Volumetric data.</param>
        /// <param name="volumeBoundingBox">Bounding box.</param>
        public static void Visualise3D(Bitmap bitmap, double zValue, VolumeData3D volume, BoundingBox3D volumeBoundingBox)
        {
            // Setup color of each cell
            var colors = SetupColors(volume.Cells.Count);

            // Access bitmap data
            int width = bitmap.Width;
            int height = bitmap.Height;
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            unsafe
            {
                // Get pointer to bitmap data
                byte* pointer = (byte*)bitmapData.Scan0;

                // Setup a point
                var point = new Vector3D();
                point.Z = zValue;

                // For each pixel of a bitmap find a cell and color the pixel.
                for (int i = 0; i < height; i++)
                {
                    // Remap pixel to a position in 3D space.
                    point.Y = MathUtils.Remap(i, 0, height, volumeBoundingBox.Max.Y, volumeBoundingBox.Min.Y);

                    for (int j = 0; j < width; j++)
                    {
                        // Remap pixel to a position in 3D space
                        point.X = MathUtils.Remap(j, 0, width, volumeBoundingBox.Min.X, volumeBoundingBox.Max.X);

                        // For each cell check if it contains the point
                        for (int k = 0; k < volume.Cells.Count; k++)
                        {
                            if (volume.Cells[k].Contains(point))
                            {
                                // Color the pixel
                                pointer[0] = colors[k].B;
                                pointer[1] = colors[k].G;
                                pointer[2] = colors[k].R;
                                pointer[3] = colors[k].A;
                                break;
                            }
                        }

                        pointer += BytesPerPixel;
                    }
                }
            }

            bitmap.UnlockBits(bitmapData);
        }

        /// <summary>
        /// Prepares different colors for each cell. 
        /// </summary>
        /// <param name="count">Nuber of cells.</param>
        /// <returns>Colors.</returns>
        private static Color[] SetupColors(int count)
        {
            // Setup color of each cell
            var colors = new Color[count];
            Random rd = new Random();
            for (int i = 0; i < count; i++)
            {
                colors[i] = Color.FromArgb(rd.Next(0, 256), rd.Next(0, 256), rd.Next(0, 256));
            }

            return colors;
        }
    }
}