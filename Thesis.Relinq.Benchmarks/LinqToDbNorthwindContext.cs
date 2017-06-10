using LinqToDB;
using LinqToDB.Data;

namespace Thesis.Relinq.Benchmarks
{
    public class LinqToDbNorthwindContext : DataConnection
    {
        public LinqToDbNorthwindContext() : base("NorthwindDataContextConnectionString") { }

        public ITable<LinqToDbPOCOs.Customer> Customers => GetTable<LinqToDbPOCOs.Customer>();
        public ITable<LinqToDbPOCOs.Employee> Employees => GetTable<LinqToDbPOCOs.Employee>();
        public ITable<LinqToDbPOCOs.Order> Orders => GetTable<LinqToDbPOCOs.Order>();
    }
}
