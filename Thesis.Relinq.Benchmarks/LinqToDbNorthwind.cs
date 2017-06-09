using LinqToDB;
using LinqToDB.Data;

namespace Thesis.Relinq.Benchmarks
{
    public class LinqToDbNorthwind : DataConnection
    {
        public LinqToDbNorthwind() : base("NorthwindDataContextConnectionString") { }

        public ITable<LinqToDbPOCOs.Customer> Customers => GetTable<LinqToDbPOCOs.Customer>();
        public ITable<LinqToDbPOCOs.Employee> Employees => GetTable<LinqToDbPOCOs.Employee>();
        public ITable<LinqToDbPOCOs.Order> Orders => GetTable<LinqToDbPOCOs.Order>();
    }
}
