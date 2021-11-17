using System;
using System.IO;
using System.Threading;
using DbBenchmark.Benchmarking;
using DbBenchmark.ORM.DAO;

namespace DbBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var baseDir = Path.GetFullPath(@"..\..\..\");
            var configFilePath = Path.Join(baseDir, @"Benchmarking\files\queries.txt");
            var config = BenchmarkConfig.ReadConfigFile(configFilePath);
            // new Benchmark(config.WithThreadCount(1)).Run();
            // Console.WriteLine(@"Finished");
            var tt = new ThreadedTest(1, new DatabaseConnection(), config.TestQueries);
            tt.Run();
        }
    }
}