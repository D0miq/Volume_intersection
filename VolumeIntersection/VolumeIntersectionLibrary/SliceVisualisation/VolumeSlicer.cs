using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace VolumeIntersection.SliceVisualisation
{
    public static class VolumeSlicer
    {
        private const int BytesPerPixel = 4;
        private const int PointSize = 3;

        public static void Slice<TVector>(Bitmap bitmap, int axis, float value, VolumeData<TVector> volume)
            where TVector : IVector, new()
        {
            Random rd = new Random();
            int width = bitmap.Width;
            int height = bitmap.Height;
            double[] position = new double[PointSize];
            position[axis] = value;
            var point = new TVector();
            int nextAxis = (axis + 1) % PointSize;
            int previousAxis = (axis - 1) % PointSize;
            var minBound = volume.BoundingBox.Min.Position;
            var maxBound = volume.BoundingBox.Max.Position;

            List<Color> colors = new List<Color>(volume.Cells.Count);
            foreach (var cell in volume.Cells)
            {
                colors.Add(Color.FromArgb(rd.Next(0, 256), rd.Next(0, 256), rd.Next(0, 256)));
            }

            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            unsafe
            {
                byte* pointer = (byte*)bitmapData.Scan0;

                for (int i = 0; i < height; i++)
                {
                    position[previousAxis] = MathUtils.Remap(i, 0, height, maxBound[previousAxis], minBound[previousAxis]);

                    for (int j = 0; j < width; j++)
                    {
                        position[nextAxis] = MathUtils.Remap(j, 0, width, minBound[nextAxis], maxBound[nextAxis]);

                        point.Position = position;

                        for (int k = 0; k < volume.Cells.Count; k++)
                        {
                            if (volume.Cells[k].Contains(point))
                            {
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
        }
    }
}