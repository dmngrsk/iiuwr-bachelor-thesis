using Dapper;
using Npgsql;
using System.Linq;
using System.Reflection;
using Thesis.Relinq.Tests.Helpers;
using Thesis.Relinq.Tests.Models;
using Xunit;

namespace Thesis.Relinq.Tests
{
    public class MethodCallTests : TestsBase
    {
        [Fact]
        public void equals()
        {
            // Arrange
            var myQuery = 
                from e in PsqlQueryFactory.Queryable<Employees>(connection)
                where e.EmployeeID.Equals(5)
                select e;

            var myQuery2 = PsqlQueryFactory.Queryable<Employees>(connection)
                .Where(e => e.EmployeeID.Equals(5));
            
            string psqlCommand = "SELECT * FROM Employees WHERE (\"EmployeeID\" = 5);";

            // Act
            var expected = connection.Query<Employees>(psqlCommand);
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtension.EqualByJson(expected, actual);
            AssertExtension.EqualByJson(expected, actual2);
        }

        [Fact]
        public void take()
        {
            // Arrange
            var myQuery = 
                from e in PsqlQueryFactory.Queryable<Employees>(connection)
                select e;

            var myQuery2 = PsqlQueryFactory.Queryable<Employees>(connection)
                .Select(x => x);
            
            string psqlCommand = "SELECT * FROM Employees LIMIT 5;";

            // Act
            var expected = connection.Query<Employees>(psqlCommand);
            var actual = myQuery.Take(5).ToArray();
            var actual2 = myQuery2.Take(5).ToArray();

            // Assert
            AssertExtension.EqualByJson(expected, actual);
            AssertExtension.EqualByJson(expected, actual2);
        }

        [Fact]
        public void skip()
        {
            // Arrange
            var myQuery = 
                from e in PsqlQueryFactory.Queryable<Employees>(connection)
                select e;

            var myQuery2 = PsqlQueryFactory.Queryable<Employees>(connection)
                .Select(x => x);
            
            string psqlCommand = "SELECT * FROM Employees OFFSET 5;";

            // Act
            var expected = connection.Query<Employees>(psqlCommand);
            var actual = myQuery.Skip(5).ToArray();
            var actual2 = myQuery2.Skip(5).ToArray();

            // Assert
            AssertExtension.EqualByJson(expected, actual);
            AssertExtension.EqualByJson(expected, actual2);
        }

        [Fact]
        public void take_and_skip()
        {
            // Arrange
            var myQuery = 
                from e in PsqlQueryFactory.Queryable<Employees>(connection)
                select e;

            var myQuery2 = PsqlQueryFactory.Queryable<Employees>(connection)
                .Select(x => x);
            
            string psqlCommand = "SELECT * FROM Employees OFFSET 5 LIMIT 3;";

            // Act
            var expected = connection.Query<Employees>(psqlCommand);
            var actual = myQuery.Skip(5).Take(3).ToArray();
            var actual2 = myQuery2.Take(3).Skip(5).ToArray();

            // Assert
            AssertExtension.EqualByJson(expected, actual);
            AssertExtension.EqualByJson(expected, actual2);
        }

        [Fact]
        public void any()
        {
            // Arrange
            var myQuery =
                from c in PsqlQueryFactory.Queryable<Customers>(connection)
                where PsqlQueryFactory.Queryable<Orders>(connection)
                    .Any(o => o.CustomerID == c.CustomerID)
                select c;

            var myQuery2 = 
                PsqlQueryFactory.Queryable<Customers>(connection)
                    .Where(c => PsqlQueryFactory.Queryable<Orders>(connection)
                        .Any(o => o.CustomerID == c.CustomerID));

            var psqlCommand = 
                "SELECT * FROM Customers WHERE " + 
                "EXISTS (SELECT * FROM Orders WHERE " +
                "customers.\"CustomerID\" = orders.\"CustomerID\");";

            // Act
            var expected = connection.Query<Customers>(psqlCommand);
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtension.EqualByJson(expected, actual);
            AssertExtension.EqualByJson(expected, actual2);
        }

