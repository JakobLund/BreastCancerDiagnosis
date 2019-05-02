﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using NUnit.Framework;
using Dicom;
using System.Windows;
using static ImagePreprocessing.Normalization;

namespace ImagePreprocessing.Tests
{
    [TestFixture]
    class NormalizationTest
    {
        [TestCase]
        public void GetTumourPositionFromMaskTest()
        {
            Rectangle RealValue = new Rectangle(1, 1, 1, 1);
            byte[,] testValue = new byte[,] { { 0, 0, 0 }, { 0, 1, 0 }, { 0, 0, 0 } };
            UByteArrayAsImage image = new UByteArrayAsImage(testValue);
            Rectangle rectangleTestValue = GetTumourPositionFromMask(image);

            Assert.AreEqual(RealValue, rectangleTestValue);
        }

        [TestCase]
        public void CropNormalCaseTest()
        {
            Rectangle testRect = new Rectangle(0, 0, 2, 2);
            
            ushort[,] testValue = new ushort[,] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 1, 1 } };
            ushort[,] realCrop = new ushort[,] {{1, 0}, {0, 1}};
            
            UShortArrayAsImage image = new UShortArrayAsImage(testValue);
            UShortArrayAsImage testCrop = Crop(testRect, image);
            
            Assert.AreEqual(realCrop, testCrop.PixelArray);

        }
        
        [TestCase]
        public void CropOutOfBoundsCaseTest()
        {
            Rectangle testRect = new Rectangle(0, 0, 4, 4);
            
            ushort[,] testValue = new ushort[,] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 1, 1 } };
            ushort[,] realCrop = new ushort[,] {{ 1, 0, 0, 0 }, { 0, 1, 0, 0 }, { 0, 1, 1, 0 }, {0, 0, 0, 0}};
            
            UShortArrayAsImage image = new UShortArrayAsImage(testValue);
            UShortArrayAsImage testCrop = Crop(testRect, image);
            
            Assert.AreEqual(realCrop, testCrop.PixelArray);

        }
        
        [TestCase]
        public void CropStartOutOfBoundsCaseTest()
        {
            Rectangle testRect = new Rectangle(-1, -1, 2, 2);
            
            ushort[,] testValue = new ushort[,] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 1, 1 } };
            ushort[,] realCrop = new ushort[,] { { 0, 0 }, { 0, 1 } };
            
            UShortArrayAsImage image = new UShortArrayAsImage(testValue);
            UShortArrayAsImage testCrop = Crop(testRect, image);
            
            Assert.AreEqual(realCrop, testCrop.PixelArray);

        }

        [TestCase]
        public void FindNearestTest()
        {
            //Does it return the value at the correct position in the array?
            ushort[,] RealImageValue = new ushort[3,5];
            RealImageValue[2, 3] = 17;
            ushort testImageValue = FindNearest(3.3, 2.8, RealImageValue);
            
            Assert.AreEqual(RealImageValue[2,3], testImageValue);
        }

        [TestCase]
        public void MapTest()
        {
            //Is testValue calculated correct?
            float realValue = -2.6904214876f;
            float testValue = Map(4.89f, 19f, -5.2f, 7f, -9.62f);
            Assert.AreEqual(Math.Round(realValue, 2, MidpointRounding.ToEven), Math.Round(testValue, 2, MidpointRounding.ToEven));
        }

        [TestCase]
        public void ResizeImageUpTest()
        {
            //Downscaling an image
            ushort[,] testValue = new ushort[,] { { 1, 1, 0, 0 }, { 0, 1, 0, 1}, { 0, 1, 0, 1}, { 0, 1, 1, 0 } };
            UShortArrayAsImage testImage = new UShortArrayAsImage(testValue);
            
            // 110000110000111111000001000001001110
            ushort[,] realValue = new ushort[,] {{1,1,0,0,0,0}, {1,1,0,0,0,0}, {1,1,1,1,1,1}, {0,0,0,0,0,1}, {0,0,0,0,0,1}, {0,0,1,1,1,0}}; 

            var imagestuff = ResizeImage(testImage, 6);
                
            Assert.AreEqual(realValue, imagestuff.PixelArray);
            
        }
        
    }
}
