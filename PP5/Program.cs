using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MPI;
namespace PP5
{
    class Program
    {
        private static int[,] _matrix;
        private static int[] _vector;
        private static int[] _localResult;
        private static int[] _finalResult;
        private static int _rowsPerProcessCount;
        private static Dictionary<int, int[]> _rowProcessDistribution;

        static void Main(string[] args)
        {
            int size = 1000;
            CalculateInParallel(ref args, size);
            //CalculateInOrder(size);
        }

        private static void CalculateInOrder(int size)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            InitializeInputData(size);
            _finalResult = MultiplicationHelper.Multiply(_matrix, _vector);
            stopWatch.Stop();
            Console.WriteLine($"Time elapsed: {stopWatch.Elapsed} ms");
        }

        private static void CalculateInParallel(ref string[] args, int size)
        {
            MPI.Environment.Run(ref args, comm =>
            {
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                InitializeProcessCommonData(size, comm.Size);
                InitializeRowDistributionData(size, comm.Size);

                if (comm.Rank == 0)
                {
                    InitializeInputData(size);
                    //PrintInput();
                    foreach (var process in _rowProcessDistribution.Keys)
                    {
                        var rowsToProcess = _rowProcessDistribution[process];
                        var rowsArray = GetMatrixRowsList(rowsToProcess);
                        comm.Send(rowsArray, process, (int)Tags.MatrixRequest);
                        comm.Send(_vector.ToList(), process, (int)Tags.VectorRequest);
                    }
                }
                else
                {
                    if (_rowProcessDistribution.ContainsKey(comm.Rank))
                    {
                        var rowsArray = comm.Receive<List<int>>(0, (int)Tags.MatrixRequest);
                        var vector = comm.Receive<List<int>>(0, (int)Tags.VectorRequest).ToArray();
                        var separateRows = GetSeparateRowsFromList(rowsArray, size);
                        var multiplicationResults = new List<int>();
                        foreach (var row in separateRows)
                        {
                            multiplicationResults.Add(MultiplicationHelper.Multiply(row, vector));
                        }

                        _localResult = multiplicationResults.ToArray();
                    }
                }
                comm.GatherFlattened(_localResult, 0, ref _finalResult);
                if (comm.Rank == 0)
                {
                    _finalResult = _finalResult.Skip(_rowsPerProcessCount).Take(size).ToArray();
                    stopWatch.Stop();
                    //PrintFinalOutput();
                    Console.WriteLine($"Time elapsed: {stopWatch.Elapsed} ms");
                }
            });
        }

        private static void InitializeProcessCommonData(int vectorLength, int processCount)
        {
            _rowsPerProcessCount = (int)Math.Ceiling((decimal)vectorLength / (processCount - 1));
            _localResult = new int[_rowsPerProcessCount];
        }

        private static void InitializeInputData(int vectorLength)
        {
            _matrix = DataGenerator.GenerateMatrix(vectorLength);
            _vector = DataGenerator.GenerateVector(vectorLength);
        }

        private static void InitializeRowDistributionData(int vectorLength, int processCount)
        {
            _rowProcessDistribution = new Dictionary<int, int[]>();
            var unassignedRows = Enumerable.Range(0, vectorLength).ToList();
            var processes = Enumerable.Range(1, processCount - 1);
            foreach (var process in processes)
            {
                if (!unassignedRows.Any())
                    break;

                _rowProcessDistribution.Add(process, unassignedRows.Take(_rowsPerProcessCount).ToArray());
                if (unassignedRows.Count >= _rowsPerProcessCount)
                {
                    unassignedRows.RemoveRange(0, _rowsPerProcessCount);
                }
                else
                {
                    unassignedRows.Clear();
                }
            }
        }

        private static void PrintInput()
        {
            Console.WriteLine("Initial matrix:");
            for (int i = 0; i < _matrix.GetLength(0); i++)
            {
                for (int j = 0; j < _matrix.GetLength(1); j++)
                {
                    Console.Write($"{_matrix[i,j]}\t");
                }
                Console.Write('\n');
            }

            Console.WriteLine("Initial vector:");
            for (int i = 0; i < _vector.Length; i++)
            {
                Console.Write($"{_vector[i]}\t");
            }
            Console.Write('\n');
        }

        private static void PrintFinalOutput()
        {

            Console.WriteLine("Resulting vector:");
            for (int i = 0; i < _finalResult.Length; i++)
            {
                Console.Write($"{_finalResult[i]}\t");
            }
            Console.Write('\n');
        }


        private static void PrintLocalOutput(int rank)
        {
            Console.WriteLine($"Local vector for rank {rank}:");
            for (int i = 0; i < _localResult.Length; i++)
            {
                Console.Write($"{_localResult[i]}\t");
            }
            Console.Write('\n');
        }

        private static List<int> GetMatrixRowsList(int[] rows)
        {
            var cells = new List<int>();
            foreach (var row in rows)
            {
                for (int i = 0; i < _matrix.GetLength(1); i++)
                {
                    cells.Add(_matrix[row, i]);
                }
            }

            return cells;
        }

        private static List<int[]> GetSeparateRowsFromList(List<int> cells, int size)
        {
            var rows = new List<int[]>();
            var rowCount = cells.Count / size;
            for (int i = 0; i < rowCount; i++)
            {
                rows.Add(cells.Skip(i * size).Take(size).ToArray());
            }

            return rows;
        }

        private static void FillMultiplicationResultToSize(List<int> multiplicationResult)
        {
            Console.WriteLine(_rowsPerProcessCount);
            Console.WriteLine(multiplicationResult.Count);
            var cellsToFill = _rowsPerProcessCount - multiplicationResult.Count;
            var zeros = Enumerable.Repeat(0, cellsToFill);
            multiplicationResult.AddRange(zeros);
        }
    }

    public enum Tags : int
    {
        MatrixRequest = 1,
        VectorRequest = 2,
        Response = 3
    }
}
