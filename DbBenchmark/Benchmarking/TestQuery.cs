using System;

namespace DbBenchmark.Benchmarking
{
    public struct TestQuery
    {
        public readonly string DbObject;
        public readonly string Method;
        public readonly string[] Params;
        public readonly int ToExecute;
        public int Executed { get; private set; }

        public bool IsDone => Executed >= ToExecute;

        public TestQuery Inc()
        {
            Executed += 1;
            return this;
        }
        
        public TestQuery(string dbObject, string method, string[] @params, int toExecute)
        {
            DbObject = dbObject;
            Method = method;
            Params = @params;
            ToExecute = toExecute;
            Executed = 0;
        }
    }
}