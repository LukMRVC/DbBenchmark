using System;
using System.Threading;
using DbBenchmark.ORM.DAO;

namespace DbBenchmark.Benchmarking
{
    public class ThreadedTest
    {
        private readonly int _id;
        private readonly DatabaseConnection _conn;
        private readonly TestQuery[] _testQueries;
        
        public ThreadedTest(int id, DatabaseConnection conn, in TestQuery[] testQueries)
        {
            _id = id;
            _conn = conn;
            _testQueries = testQueries;
        }
        

        public void Run()
        {
            var random = new Random();
            int i = 0;
            while (i < 5_000)
            {
                ++i;
                var idx = random.Next(_testQueries.Length);
                
                if (_testQueries[idx].IsDone)
                {
                    continue;
                }
                
                lock (_testQueries)
                {
                    _testQueries[idx].Inc();
                    Console.WriteLine($"Incrementing on IDX {idx}, Thread: {_id}, currentValue: {_testQueries[idx].Executed}");
                }
                // TODO: Get class, method and params to execute the query on
                // TODO: Execute query outside of lock block, otherwise the requests wouldn't be actually concurrent

                if (ShouldFinish())
                {
                    break;
                }
            }
        }

        private bool ShouldFinish()
        {
            lock (_testQueries)
            {
                foreach (var testQuery in _testQueries)
                {
                    if (!testQuery.IsDone)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }
}