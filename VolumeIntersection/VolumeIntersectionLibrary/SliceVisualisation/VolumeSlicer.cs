using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace VolumeIntersection.SliceVisualisation
{
    /// <summary>
    /// Visualisation of a volumetric data.
    /// </summary>
    public static class VolumeSlicer
    {
        /// <summary>
        /// Number of bytes needed for a pixel.
        /// </summary>
        private const int BytesPerPixel = 4;

        /// <summary>
        /// Dimensions of volumetric data
        /// </summary>
        private const int Dimension = 3;

        /// <summary>
        /// Visualises a slice of a 3D volumetric data.
        /// </summary>
        /// <typeparam name="TVector">Vector type.</typeparam>
        /// <param name="bitmap">Bitmap that contains the visualisation.</param>
        /// <param name="axis">Slicing axis.</param>
        /// <param name="value">Slicing value.</param>
        /// <param name="volume">3D Volumetric data.</param>
        public static void Slice<TVector>(Bitmap bitmap, int axis, double value, VolumeData<TVector> volume)
            where TVector : IVector, new()
        {
            // Setup color of each cell
            List<Color> colors = new List<Color>(volume.Cells.Count);
            Random rd = new Random();
            foreach (var cell in volume.Cells)
            {
                colors.Add(Color.FromArgb(rd.Next(0, 256), rd.Next(0, 256), rd.Next(0, 256)));
            }

            // Access bitmap data
            int width = bitmap.Width;
            int height = bitmap.Height;
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            unsafe
            {
                // Get pointer to bitmap data
                byte* pointer = (byte*)bitmapData.Scan0;

                // Setup a point
                double[] position = new double[Dimension];
                position[axis] = value;
                var point = new TVector();

                // Setup axis and bounds
                int nextAxis = (axis + 1) % Dimension;
                int previousAxis = (axis - 1) % Dimension;
                var minBound = volume.BoundingBox.Min.Position;
                var maxBound = volume.BoundingBox.Max.Position;

                // For each pixel of a bitmap find a cell and color the pixel.
                for (int i = 0; i < height; i++)
                {
                    // Remap pixel to a position in 3D space.
                    position[previousAxis] = MathUtils.Remap(i, 0, height, maxBound[previousAxis], minBound[previousAxis]);

                    for (int j = 0; j < width; j++)
                    {
                        // Remap pixel to a position in 3D space
                        position[nextAxis] = MathUtils.Remap(j, 0, width, minBound[nextAxis], maxBound[nextAxis]);

                        point.Position = position;

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
    }
}