namespace ImmutableCollectionsPerformanceTests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                //BenchmarkAddRange();
                BenchmarkFindAll();
                BenchmarkConvertAll();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void BenchmarkFindAll()
        {
            Console.WriteLine("Benchmarking FindAll:");
            int seed = (int)DateTime.Now.Ticks;
            Console.WriteLine("Using random seed {0}", seed);
            var random = new Random(seed);

            int count = 100000;

            var items = ImmutableList<int>.Empty.AddRange(Enumerable.Repeat(0, count).Select(i => random.Next(100)));
            Predicate<int> p = x => x < 50;

            // Test
            VerifyListsEqual(items.FindAllOriginal(p), items.FindAll(p));

            int iterations = 20;
            Benchmark("Optimized", iterations, () => items.FindAll(p));
            Benchmark("Original", iterations, () => items.FindAllOriginal(p));

            Console.WriteLine();
        }

        private static void BenchmarkConvertAll()
        {
            Console.WriteLine("Benchmarking ConvertAll:");
            int seed = (int)DateTime.Now.Ticks;
            Console.WriteLine("Using random seed {0}", seed);
            var random = new Random(seed);

            int count = 100000;

            var items = ImmutableList<int>.Empty.AddRange(Enumerable.Repeat(0, count).Select(i => random.Next(100)));
            Func<int, bool> p = x => x < 50;

            // Test
            VerifyListsEqual(items.ConvertAllOriginal(p), items.ConvertAll(p));

            int iterations = 20;
            Benchmark("Optimized", iterations, () => items.ConvertAll(p));
            Benchmark("Original", iterations, () => items.ConvertAllOriginal(p));

            Console.WriteLine();
        }

        private static void BenchmarkAddRange()
        {
            var starterList = ImmutableList.CreateRange(Enumerable.Range(5, 100));
            var expected = ImmutableList.CreateRange(Enumerable.Range(5, 150));
            var diff = Enumerable.Range(105, 50).ToArray();
            var diffList = ImmutableList.CreateRange(diff);

            // Test
            VerifyListsEqual(expected, starterList.AddRange(diff));
            VerifyListsEqual(expected, starterList.AddRangeOriginal(diff));

            int iterations = 100000;

            Benchmark("Optimized", iterations, () => starterList.AddRange(diff));
            Benchmark("Original", iterations, () => starterList.AddRangeOriginal(diff));
            Benchmark("Optimized List", iterations, () => starterList.AddRange(diffList));
            Benchmark("Original List", iterations, () => starterList.AddRangeOriginal(diffList));
        }

        private static void VerifyListsEqual<T>(IList<T> expected, IList<T> actual)
        {
            if (expected.Count != actual.Count)
            {
                throw new Exception(string.Format("List lengths differ: expected {0}, actual {1}", expected.Count, actual.Count));
            }

            for (int i = 0; i < expected.Count; i++)
            {
                if (!EqualityComparer<T>.Default.Equals(expected[i], actual[i]))
                {
                    throw new Exception(string.Format("List differ at position {0}: expected {1}, actual {2}", i, expected, actual));
                }
            }
        }

        private static void Benchmark(string label, int iterations, Action f)
        {
            // Warmup
            f.Invoke();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var sw = System.Diagnostics.Stopwatch.StartNew();

            for (int i = 0; i < iterations; i++)
            {
                f.Invoke();
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var elapsed = sw.Elapsed;

            Console.WriteLine("{0}; iterations: {1}; Time: {2}", label, iterations, elapsed);
        }
    }
}
