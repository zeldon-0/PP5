using System;
using System.Collections.Generic;
using System.Text;

namespace PP5
{
    public static class DataGenerator
    {
        public static int[,] GenerateMatrix(int size)
        {
            var random = new Random();
            var matrix = new int[size, size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                    matrix[i, j] = random.Next(-100, 101);
            }

            return matrix;
        }

        public static int[] GenerateVector(int size)
        {
            var random = new Random();
            var vector = new int[size];
            for (int i = 0; i < size; i++)
            {
                vector[i] = random.Next(-100, 101);
            }

            return vector;
        }
    }
}
