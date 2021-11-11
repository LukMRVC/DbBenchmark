using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
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
            var parameterGenerator = new FakeParameterGenerator();
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

                // TODO: Generate params
                var dictOfParams = parameterGenerator.GenerateParams(_testQueries[idx].Params);
                var args = new object[dictOfParams.Count];
                dictOfParams.Values.CopyTo(args, 0);
                // TODO: Fix this, add DatabaseConnection Type and `ignoreRelation` 
                var types = new List<Type>();
                foreach (var methodVal in args)
                {
                    types.Add(methodVal.GetType());
                }
                var dbObject = "DbBenchmark.ORM.DAO" + _testQueries[idx].DbObject;
                Type.GetType(dbObject)?.GetMethod(_testQueries[idx].Method, types.ToArray()).Invoke(null, args);

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