using System;
using Complex = System.Numerics.Complex;
using System.Collections.Generic;
using BitMiracle.LibJpeg.Classic;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Storage;
using ImagePreprocessing;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;
using Microsoft.Win32.SafeHandles;
using Vector = MathNet.Numerics.LinearAlgebra.Double.Vector;


namespace DimensionReduction
{
    public class PCA
    {
        public PCA()
        {
        }

        public void LoadModelFromFile(string path)
        {
            //Load database
        }

        public void SaveModelToFile(string path)
        {
            //Save to database
        }

        public void GetComponentsFromImage(int count, int image)
        {
            //TBD option 1: weighted solution. option 2: other solution.
            //Select 1
        }

        public void Train(List<UshortArrayAsImage> images)
        {
            double[,] allImages = new double[images.Count, images[0].Width*images[0].Height];
            int i = 0;
            foreach (var image in images)
            {
                double[,] tempI = new double[image.Width,image.Height]; 
                Array.Copy(image.PixelArray,tempI,image.PixelCount); 
                double[] dImage = new double[image.Width*image.Height];
                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        //dImage[y * image.Width + x] = image.GetPixel(x,y).R;
                        dImage[y * image.Width + x] = tempI[x,y];
                    }
                }

                for (int y = 0; y < image.Height; y++)
                {
                    for (int x = 0; x < image.Width; x++)
                    {
                        allImages[i, y * image.Width + x] = dImage[y * image.Width + x];
                    }
                }

                i++;
            }

            SparseMatrix matrix = SparseMatrix.OfArray(allImages);
            matrix = MeanSubtraction(matrix);
            matrix = CovarianceMatrix(matrix);

            Evd<double> eigen = SolveForEigen(matrix);
            
            List<(Complex, Matrix<double>)> eigenLumps = new List<(Complex, Matrix<double>)>();

            List<List<double>> features = new List<List<double>>();
            List<System.Numerics.Vector<double>> meanSums = new List<System.Numerics.Vector<double>>();

            Model model = new Model(eigen.EigenValues, eigen.EigenVectors, eigenLumps, features, new List<Vector<double>>());
            model.SaveModelToFile("t.xml");
        }

        ///<summary>
        ///Finds the mean of each column in a matrix, then subtracts the mean of
        ///each column from all its values
        ///</summary>
        ///<param name=matrix>a SparseMatrix to perform MeanSubtraction on</param>
        public SparseMatrix MeanSubtraction(SparseMatrix matrix)
        {
            var sums = matrix.ColumnSums();
            int index = 0;
            SparseMatrix tmpMatrix = new SparseMatrix(matrix.RowCount,matrix.ColumnCount);
            List<MathNet.Numerics.LinearAlgebra.Vector<double>> vectors = new List<MathNet.Numerics.LinearAlgebra.Vector<double>>();
            matrix.CopyTo(tmpMatrix);
            foreach (var sum in sums)
            {
                if (Double.IsNegativeInfinity(sum) || Double.IsInfinity(sum))
                    throw new NotFiniteNumberException(sum);

                double xI = sum / matrix.RowCount;

                var tmpVector = matrix.Column(index);
                tmpVector = tmpVector.Subtract(xI);
                vectors.Add(tmpVector);
                
                index += 1;
            }

            SparseMatrix sMatrix = SparseMatrix.OfColumnVectors(vectors);
            return sMatrix;
        }

        ///<summary>
        ///Finds covariance for two doubles
        ///</summary>
        ///<param name=x>first value</param>
        ///<param name=y>second value</param>
        ///<param name=dim>dimensions of parent matrix</param>
        private double Covariance(double x, double y, int dim)
        {
            return (x * y) / (dim - 1);
        }

        ///<summary>
        ///Finds the covariance matrix of a SparseMatrix
        ///</summary>
        ///<param name=matrix>input matrix</param>
        public SparseMatrix CovarianceMatrix(SparseMatrix matrix)
        {
            double[,] cMatrix = new double[matrix.ColumnCount, matrix.ColumnCount];
            SparseMatrix tmpMatrix = new SparseMatrix(matrix.RowCount,matrix.ColumnCount);
            matrix.CopyTo(tmpMatrix);

            for (int x = 0; x < matrix.ColumnCount; x++)
            {
                for (int y = 0; y < matrix.ColumnCount; y++)
                {
                    for (int i = 0; i < matrix.RowCount; i++)
                    {
                        double val = Covariance(tmpMatrix[i, x], tmpMatrix[i, y],
                            matrix.RowCount);

                        if (Double.IsNegativeInfinity(val) || Double.IsInfinity(val))
                            throw new NotFiniteNumberException(val);

                        cMatrix[x, y] += val;
                    }
                }
            }

            return SparseMatrix.OfArray(cMatrix);
        }

        ///<summary>
        ///Finds the eigen values and vectors of a matrix.
        ///</summary>
        ///<param name=matrix>input matrix. Must be square</param>
        public Evd<double> SolveForEigen(SparseMatrix matrix)
        {
            if (matrix.RowCount != matrix.ColumnCount)
                throw new ArgumentException();

            var eigen = matrix.Evd();

            /*List<Complex> eigenValues = new List<Complex>();
            for (int i = 0; i < evd.EigenValues.Count; i++)
            {
                model.EigenValues.Add(eigenValue);
            }

            for (int i = 0; i < matrix.ColumnCount; i++)
            {
                model.EigenVectors.Add(eigen.EigenVectors.Column(i));
            }*/

            return eigen;
        }
    }
}
