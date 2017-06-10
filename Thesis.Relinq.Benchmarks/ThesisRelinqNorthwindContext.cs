using System.Data.Common;
using Thesis.Relinq.Benchmarks.ThesisPOCOs;

namespace Thesis.Relinq.Benchmarks
{
    public class ThesisRelinqNorthwindContext
    {
        public PsqlQueryable<Orders> Orders { get; set; }
        public PsqlQueryable<Customers> Customers { get; set; }
        public PsqlQueryable<Employees> Employees { get; set; }

        public ThesisRelinqNorthwindContext(DbConnection connection)
        {
            this.Customers = PsqlQueryFactory.Queryable<Customers>(connection);
            this.Employees = PsqlQueryFactory.Queryable<Employees>(connection);
            this.Orders = PsqlQueryFactory.Queryable<Orders>(connection);
        }
    }
}