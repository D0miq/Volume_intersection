using System;
using System.Collections.Generic;
using System.Linq;

namespace VolumeIntersection
{
    public static class VolumeIntersection<TVector>
         where TVector : IVector, new()
    {
        private class Pair
        {
            public int FirstIndex;
            public int SecondIndex;

            public Pair(int firstIndex, int secondIndex)
            {
                this.FirstIndex = firstIndex;
                this.SecondIndex = secondIndex;
            }

            public override bool Equals(object obj)
            {
                return obj is Pair pair &&
                       FirstIndex == pair.FirstIndex &&
                       SecondIndex == pair.SecondIndex;
            }

            public override int GetHashCode()
            {
                int hashCode = -495750798;
                hashCode = hashCode * -1521134295 + FirstIndex.GetHashCode();
                hashCode = hashCode * -1521134295 + SecondIndex.GetHashCode();
                return hashCode;
            }
        }

        public static VolumeData<TVector> Intersect(VolumeData<TVector> volumeData1, VolumeData<TVector> volumeData2)
        {
            // Generate random point inside volume data and find cells that contain it
            
            // TODO Najít náhodný tetrahedron a jeho centroid podle centroidu najít vornoi buňku
            
            Random rd = new Random();

            var start = new TVector() { Position = new double[] { } };

            int firstIndex = volumeData1.Cells.FindIndex(cell => cell.Contains(start));
            int secondIndex = volumeData2.Cells.FindIndex(cell => cell.Contains(start));



            // Create a queue
            var visitedPairs = new HashSet<Pair>();
            var cellQueue = new Queue<Pair>();

            var pair = new Pair(firstIndex, secondIndex);
            visitedPairs.Add(pair);
            cellQueue.Enqueue(pair);

            while(cellQueue.Count != 0)
            {
                pair = cellQueue.Dequeue();

                var firstCell = volumeData1.Cells[pair.FirstIndex];
                var secondCell = volumeData2.Cells[pair.SecondIndex];

                var edges = RemoveHalfSpaces(firstCell, secondCell);
                
                foreach(var edge in edges)
                {
                    if(edge.Source == firstCell)
                    {
                        pair = new Pair(/* edge.Target a secondCell */);
                    } else
                    {
                        pair = new Pair(/* edge.Target a firstCell */);
                    }

                    if(visitedPairs.Add(pair))
                    {
                        cellQueue.Enqueue(pair);
                    }
                }

                // 
            }
        }

        private static List<Face<TVector>> RemoveHalfSpaces(Cell<TVector> c1, Cell<TVector> c2)
        {
            var usedHalfSpaces = new HashSet<Face<TVector>>();
            var halfSpaces = new List<Face<TVector>>(c1.Edges.Count + c2.Edges.Count);
            halfSpaces.AddRange(c1.Edges);
            halfSpaces.AddRange(c2.Edges);

            for (int i = 0; i < halfSpaces.Count; i++)
            {
                for(int j = i + 1; j < halfSpaces.Count; j++)
                {
                    for(int k = j + 1; k < halfSpaces.Count; k++)
                    {
                        var intersectionPoint = FindIntersectionPoint(halfSpaces[i], halfSpaces[j], halfSpaces[k]);

                        if(c1.Contains(intersectionPoint) && c2.Contains(intersectionPoint))
                        {
                            usedHalfSpaces.Add(halfSpaces[i]);
                            usedHalfSpaces.Add(halfSpaces[j]);
                            usedHalfSpaces.Add(halfSpaces[k]);
                        }
                    }
                }
            }

            return usedHalfSpaces.ToList();
        }

        private static TVector FindIntersectionPoint(Face<TVector> e1, Face<TVector> e2, Face<TVector> e3)
        {
            var e1Normal = e1.Normal.Position;
            var e2Normal = e2.Normal.Position;
            var e3Normal = e3.Normal.Position;

            double[,] sub1 = { { e1Normal[1], e1Normal[2], e1.C }, { e2Normal[1], e2Normal[2], e2.C }, { e3Normal[1], e3Normal[2], e3.C } };
            double[,] sub2 = { { e1Normal[0], e1Normal[2], e1.C }, { e2Normal[0], e2Normal[2], e2.C }, { e3Normal[0], e3Normal[2], e3.C } };
            double[,] sub3 = { { e1Normal[0], e1Normal[1], e1.C }, { e2Normal[0], e2Normal[1], e2.C }, { e3Normal[0], e3Normal[1], e3.C } };
            double[,] sub4 = { { e1Normal[0], e1Normal[1], e1Normal[2] }, { e2Normal[0], e2Normal[1], e2Normal[2] }, { e3Normal[0], e3Normal[1], e3Normal[2] } };

            double[] position = { MathUtils.Det3x3(sub1), -MathUtils.Det3x3(sub2), MathUtils.Det3x3(sub3), -MathUtils.Det3x3(sub4) };

            return new TVector() { Position = position };
        }
    }
}