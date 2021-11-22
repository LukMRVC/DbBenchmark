using DbBenchmark.ORM.DAO;

namespace DbBenchmark.Benchmarking
{
    public class DatabaseConnectionPool
    {

        public DatabaseConnection GetNew()
        {
            return new DatabaseConnection(runEveryInTransaction: true);
        }
    }
}