using System.Data.Common;
using Thesis.Relinq.Tests.Models;

namespace Thesis.Relinq.Benchmarks
{
    public class ThesisNorthwindContext
    {
        public PsqlQueryable<Orders> Orders { get; set; }
        public PsqlQueryable<Customers> Customers { get; set; }
        public PsqlQueryable<Employees> Employees { get; set; }

        public ThesisNorthwindContext(DbConnection connection)
        {
            this.Customers = PsqlQueryFactory.Queryable<Customers>(connection);
            this.Employees = PsqlQueryFactory.Queryable<Employees>(connection);
            this.Orders = PsqlQueryFactory.Queryable<Orders>(connection);
        }
    }
}