        [Fact]
        public void all()
        {
            // Arrange
            var myQuery =
                from c in PsqlQueryFactory.Queryable<Customers>(connection)
                where PsqlQueryFactory.Queryable<Orders>(connection)
                    .All(o => o.CustomerID != c.CustomerID)
                select c;

            var myQuery2 = 
                PsqlQueryFactory.Queryable<Customers>(connection)
                    .Where(c => PsqlQueryFactory.Queryable<Orders>(connection)
                        .All(o => o.CustomerID != c.CustomerID));

            var psqlCommand = 
                "SELECT * FROM Customers WHERE " +
                "NOT EXISTS (SELECT * FROM Orders WHERE " +
                "NOT (customers.\"CustomerID\" != orders.\"CustomerID\"))";

            // Act
            var expected = connection.Query<Customers>(psqlCommand);
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtension.EqualByJson(expected, actual);
            AssertExtension.EqualByJson(expected, actual2);
        }

        [Fact]
        public void union()
        {
            // Arrange
            var myQuery = 
                (from c in PsqlQueryFactory.Queryable<Customers>(connection)
                where c.City == "London"
                select c)
            .Union(
                from c in PsqlQueryFactory.Queryable<Customers>(connection)
                where c.City == "Paris"
                select c);

            var myQuery2 = PsqlQueryFactory.Queryable<Customers>(connection)
                .Where(c => c.City == "London")
            .Union(PsqlQueryFactory.Queryable<Customers>(connection)
                .Where(c => c.City == "Paris"));

            var psqlCommand = 
                "(SELECT * FROM Customers WHERE \"City\" = 'London') " +
                "UNION " +
                "(SELECT * FROM Customers WHERE \"City\" = 'Paris');";

            // Act
            var expected = connection.Query<Customers>(psqlCommand);
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtension.EqualByJson(expected, actual);
            AssertExtension.EqualByJson(expected, actual2);
        }

        [Fact]
        public void concat_as_union_all()
        {
            // Arrange
            var myQuery = 
                    (from c in PsqlQueryFactory.Queryable<Customers>(connection)
                    select c.City)
                .Concat(
                    (from c in PsqlQueryFactory.Queryable<Customers>(connection)
                    select c.City));

            var myQuery2 = PsqlQueryFactory.Queryable<Customers>(connection)
                .Select(c => c.City)
            .Concat(PsqlQueryFactory.Queryable<Customers>(connection)
                .Select(c => c.City));

            var psqlCommand = 
                "(SELECT \"City\" FROM Customers) " +
                "UNION ALL " +
                "(SELECT \"City\" FROM Customers);";

            // Act
            var expected = connection.Query<string>(psqlCommand);
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtension.EqualByJson(expected, actual);
            AssertExtension.EqualByJson(expected, actual2);
        }

        [Fact]
        public void intersect()
        {
            // Arrange
            var myQuery = 
                (from e in PsqlQueryFactory.Queryable<Employees>(connection)
                where e.EmployeeID < 7
                select e)
            .Intersect(
                from e in PsqlQueryFactory.Queryable<Employees>(connection)
                where e.EmployeeID > 3
                select e);

            var myQuery2 = PsqlQueryFactory.Queryable<Employees>(connection)
                .Where(e => e.EmployeeID < 7)
            .Intersect(PsqlQueryFactory.Queryable<Employees>(connection)
                .Where(e => e.EmployeeID > 3));

            var psqlCommand = 
                "(SELECT * FROM Employees WHERE \"EmployeeID\" < 7) " +
                "INTERSECT " +
                "(SELECT * FROM Employees WHERE \"EmployeeID\" > 3);";

            // Act
            var expected = connection.Query<Employees>(psqlCommand);
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtension.EqualByJson(expected, actual);
            AssertExtension.EqualByJson(expected, actual2);
        }

        [Fact]
        public void except()
        {
            // Arrange
            var myQuery =
                (from e in PsqlQueryFactory.Queryable<Employees>(connection)
                select e)
            .Except(
                from e in PsqlQueryFactory.Queryable<Employees>(connection)
                where e.EmployeeID > 6
                select e);

            var myQuery2 = PsqlQueryFactory.Queryable<Employees>(connection)
                .Select(e => e)
            .Except(PsqlQueryFactory.Queryable<Employees>(connection)
                .Where(e => e.EmployeeID > 6));

            var psqlCommand = 
                "(SELECT * FROM Employees) EXCEPT " +
                "(SELECT * FROM Employees WHERE \"EmployeeID\" > 6);";

            // Act
            var expected = connection.Query<Employees>(psqlCommand);
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtension.EqualByJson(expected, actual);
            AssertExtension.EqualByJson(expected, actual2);
        }
    }
}