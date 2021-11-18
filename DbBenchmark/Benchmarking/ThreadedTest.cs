using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Reflection;
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

                var generatedParams = parameterGenerator.GenerateParams(_testQueries[idx].Params);
                var paramsToExecute = new List<object>();
                var types = new List<Type>();
                var dbObject = "DbBenchmark.ORM.DAO." + _testQueries[idx].DbObject;
                var methods = Type.GetType(dbObject)?.GetMethods();
                MethodInfo methodToExecute = null;
                foreach (var methodInfo in methods)
                {
                    if (methodInfo.Name == _testQueries[idx].Method)
                    {
                        var args = methodInfo.GetParameters();
                        var paramCount = 0;
                        foreach (var parameterInfo in args)
                        {
                            types.Add(parameterInfo.GetType());
                            switch (parameterInfo.Name.ToLower())
                            {
                                case "db":
                                case "connection":
                                    paramsToExecute.Add(_conn);
                                    continue;
                                    
                                case "relationignore":
                                    paramsToExecute.Add(true);
                                    continue;
                                default:
                                    paramsToExecute.Add(generatedParams[paramCount]);
                                    paramCount += 1;
                                    break;
                            }
                        }

                        if (paramCount == generatedParams.Length)
                        {
                            methodToExecute = methodInfo;
                            break;
                        }
                        // if it is not the method I want to Invoke, clear params and types
                        paramsToExecute.Clear();
                        types.Clear();
                    }
                }

                if (methodToExecute == null)
                {
                    throw new System.Exception(@"Could not find method to Invoke");
                }
                else
                {
                    methodToExecute.Invoke(null, paramsToExecute.ToArray());
                }

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