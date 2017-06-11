using System.Data.Common;
using Thesis.Relinq.Benchmarks.ThesisPOCOs;

namespace Thesis.Relinq.Benchmarks
{
    public class ThesisRelinqNorthwindContext
    {
        public PsqlQueryable<Order> Orders { get; }
        public PsqlQueryable<Customer> Customers { get; }
        public PsqlQueryable<Employee> Employees { get; }

        public ThesisRelinqNorthwindContext(DbConnection connection)
        {
            this.Customers = PsqlQueryFactory.Queryable<Customer>(connection);
            this.Employees = PsqlQueryFactory.Queryable<Employee>(connection);
            this.Orders = PsqlQueryFactory.Queryable<Order>(connection);
        }
    }
}