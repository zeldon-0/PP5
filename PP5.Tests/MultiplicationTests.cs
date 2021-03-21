using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Xunit;
using FluentAssertions;

namespace PP5.Tests
{
    public class MultiplicationTests
    {
        [Theory]
        [MemberData(nameof(VectorData))]
        public void Should_correctly_multiply_vectors(int[] firstVector, int[] secondVector, int expectedResult)
        {
            //Act
            var multiplicationResult = MultiplicationHelper.Multiply(firstVector, secondVector);

            //Assert
            multiplicationResult.Should().Be(expectedResult);
        }

        [Theory]
        [MemberData(nameof(MatrixData))]
        public void Should_correctly_multiply_matrix_and_vector(int[,] matrix, int[] vector, int[] expectedResult)
        {
            //Act
            var multiplicationResult = MultiplicationHelper.Multiply(matrix, vector);

            //Assert
            multiplicationResult.Should().BeEquivalentTo(expectedResult);
        }

        public static IEnumerable<object[]> VectorData => new List<object[]>
        {
            new object[]
            {
                new int[] {1, 2, 3, 4, 5},
                new int[] {5, 4, 3, 2, 1},
                35
            }
        };

        public static IEnumerable<object[]> MatrixData => new List<object[]>
        {
            new object[]
            {
                new int[,] {{1, 7, 6, 5, 7}, {5, 2, 3, 4, 6}, {9, 0, 3, 4, 5}},
                new int[] {1, 2, 3, 4, 5},
                new int[] {88, 64, 59}
            }
        };
    }
}
