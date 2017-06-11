using System.Linq;
using Dapper;
using Thesis.Relinq.Tests.Helpers;
using Thesis.Relinq.Tests.Models;
using Xunit;

namespace Thesis.Relinq.Tests
{
    public class MethodCallTests : TestClassBase
    {
        [Fact]
        public void equals()
        {
            // Arrange
            var myQuery = 
                from e in Context.Employees
                where e.EmployeeID.Equals(5)
                select e;

            var myQuery2 = Context.Employees
                .Where(e => e.EmployeeID.Equals(5));
            
            string psqlCommand = "SELECT * FROM Employees WHERE (\"EmployeeID\" = 5);";

            // Act
            var expected = Connection.Query<Employee>(psqlCommand);
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void take()
        {
            // Arrange
            var myQuery = 
                from e in Context.Employees
                select e;

            var myQuery2 = Context.Employees
                .Select(x => x);
            
            string psqlCommand = "SELECT * FROM Employees LIMIT 5;";

            // Act
            var expected = Connection.Query<Employee>(psqlCommand);
            var actual = myQuery.Take(5).ToArray();
            var actual2 = myQuery2.Take(5).ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void skip()
        {
            // Arrange
            var myQuery = 
                from e in Context.Employees
                select e;

            var myQuery2 = Context.Employees
                .Select(x => x);
            
            string psqlCommand = "SELECT * FROM Employees OFFSET 5;";

            // Act
            var expected = Connection.Query<Employee>(psqlCommand);
            var actual = myQuery.Skip(5).ToArray();
            var actual2 = myQuery2.Skip(5).ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void take_and_skip()
        {
            // Arrange
            var myQuery = 
                from e in Context.Employees
                select e;

            var myQuery2 = Context.Employees
                .Select(x => x);
            
            string psqlCommand = "SELECT * FROM Employees OFFSET 5 LIMIT 3;";

            // Act
            var expected = Connection.Query<Employee>(psqlCommand);
            var actual = myQuery.Skip(5).Take(3).ToArray();
            var actual2 = myQuery2.Take(3).Skip(5).ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void any()
        {
            // Arrange
            var myQuery =
                from c in Context.Customers
                where Context.Orders
                    .Any(o => o.CustomerID == c.CustomerID)
                select c;

            var myQuery2 = 
                Context.Customers
                    .Where(c => Context.Orders
                        .Any(o => o.CustomerID == c.CustomerID));

            var psqlCommand = 
                "SELECT * FROM Customers WHERE " + 
                "EXISTS (SELECT * FROM Orders WHERE " +
                "customers.\"CustomerID\" = orders.\"CustomerID\");";

            // Act
            var expected = Connection.Query<Customer>(psqlCommand);
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void all()
        {
            // Arrange
            var myQuery =
                from c in Context.Customers
                where Context.Orders
                    .All(o => o.CustomerID != c.CustomerID)
                select c;

            var myQuery2 = 
                Context.Customers
                    .Where(c => Context.Orders
                        .All(o => o.CustomerID != c.CustomerID));

            var psqlCommand = 
                "SELECT * FROM Customers WHERE " +
                "NOT EXISTS (SELECT * FROM Orders WHERE " +
                "NOT (customers.\"CustomerID\" != orders.\"CustomerID\"))";

            // Act
            var expected = Connection.Query<Customer>(psqlCommand);
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void union()
        {
            // Arrange
            var myQuery = 
                (from c in Context.Customers
                where c.City == "London"
                select c)
            .Union(
                from c in Context.Customers
                where c.City == "Paris"
                select c);

            var myQuery2 = Context.Customers
                .Where(c => c.City == "London")
            .Union(Context.Customers
                .Where(c => c.City == "Paris"));

            var psqlCommand = 
                "(SELECT * FROM Customers WHERE \"City\" = 'London') " +
                "UNION " +
                "(SELECT * FROM Customers WHERE \"City\" = 'Paris');";

            // Act
            var expected = Connection.Query<Customer>(psqlCommand);
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void concat_as_union_all()
        {
            // Arrange
            var myQuery = 
                    (from c in Context.Customers
                    select c.City)
                .Concat(
                    (from c in Context.Customers
                    select c.City));

            var myQuery2 = Context.Customers
                .Select(c => c.City)
            .Concat(Context.Customers
                .Select(c => c.City));

            var psqlCommand = 
                "(SELECT \"City\" FROM Customers) " +
                "UNION ALL " +
                "(SELECT \"City\" FROM Customers);";

            // Act
            var expected = Connection.Query<string>(psqlCommand);
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void intersect()
        {
            // Arrange
            var myQuery = 
                (from e in Context.Employees
                where e.EmployeeID < 7
                select e)
            .Intersect(
                from e in Context.Employees
                where e.EmployeeID > 3
                select e);

            var myQuery2 = Context.Employees
                .Where(e => e.EmployeeID < 7)
            .Intersect(Context.Employees
                .Where(e => e.EmployeeID > 3));

            var psqlCommand = 
                "(SELECT * FROM Employees WHERE \"EmployeeID\" < 7) " +
                "INTERSECT " +
                "(SELECT * FROM Employees WHERE \"EmployeeID\" > 3);";

            // Act
            var expected = Connection.Query<Employee>(psqlCommand);
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void except()
        {
            // Arrange
            var myQuery =
                (from e in Context.Employees
                select e)
            .Except(
                from e in Context.Employees
                where e.EmployeeID > 6
                select e);

            var myQuery2 = Context.Employees
                .Select(e => e)
            .Except(Context.Employees
                .Where(e => e.EmployeeID > 6));

            var psqlCommand = 
                "(SELECT * FROM Employees) EXCEPT " +
                "(SELECT * FROM Employees WHERE \"EmployeeID\" > 6);";

            // Act
            var expected = Connection.Query<Employee>(psqlCommand);
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }
    }
}