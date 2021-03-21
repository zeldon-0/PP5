using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PP5
{
    public static class MultiplicationHelper
    {
        public static int[] Multiply(int[,] matrix, int[] vector)
        {
            var result = new int[matrix.GetLength(0)];
            for (var i = 0; i < matrix.GetLength(0); i++)
            {
                var matrixRow =
                    Enumerable.Range(0, matrix.GetLength(1))
                        .Select(x => matrix[i, x])
                        .ToArray();
                result[i] = Multiply(matrixRow, vector);
            }

            return result;
        }

        public static int Multiply(int[] matrixRow, int[] vector) =>
                Enumerable.Range(0, vector.Length)
                .Select(i => matrixRow[i] * vector[i])
                .Sum();
    }
}
