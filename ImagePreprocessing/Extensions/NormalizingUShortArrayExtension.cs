using System;
using System.Collections.Generic;
using System.Linq;

namespace ImagePreprocessing
{
    public static class NormalizingUShortArrayExtension
    {
        public static UshortArrayAsImage GetNormalizedSizedCrop(this UshortArrayAsImage cropped, int size)
        {
            byte[] orgPixelData = cropped.PixelData;
            int currentByteInOrgPixelData = 0;
            
            // lets find how many black lines we should add on each side
            int linesToAddVertical = (size - cropped.Width) / 2;
            int linesToAddHorizontal = (size - cropped.Height) / 2;
            
            
            
            // lets add the lines;
            byte[] pixelData = new byte[size*size*2];

            int currentByte = 0;
            // add horizontal first, as they are the first bytes
            for (int i = 0; i < linesToAddHorizontal; i++)
            {
                for (int u = 0; u < size * 2; u++) pixelData[currentByte++] = 0;
            }

            for (int i = 0; i < cropped.Height; i++)
            {
                // for every horizontal line of img
                // add the black lines first
                for (int u = 0; u < linesToAddVertical*2; u++) pixelData[currentByte++] = 0;
                // then add the pixeldata for the next width*2 bytes;
                for (int u = 0; u < cropped.Width*2; u++) pixelData[currentByte++] = orgPixelData[currentByteInOrgPixelData++];
                
                // lets add the rest of vertical lines
                for (int u = 0; u < (size-linesToAddVertical-cropped.Width)*2; u++) pixelData[currentByte++] = 0;
            }
            
            return new UshortArrayAsImage(pixelData,size, size);
        }
        public static void Normalize(this UshortArrayAsImage ushortImg)
        {
            throw new NotImplementedException();
        }
        public static UshortArrayAsImage Edge(this UshortArrayAsImage ushortImg, int threshold)
        {
            var image = ushortImg.PixelArray;
            ushort[,] imageOverlay = image.Clone() as ushort[,];



            for (int i = 1; i < image.GetLength(0) - 1; i++)
            {
                for (int j = 1; j < image.GetLength(1) - 1; j++)
                {
                    ushort cr = image[i + 1, j];
                    ushort cl = image[i - 1, j];
                    ushort cu = image[i, j - 1];
                    ushort cd = image[i, j + 1];
                    ushort cld = image[i - 1, j + 1];
                    ushort clu = image[i - 1, j - 1];
                    ushort crd = image[i + 1, j + 1];
                    ushort cru = image[i + 1, j - 1];
                    int power = GetMaxD(cr, cl, cu, cd, cld, clu, cru, crd);

                    if (power > threshold)
                    {
                        imageOverlay[i, j] = 0;
                    }
                    else
                    {
                        imageOverlay[i, j] = UInt16.MaxValue;
                    }

                }
            }
            


            //Console.WriteLine(imageOverlay[(imageOverlay.GetLength(0)-1)/2,imageOverlay.GetLength(1)/2]);
            var x = new UshortArrayAsImage(new byte[ushortImg.Width * ushortImg.Height * 2], ushortImg.Width,
                ushortImg.Height);
            x.PixelArray = GetBiggestSector(imageOverlay);
            TumorDimensions(imageOverlay);
            return x;
        }

        private static int GetD(int cr, int cl, int cu, int cd, int cld, int clu, int cru, int crd, int[,] matrix)
        {
            return Math.Abs(matrix[0, 0] * clu + matrix[0, 1] * cu + matrix[0, 2] * cru
                            + matrix[1, 0] * cl + matrix[1, 2] * cr
                            + matrix[2, 0] * cld + matrix[2, 1] * cd + matrix[2, 2] * crd);
        }

        private static int GetMaxD(int cr, int cl, int cu, int cd, int cld, int clu, int cru, int crd)
        {
            int max = int.MinValue;
            for (int i = 0; i < templates.Count; i++)
            {
                int newVal = GetD(cr, cl, cu, cd, cld, clu, cru, crd, templates[i]);
                if (newVal > max)
                    max = newVal;
            }

            return max;
        }

