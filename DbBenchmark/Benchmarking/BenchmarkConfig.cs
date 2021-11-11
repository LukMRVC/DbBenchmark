using System;
using System.Collections.Generic;

namespace DbBenchmark.Benchmarking
{
    public class BenchmarkConfig
    {
        public readonly TestQuery[] TestQueries;
        public int ThreadCount;
        public BenchmarkConfig(TestQuery[] queries)
        {
            TestQueries = queries;
            ThreadCount = 1;
        }

        public BenchmarkConfig WithThreadCount(int threadCount)
        {
            ThreadCount = threadCount;
            return this;
        }

        public static BenchmarkConfig ReadConfigFile(string filepath)
        {
            var testQueries = new List<TestQuery>();
            foreach (var line in System.IO.File.ReadLines(filepath))
            {
                if (! line.StartsWith("//"))
                {
                    var queryParams = line.Split(' ');
                    var lst = new List<string>(queryParams);
                    if (!queryParams[0].EndsWith("Table"))
                    {
                        queryParams[0] += "Table";
                    }
                    
                    testQueries.Add(
                        new TestQuery(
                            queryParams[0],
                            queryParams[1],
                            lst.GetRange(3, Int32.Parse(queryParams[2])).ToArray(),
                            Int32.Parse(queryParams[^1]))
                        );
                }
            }
            return new BenchmarkConfig(testQueries.ToArray());
        }
    }
}