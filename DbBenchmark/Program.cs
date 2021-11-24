using System;
using System.IO;
using DbBenchmark.Benchmarking;

namespace DbBenchmark
{
    class Program
    {
        public static string GoUpNLevels(int levels, string path)
        {
            string newPath = (string) path.Clone();
            for (int i = 0; i < levels; ++i)
            {
                newPath = Path.GetDirectoryName(newPath);
            }

            return newPath;
        }

        static void Main(string[] args)
        {
            int threadCount = 4;
            try
            {
                threadCount = Int32.Parse(args[0]);
            }
            catch (Exception e)
            {}

            var baseDir = GoUpNLevels(4, Path.GetFullPath("./"));
            var configFilePath = Path.Combine(baseDir, "Benchmarking", "files", "queries.txt");
            var config = BenchmarkConfig.ReadConfigFile(configFilePath);
            new Benchmark(config.WithThreadCount(threadCount)).Run();
            Console.WriteLine(@"Finished");
        }
    }
}