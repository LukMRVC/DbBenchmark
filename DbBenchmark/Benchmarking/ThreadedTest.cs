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
                    Console.WriteLine($"T{_id}: Picked {_testQueries[idx].DbObject} {_testQueries[idx].Method} {_testQueries[idx].Params.Length}");
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
                            if (parameterInfo.Name.Equals("page", StringComparison.OrdinalIgnoreCase))
                            {
                                paramsToExecute.Add(random.Next(10_000));
                                continue;
                            }
                            
                            if (parameterInfo.Name.Equals("pagesize", StringComparison.OrdinalIgnoreCase))
                            {
                                paramsToExecute.Add(50);
                                continue;
                            }
                            
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
                    try
                    {
                        methodToExecute.Invoke(null, paramsToExecute.ToArray());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Thread {_id} failed with exception: {e.Message} {e.StackTrace}");
                        throw;
                    }
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