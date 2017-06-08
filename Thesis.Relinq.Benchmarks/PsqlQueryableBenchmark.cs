using BenchmarkDotNet.Attributes;
using Npgsql;
using System.Linq;
using Thesis.Relinq.Benchmarks.Helpers;
using Thesis.Relinq.Benchmarks.Models;

namespace Thesis.Relinq.Benchmarks
{
    public class PsqlQueryableBenchmark
    {
        private static NpgsqlConnectionAdapter adapter = new NpgsqlConnectionAdapter
        {
            Server = "localhost",
            Port = 5432,
            Username = "dmngrsk",
            Password = "qwerty",
            Database = "northwind"
        };

        private static NpgsqlConnection connection = adapter.GetConnection();



        [Benchmark]
        public void select_all()
        {
            var myQuery = 
                from c in PsqlQueryFactory.Queryable<Customers>(connection)
                select c;

            var executedQuery = myQuery.ToArray();
        }

        [Benchmark]
        public void select_columns_creating_an_anonymous_type()
        {
            var myQuery = 
                from c in PsqlQueryFactory.Queryable<Customers>(connection)
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
                from c in PsqlQueryFactory.Queryable<Customers>(connection)
                where c.CustomerID == "PARIS" 
                select c;

            var executedQuery = myQuery.ToArray();
        }

        [Benchmark]
        public void select_with_multiconditional_where()
        {
            var myQuery = 
                from e in PsqlQueryFactory.Queryable<Employees>(connection)
                where e.EmployeeID > 5 && e.City == "London" 
                select e;

            var executedQuery = myQuery.ToArray();
        }

        [Benchmark]
        public void select_with_multiple_wheres()
        {
            var myQuery = 
                from e in PsqlQueryFactory.Queryable<Employees>(connection)
                where e.EmployeeID > 5
                where e.City == "London" 
                select e;

            var executedQuery = myQuery.ToArray();
        }
        
        [Benchmark]
        public void select_with_case()
        {
            var myQuery = 
                from e in PsqlQueryFactory.Queryable<Employees>(connection)
                where e.EmployeeID < 8
                select new 
                {
                    EmployeeID = e.EmployeeID, 
                    CaseResult = (e.EmployeeID < 5 ? "smaller than five" :
                                  e.EmployeeID == 5 ? "equal to five" :
                                  "larger than five")
                };

            var executedQuery = myQuery.ToArray();
        }

        [Benchmark]
        public void handles_string_addition()
        {
            var myQuery = 
                from e in PsqlQueryFactory.Queryable<Employees>(connection)
                select e.FirstName + " " + e.LastName + " has ID: " + e.EmployeeID;

            var executedQuery = myQuery.ToArray();
        }

        [Benchmark]
        public void logical_not_applied()
        {
            var myQuery = 
                from c in PsqlQueryFactory.Queryable<Employees>(connection)
                where !(c.EmployeeID < 7 && c.EmployeeID > 3)
                select c;

            var executedQuery = myQuery.ToArray();
        }

        [Benchmark]
        public void select_with_multiple_orderings_joined()
        {
            var myQuery = 
                from e in PsqlQueryFactory.Queryable<Employees>(connection)
                orderby e.City, e.EmployeeID descending
                select e;

            var executedQuery = myQuery.ToArray();
        }

        [Benchmark]
        public void select_with_multiple_orderings_split()
        {
            var myQuery = 
                from e in PsqlQueryFactory.Queryable<Employees>(connection)
                orderby e.EmployeeID descending
                orderby e.City
                select e;

            var executedQuery = myQuery.ToArray();
        }

        [Benchmark]
        public void select_with_additional_from_as_cross_join()
        {
            var myQuery =
                from o in PsqlQueryFactory.Queryable<Orders>(connection)
                from e in PsqlQueryFactory.Queryable<Employees>(connection)
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
                from c in PsqlQueryFactory.Queryable<Customers>(connection)
                join o in PsqlQueryFactory.Queryable<Orders>(connection)
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
                from c in PsqlQueryFactory.Queryable<Customers>(connection)
                join o in PsqlQueryFactory.Queryable<Orders>(connection) 
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
                from c in PsqlQueryFactory.Queryable<Customers>(connection)
                join o in PsqlQueryFactory.Queryable<Orders>(connection)
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
        public void any()
        {
            var myQuery =
                from c in PsqlQueryFactory.Queryable<Customers>(connection)
                where PsqlQueryFactory.Queryable<Orders>(connection)
                    .Any(o => o.CustomerID == c.CustomerID)
                select c;

            var executedQuery = myQuery.ToArray();
        }

        [Benchmark]
        public void all()
        {
            var myQuery =
                from c in PsqlQueryFactory.Queryable<Customers>(connection)
                where PsqlQueryFactory.Queryable<Orders>(connection)
                    .All(o => o.CustomerID != c.CustomerID)
                select c;

            var executedQuery = myQuery.ToArray();
        }

        [Benchmark]
        public void union()
        {
            var myQuery = 
                (from c in PsqlQueryFactory.Queryable<Customers>(connection)
                where c.City == "London"
                select c)
            .Union(
                from c in PsqlQueryFactory.Queryable<Customers>(connection)
                where c.City == "Paris"
                select c);

            var executedQuery = myQuery.ToArray();
        }

        [Benchmark]
        public void concat_as_union_all()
        {
            var myQuery = 
                (from c in PsqlQueryFactory.Queryable<Customers>(connection)
                select c.City)
            .Concat(
                (from c in PsqlQueryFactory.Queryable<Customers>(connection)
                select c.City));

            var executedQuery = myQuery.ToArray();
        }

        [Benchmark]
        public void intersect()
        {
            var myQuery = 
                (from e in PsqlQueryFactory.Queryable<Employees>(connection)
                where e.EmployeeID < 7
                select e)
            .Intersect(
                from e in PsqlQueryFactory.Queryable<Employees>(connection)
                where e.EmployeeID > 3
                select e);

            var executedQuery = myQuery.ToArray();
        }

        [Benchmark]
        public void except()
        {
            var myQuery =
                (from e in PsqlQueryFactory.Queryable<Employees>(connection)
                select e)
            .Except(
                from e in PsqlQueryFactory.Queryable<Employees>(connection)
                where e.EmployeeID > 6
                select e);

            var executedQuery = myQuery.ToArray();
        }
    }
}
