using System.Configuration;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Npgsql;
using Thesis.Relinq.Benchmarks.ThesisPOCOs;

namespace Thesis.Relinq.Benchmarks
{
    public class ThesisBenchmark
    {   
        private static readonly NpgsqlConnection Connection = new NpgsqlConnection(
            ConfigurationManager.ConnectionStrings["NorthwindDataContextConnectionString"].ConnectionString);



        [Benchmark]
        public void select_all()
        {
            var myQuery = 
                from c in PsqlQueryFactory.Queryable<Customers>(Connection)
                select c;

            var executedQuery = myQuery.ToArray();
        }

        [Benchmark]
        public void select_columns_creating_an_anonymous_type()
        {
            var myQuery = 
                from c in PsqlQueryFactory.Queryable<Customers>(Connection)
                select new
                {
                    Name = c.ContactName,
                    City = c.City
                };

            var executedQuery = myQuery.ToArray();
        }

        [Benchmark]
        public void select_with_where()
        {
            var myQuery = 
                from c in PsqlQueryFactory.Queryable<Customers>(Connection)
                where c.CustomerID == "PARIS" 
                select c;

            var executedQuery = myQuery.ToArray();
        }

        [Benchmark]
        public void select_with_multiconditional_where()
        {
            var myQuery = 
                from e in PsqlQueryFactory.Queryable<Employees>(Connection)
                where e.EmployeeID > 5 && e.City == "London" 
                select e;

            var executedQuery = myQuery.ToArray();
        }

        [Benchmark]
        public void select_with_multiple_wheres()
        {
            var myQuery = 
                from e in PsqlQueryFactory.Queryable<Employees>(Connection)
                where e.EmployeeID > 5
                where e.City == "London" 
                select e;

            var executedQuery = myQuery.ToArray();
        }
        
        [Benchmark]
        public void select_with_case()
        {
            var myQuery = 
                from e in PsqlQueryFactory.Queryable<Employees>(Connection)
                where e.EmployeeID < 8
                select new 
                {
                    EmployeeID = e.EmployeeID, 
                    CaseResult = (e.EmployeeID < 5 
                        ? "smaller than five" 
                        : e.EmployeeID == 5 
                            ? "equal to five"
                            : "larger than five")
                };

            var executedQuery = myQuery.ToArray();
        }

        [Benchmark]
        public void select_with_multiple_orderings_joined()
        {
            var myQuery = 
                from e in PsqlQueryFactory.Queryable<Employees>(Connection)
                orderby e.City, e.EmployeeID descending
                select e;

            var executedQuery = myQuery.ToArray();
        }

        [Benchmark]
        public void select_with_multiple_orderings_split()
        {
            var myQuery = 
                from e in PsqlQueryFactory.Queryable<Employees>(Connection)
                orderby e.EmployeeID descending
                orderby e.City
                select e;

            var executedQuery = myQuery.ToArray();
        }

        [Benchmark]
        public void select_with_take_while()
        {
            var myQuery = PsqlQueryFactory.Queryable<Employees>(Connection)
                .TakeWhile(e => e.EmployeeID < 5);

            var executedQuery = myQuery.ToArray();
        }

        [Benchmark]
        public void select_with_additional_from_as_cross_join()
        {
            var myQuery =
                from o in PsqlQueryFactory.Queryable<Orders>(Connection)
                from e in PsqlQueryFactory.Queryable<Employees>(Connection)
                where o.EmployeeID == e.EmployeeID
                select new
                {
                    Employee = o.EmployeeID,
                    Order = o.OrderID
                };

            var executedQuery = myQuery.ToArray();
        }

        [Benchmark]
        public void select_with_inner_join()
        {
            var myQuery = 
                from c in PsqlQueryFactory.Queryable<Customers>(Connection)
                join o in PsqlQueryFactory.Queryable<Orders>(Connection)
                on c.CustomerID equals o.CustomerID
                select new
                {
                    Name = c.ContactName,
                    Order = o.OrderID
                };
            
            var executedQuery = myQuery.ToArray();
        }

        [Benchmark]
        public void select_with_group_join()
        {
            var myQuery =
                from c in PsqlQueryFactory.Queryable<Customers>(Connection)
                join o in PsqlQueryFactory.Queryable<Orders>(Connection) 
                on c.CustomerID equals o.CustomerID into orders
                select new
                {
                    Customer = c.CustomerID,
                    Orders = orders
                };
                
            var executedQuery = myQuery.ToArray();
        }

        [Benchmark]
        public void select_with_outer_join()
        {
            var myQuery = 
                from c in PsqlQueryFactory.Queryable<Customers>(Connection)
                join o in PsqlQueryFactory.Queryable<Orders>(Connection)
                on c.CustomerID equals o.CustomerID into joined
                from j in joined.DefaultIfEmpty()
                select new
                {
                    CustomerID = c.CustomerID,
                    OrderID = j.OrderID
                };

            var executedQuery = myQuery.ToArray();
        }

        [Benchmark]
        public void select_with_paging()
        {
            var myQuery = PsqlQueryFactory.Queryable<Employees>(Connection)
                .Select(x => x).Take(5).Skip(3);

            var executedQuery = myQuery.ToArray();
        }

        [Benchmark]
        public void select_where_any_matches_condition()
        {
            var myQuery =
                from c in PsqlQueryFactory.Queryable<Customers>(Connection)
                where PsqlQueryFactory.Queryable<Orders>(Connection)
                    .Any(o => o.CustomerID == c.CustomerID)
                select c;

            var executedQuery = myQuery.ToArray();
        }

        [Benchmark]
        public void select_where_all_match_condition()
        {
            var myQuery =
                from c in PsqlQueryFactory.Queryable<Customers>(Connection)
                where PsqlQueryFactory.Queryable<Orders>(Connection)
                    .All(o => o.CustomerID != c.CustomerID)
                select c;

            var executedQuery = myQuery.ToArray();
        }

        [Benchmark]
        public void select_with_union()
        {
            var myQuery = 
                (from c in PsqlQueryFactory.Queryable<Customers>(Connection)
                where c.City == "London"
                select c)
            .Union(
                from c in PsqlQueryFactory.Queryable<Customers>(Connection)
                where c.City == "Paris"
                select c);

            var executedQuery = myQuery.ToArray();
        }

        [Benchmark]
        public void select_with_concat_as_union_all()
        {
            var myQuery = 
                (from c in PsqlQueryFactory.Queryable<Customers>(Connection)
                select c.City)
            .Concat(
                (from c in PsqlQueryFactory.Queryable<Customers>(Connection)
                select c.City));

            var executedQuery = myQuery.ToArray();
        }

        [Benchmark]
        public void select_with_intersect()
        {
            var myQuery = 
                (from e in PsqlQueryFactory.Queryable<Employees>(Connection)
                where e.EmployeeID < 7
                select e)
            .Intersect(
                from e in PsqlQueryFactory.Queryable<Employees>(Connection)
                where e.EmployeeID > 3
                select e);

            var executedQuery = myQuery.ToArray();
        }

        [Benchmark]
        public void select_with_except()
        {
            var myQuery =
                (from e in PsqlQueryFactory.Queryable<Employees>(Connection)
                select e)
            .Except(
                from e in PsqlQueryFactory.Queryable<Employees>(Connection)
                where e.EmployeeID > 6
                select e);

            var executedQuery = myQuery.ToArray();
        }
    }
}
