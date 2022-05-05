using Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Drawing.Drawing2D;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Chapter4
{
    [SuppressMessage("Usage", "CA1416", Justification = "Code runs on Windows")]
    public class Chapter4_01 : IChapter
    {
        public void Run()
        {
            List<Matrix> matrices = CreateMatrices(10000000);

            RotateMatrices(matrices, 30);

        }

        private static List<Matrix> CreateMatrices(int amountToCreate)
        {
            List<Matrix> matrices = new();
            for (int i = 0; i < amountToCreate; i++)
            {
                var rand = new Random();
                int[] numbers = new int[10];
                for (int j = 0; j < 6; j++)
                {
                    numbers[j] = rand.Next(0, 10000);
                }

                var matrix3x2 = new Matrix3x2(numbers[0], numbers[1], numbers[2], numbers[3], numbers[4], numbers[5]);
                var matrix = new Matrix(matrix3x2);
                matrices.Add(matrix);
            }

            return matrices;
        }

        void RotateMatrices(IEnumerable<Matrix> matrices, float degrees)
        {
            Parallel.ForEach(matrices, matrix => matrix.Rotate(degrees));
        }
    }
}