        private static List<int[,]> templates = new List<int[,]>
        {
            new int[,] {{-3, -3, 5}, {-3, 0, 5}, {-3, -3, 5}},
            new int[,] {{-3, 5, 5}, {-3, 0, 5}, {-3, -3, -3}},
            new int[,] {{5, 5, 5}, {-3, 0, -3}, {-3, -3, -3}},
            new int[,] {{5, 5, -3}, {5, 0, -3}, {-3, -3, -3}},
            new int[,] {{5, -3, -3}, {5, 0, -3}, {5, -3, -3}},
            new int[,] {{-3, -3, -3}, {5, 0, -3}, {5, 5, -3}},
            new int[,] {{-3, -3, -3}, {-3, 0, -3}, {5, 5, 5}},
            new int[,] {{-3, -3, -3}, {-3, 0, 5}, {-3, 5, 5}}
        };

        private static double EuclideanDistance(int p1, int q1, int p2, int q2)
        {
            return Math.Sqrt(Math.Pow(q1 - p1, 2) + Math.Pow(q2 - p2, 2));
            ;
        }

        private static ushort[,] GetBiggestSector(ushort[,] imageOverlay)
        {
            List<Sector> sectors = new List<Sector>();

            for (int i = 0; i < imageOverlay.GetLength(0); i++)
            {
                for (int j = 0; j < imageOverlay.GetLength(1); j++)
                {
                    bool isSearched = false;
                    foreach (Sector x in sectors)
                    {
                        if (x.ImgArray[i, j] == UInt16.MaxValue) isSearched = true;
                    }
                    if (!isSearched && imageOverlay[i, j] == UInt16.MaxValue)
                    {
                        sectors.Add(GetSector(imageOverlay, i, j));
                    }
                }
            }
            return sectors.First(x => x.SectorSize == sectors.Max(y => y.SectorSize)).ImgArray;
        }

        private static Sector GetSector(ushort[,] imageOverlay, int i, int j)
        {
            Sector result = new Sector();
            result.ImgArray = imageOverlay.Clone() as ushort[,];

            

            result.SectorSize = GetSizeOfSector(result.ImgArray, i, j);
            
            for (int k = 0; k < result.ImgArray.GetLength(0); k++)
            {
                for (int l = 0; l < result.ImgArray.GetLength(1); l++)
                {
                    if (result.ImgArray[k, l] == UInt16.MaxValue) result.ImgArray[k, l] = 0;
                    if (result.ImgArray[k, l] == UInt16.MaxValue-1) result.ImgArray[k, l] = UInt16.MaxValue;
                    
                }
            }
            return result;
            
        }

        private static int GetSizeOfSector(ushort[,] resultImgArray, int i, int j)
        {
            if (i < 0 || j < 0 || i >= resultImgArray.GetLength(0) || j >= resultImgArray.GetLength(1)) return 0;
            if (resultImgArray[i, j] != UInt16.MaxValue) return 0;
            
            
            resultImgArray[i, j] = UInt16.MaxValue-1;
            return GetSizeOfSector(resultImgArray, i - 1, j) +
                   GetSizeOfSector(resultImgArray, i + 1, j) +
                   GetSizeOfSector(resultImgArray, i, j - 1) +
                   GetSizeOfSector(resultImgArray, i, j + 1)+1;
        }

        private static void TumorDimensions(ushort[,] imageOverlay)
        {
            int p1 = 0, p2 = 0, q1 = 0, q2 = 0;

            while (imageOverlay[p1, q1] != UInt16.MaxValue)
            {
                q1++;
                if (imageOverlay.GetLength(1) == q1)
                {
                    q1 = 0;
                    p1++;
                }
            }

            Console.WriteLine(imageOverlay[p1, q1]);
            Console.WriteLine(q1);
            Console.WriteLine(p1);
            
        }

        private class Sector
        {
            public ushort[,] ImgArray { get; set; }
            public int SectorSize { get; set; }
        }
}
}
    
