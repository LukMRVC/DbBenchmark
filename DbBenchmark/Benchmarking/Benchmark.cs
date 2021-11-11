using System;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Threading;

namespace DbBenchmark.Benchmarking
{
    public class Benchmark
    {
        private readonly BenchmarkConfig _config;
        private Thread[] _testThreads;
        private ThreadedTest[] _tests;
        public Benchmark(BenchmarkConfig config)
        {
            _config = config;
            _testThreads = new Thread[config.ThreadCount];
            _tests = new ThreadedTest[config.ThreadCount];
        }

        public void Run()
        {
            var connectionPool = new DatabaseConnectionPool();
            for (int i = 0; i < _config.ThreadCount; ++i)
            {
                _tests[i] = new ThreadedTest(i, connectionPool.GetNew(), _config.TestQueries);
            }


            for (int i = 0; i < _config.ThreadCount; i++)
            {
                _testThreads[i] = new Thread(_tests[i].Run);
            }

            var stopwatch = Stopwatch.StartNew();

            for (int i = 0; i < _config.ThreadCount; i++)
            {
                _testThreads[i].Start();
            }
            
            for (int i = 0; i < _config.ThreadCount; i++)
            {
                _testThreads[i].Join();
            }

            int totalExecuted = 0;
            foreach (var testQuery in _config.TestQueries)
            {
                totalExecuted += testQuery.Executed;
            }
            
            Console.WriteLine($"Executed {totalExecuted} ops, elapsed time: {stopwatch.ElapsedMilliseconds}ms");
            stopwatch.Stop();
        }
    }
}