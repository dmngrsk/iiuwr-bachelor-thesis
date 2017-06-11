using System.Data.Common;
using Thesis.Relinq.Tests.Models;

namespace Thesis.Relinq.Tests.Helpers
{
    public class NorthwindContext
    {
        public PsqlQueryable<Order> Orders { get; }
        public PsqlQueryable<Customer> Customers { get; }
        public PsqlQueryable<Employee> Employees { get; }

        public NorthwindContext(DbConnection connection)
        {
            this.Customers = PsqlQueryFactory.Queryable<Customer>(connection);
            this.Employees = PsqlQueryFactory.Queryable<Employee>(connection);
            this.Orders = PsqlQueryFactory.Queryable<Order>(connection);
        }
    }
}