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
                var starterList = ImmutableList.CreateRange(Enumerable.Range(5, 100));
                var expected = ImmutableList.CreateRange(Enumerable.Range(5, 150));
                var diff = Enumerable.Range(105, 50).ToArray();
                var diffList = ImmutableList.CreateRange(diff);

                // Test
                VerifyListsEqual(expected, starterList.AddRange(diff));
                VerifyListsEqual(expected, starterList.AddRangeOriginal(diff));

                int iterations = 100000;

                Time("Optimized", iterations, () => starterList.AddRange(diff));
                Time("Original", iterations, () => starterList.AddRangeOriginal(diff));
                Time("Optimized List", iterations, () => starterList.AddRange(diffList));
                Time("Original List", iterations, () => starterList.AddRangeOriginal(diffList));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
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

        private static void Time(string label, int iterations, Action f)
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
