using System.Data.Common;
using Thesis.Relinq.Tests.Models;

namespace Thesis.Relinq.Tests.Helpers
{
    public class NorthwindContext
    {
        public PsqlQueryable<Order> Orders { get; set; }
        public PsqlQueryable<Customer> Customers { get; set; }
        public PsqlQueryable<Employee> Employees { get; set; }

        public NorthwindContext(DbConnection connection)
        {
            this.Customers = PsqlQueryFactory.Queryable<Customer>(connection);
            this.Employees = PsqlQueryFactory.Queryable<Employee>(connection);
            this.Orders = PsqlQueryFactory.Queryable<Order>(connection);
        }
    }
}