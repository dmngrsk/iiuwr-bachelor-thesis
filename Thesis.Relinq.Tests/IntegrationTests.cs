using System.Data.Common;
using System.Linq;
using System.Reflection;
using Dapper;
using Thesis.Relinq.Tests.Helpers;
using Thesis.Relinq.Tests.Models;
using Xunit;

namespace Thesis.Relinq.Tests
{
    public class IntegrationTests : TestClassBase
    {
        [Fact]
        public void select_all()
        {
            // Arrange
            var myQuery = 
                from c in Context.Customers
                select c;

            var myQuery2 = Context.Customers
                .Select(c => c);
            
            string psqlCommand = "SELECT * FROM Customers;";

            // Act
            var expected = Connection.Query<Customer>(psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void select_columns_creating_an_anonymous_type()
        {
            // Arrange
            var myQuery = 
                from c in Context.Customers
                select new
                {
                    Name = c.ContactName,
                    City = c.City
                };

            var myQuery2 = Context.Customers
                .Select(c => new { Name = c.ContactName, City = c.City });
            
            string psqlCommand = "SELECT \"ContactName\", \"City\" FROM Customers;";
            var queryMethod = typeof(ExtensionMethods)
                .GetMethod("QueryAnonymous", new[] { typeof(DbConnection), typeof(string) })
                .MakeGenericMethod(myQuery.ElementType);

            // Act
            var expected = queryMethod.Invoke(null, new object[] { Connection, psqlCommand });
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();
            
            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void select_with_where()
        {
            // Arrange
            var myQuery = 
                from c in Context.Customers
                where c.CustomerID == "PARIS" 
                select c;
                
            var myQuery2 = Context.Customers
                .Where(c => c.CustomerID == "PARIS");
            
            string psqlCommand = "SELECT * FROM Customers WHERE \"CustomerID\" = 'PARIS';";

            // Act
            var expected = Connection.Query<Customer>(psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();
            
            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void select_with_multiconditional_where()
        {
            // Arrange
            var myQuery = 
                from e in Context.Employees
                where e.EmployeeID > 5 && e.City == "London" 
                select e;

            var myQuery2 = Context.Employees
                .Where(e => e.EmployeeID > 5 && e.City == "London");

            string psqlCommand = 
                "SELECT * FROM Employees " +
                "WHERE \"EmployeeID\" > 5 AND \"City\" = 'London';";

            // Act
            var expected = Connection.Query<Employee>(psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();
            
            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void select_with_multiple_wheres()
        {
            // Arrange
            var myQuery = 
                from e in Context.Employees
                where e.EmployeeID > 5
                where e.City == "London" 
                select e;

            var myQuery2 = Context.Employees
                .Where(e => e.EmployeeID > 5)
                .Where(e => e.City == "London");
            
            string psqlCommand = 
                "SELECT * FROM Employees " +
                "WHERE \"EmployeeID\" > 5 AND \"City\" = 'London';";

            // Act
            var expected = Connection.Query<Employee>(psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();
            
            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void select_with_case()
        {
            // Arrange
            var myQuery = 
                from e in Context.Employees
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

            var myQuery2 = Context.Employees
                .Where(e => e.EmployeeID < 8)
                .Select(e => new
                {
                    EmployeeID = e.EmployeeID, 
                    CaseResult = (e.EmployeeID < 5 
                        ? "smaller than five" 
                        : e.EmployeeID == 5 
                            ? "equal to five"
                            : "larger than five")
                });

            var psqlCommand = 
                "SELECT \"EmployeeID\", CASE " +
                "WHEN \"EmployeeID\" < 5 THEN 'smaller than five' " +
                "WHEN \"EmployeeID\" = 5 THEN 'equal to five' " + 
                "ELSE 'larger than five' END " +
                "FROM employees WHERE \"EmployeeID\" < 8;";
            var queryMethod = typeof(ExtensionMethods)
                .GetMethod("QueryAnonymous", new[] { typeof(DbConnection), typeof(string) })
                .MakeGenericMethod(myQuery.ElementType);

            // Act
            var expected = queryMethod.Invoke(null, new object[] { Connection, psqlCommand });
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void queries_are_sanitized()
        {
            // Arrange
            var rowCountBeforeQuery = Connection.Database.Count();
            var myQuery = 
                from e in Context.Employees
                where e.City == "London'; DROP TABLE Employees;" 
                select e;

            // Act
            var expected = new Employee[0];
            var actual = myQuery.ToArray();
            var rowCountAfterQuery = Connection.Database.Count();

            // Arrange
            Assert.True(rowCountBeforeQuery > 0);
            Assert.Equal(rowCountBeforeQuery, rowCountAfterQuery);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void handles_string_addition()
        {
            var myQuery = 
                from e in Context.Employees
                select e.FirstName + " " + e.LastName + " has ID: " + e.EmployeeID;

            var myQuery2 = Context.Employees
                .Select(e => e.FirstName + " " + e.LastName + " has ID: " + e.EmployeeID);

            var psqlCommand = 
                "SELECT \"FirstName\" || ' ' || \"LastName\" || " +
                "' has ID: ' || \"EmployeeID\" FROM employees;";

            // Act
            var expected = Connection.Query<string>(psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void logical_not_applied()
        {
            // Arrange
            var myQuery = 
                from c in Context.Employees
                where !(c.EmployeeID < 7 && c.EmployeeID > 3)
                select c;

            var myQuery2 = Context.Employees
                .Where(c => !(c.EmployeeID < 7 && c.EmployeeID > 3));
            
            string psqlCommand = 
                "SELECT * FROM Employees " +
                "WHERE NOT (\"EmployeeID\" < 7 AND \"EmployeeID\" > 3);";

            // Act
            var expected = Connection.Query<Employee>(psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void select_empty_result_with_always_false_where()
        {
            // Arrange
            var myQuery = 
                from e in Context.Employees
                where 3 > 5
                select e;

            var myQuery2 = Context.Employees
                .Where(x => 3 > 5);

            string psqlCommand = "SELECT * FROM Employees WHERE 3 > 5;";

            // Act
            var expected = Connection.Query<Employee>(psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void select_with_multiple_orderings_joined()
        {
            // Arrange
            var myQuery = 
                from e in Context.Employees
                orderby e.City, e.EmployeeID descending
                select e;

            var myQuery2 = Context.Employees
                .OrderBy(e => e.City)
                .ThenByDescending(e => e.EmployeeID);

            string psqlCommand = 
                "SELECT * FROM Employees " +
                "ORDER BY \"City\", \"EmployeeID\" DESC;";

            // Act
            var expected = Connection.Query<Employee>(psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void select_with_multiple_orderings_split()
        {
            // Arrange
            var myQuery = 
                from e in Context.Employees
                orderby e.EmployeeID descending
                orderby e.City
                select e;

            var myQuery2 = Context.Employees
                .OrderByDescending(e => e.EmployeeID)
                .OrderBy(e => e.City);

            string psqlCommand = 
                "SELECT * FROM Employees " +
                "ORDER BY \"City\", \"EmployeeID\" DESC;";

            // Act
            var expected = Connection.Query<Employee>(psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void select_with_additional_from_as_cross_join()
        {
            // Arrange
            var myQuery =
                from o in Context.Orders
                from e in Context.Employees
                where o.EmployeeID == e.EmployeeID
                select new
                {
                    Employee = o.EmployeeID,
                    Order = o.OrderID
                };

            var myQuery2 = Context.Orders
                .SelectMany(o => 
                    Context.Employees,
                    (o, e) => new { o, e })
                .Where(x => x.o.EmployeeID == x.e.EmployeeID)
                .Select(x => new
                {
                    Employee = x.o.EmployeeID,
                    Order = x.o.OrderID
                });

            string psqlCommand = 
                "SELECT Orders.\"EmployeeID\", Orders.\"OrderID\" " + 
                "FROM Orders, Employees WHERE Orders.\"EmployeeID\" = Employees.\"EmployeeID\";";
            var queryMethod = typeof(ExtensionMethods)
                .GetMethod("QueryAnonymous", new[] { typeof(DbConnection), typeof(string) })
                .MakeGenericMethod(myQuery.ElementType);

            // Act
            var expected = queryMethod.Invoke(null, new object[] { Connection, psqlCommand });
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void select_with_inner_join()
        {
            // Arrange
            var myQuery = 
                from c in Context.Customers
                join o in Context.Orders
                on c.CustomerID equals o.CustomerID
                select new
                {
                    Name = c.ContactName,
                    Order = o.OrderID
                };
            
            var myQuery2 = Context.Customers
                .Join(Context.Orders,
                    c => c.CustomerID,
                    o => o.CustomerID,
                    (c, o) => new 
                    {
                        Name = c.ContactName,
                        Order = o.OrderID
                    });

            string psqlCommand = 
                "SELECT \"ContactName\", \"OrderID\" " + 
                "FROM Customers t1 INNER JOIN Orders t2 ON " +
                "t1.\"CustomerID\" = t2.\"CustomerID\";";
            var queryMethod = typeof(ExtensionMethods)
                .GetMethod("QueryAnonymous", new[] { typeof(DbConnection), typeof(string) })
                .MakeGenericMethod(myQuery.ElementType);

            // Act
            var expected = queryMethod.Invoke(null, new object[] { Connection, psqlCommand });
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void select_with_group_join()
        {
            // Arrange
            var myQuery =
                from c in Context.Customers
                join o in Context.Orders 
                on c.CustomerID equals o.CustomerID into orders
                select new
                {
                    Customer = c.CustomerID,
                    Orders = orders
                };

            var myQuery2 = Context.Customers
                .GroupJoin(Context.Orders,
                    c => c.CustomerID,
                    o => o.CustomerID,
                    (c, result) => new 
                    { 
                        Customer = c.CustomerID, 
                        Orders = result 
                    });

            string psqlCommand = // Required by the QueryAnonymous method to map properly.
                "SELECT \"customers\".\"CustomerID\" AS \"CustomerID\", " + 
                "\"orders\".\"OrderID\" AS \"Order.OrderID\", " +
                "\"orders\".\"CustomerID\" AS \"Order.CustomerID\", " +
                "\"orders\".\"EmployeeID\" AS \"Order.EmployeeID\", " + 
                "\"orders\".\"OrderDate\" AS \"Order.OrderDate\", " +
                "\"orders\".\"RequiredDate\" AS \"Order.RequiredDate\", " +
                "\"orders\".\"ShippedDate\" AS \"Order.ShippedDate\", " + 
                "\"orders\".\"ShipVia\" AS \"Order.ShipVia\", " +
                "\"orders\".\"Freight\" AS \"Order.Freight\", " +
                "\"orders\".\"ShipName\" AS \"Order.ShipName\", " +
                "\"orders\".\"ShipAddress\" AS \"Order.ShipAddress\", " +
                "\"orders\".\"ShipCity\" AS \"Order.ShipCity\", " + 
                "\"orders\".\"ShipRegion\" AS \"Order.ShipRegion\", " + 
                "\"orders\".\"ShipPostalCode\" AS \"Order.ShipPostalCode\", " +
                "\"orders\".\"ShipCountry\" AS \"Order.ShipCountry\", " +
                "(SELECT COUNT(*) from orders AS temp1_orders WHERE temp1_orders.\"CustomerID\" = customers.\"CustomerID\") AS \"Order.__GROUP_COUNT\" " +
                "FROM customers LEFT OUTER JOIN orders ON customers.\"CustomerID\" = orders.\"CustomerID\" " +
                "ORDER BY customers.\"CustomerID\", orders.\"CustomerID\";";
            var queryMethod = typeof(ExtensionMethods)
                .GetMethod("QueryAnonymous", new[] { typeof(DbConnection), typeof(string) })
                .MakeGenericMethod(myQuery.ElementType);

            // Act
            var expected = queryMethod.Invoke(null, new object[] { Connection, psqlCommand });
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void select_with_outer_join()
        {
            // Arrange
            var myQuery = 
                from c in Context.Customers
                join o in Context.Orders
                on c.CustomerID equals o.CustomerID into joined
                from j in joined.DefaultIfEmpty()
                select new
                {
                    CustomerID = c.CustomerID,
                    OrderID = j.OrderID
                };

            var myQuery2 = Context.Customers
                .GroupJoin(Context.Orders,
                    c => c.CustomerID,
                    o => o.CustomerID,
                    (c, os) => new 
                    { 
                        CustomerID = c.CustomerID,
                        Orders = os
                    })
                .SelectMany(j =>
                    j.Orders.DefaultIfEmpty(),
                    (j, o) => new
                    {
                        CustomerID = j.CustomerID,
                        OrderID = o.OrderID
                    });
            
            var psqlCommand = 
                "SELECT customers.\"CustomerID\", \"OrderID\" " +
                "FROM customers LEFT JOIN orders " +
                "ON customers.\"CustomerID\" = orders.\"CustomerID\";";
            var queryMethod = typeof(ExtensionMethods)
                .GetMethod("QueryAnonymous", new[] { typeof(DbConnection), typeof(string) })
                .MakeGenericMethod(myQuery.ElementType);

            // Act
            var expected = queryMethod.Invoke(null, new object[] { Connection, psqlCommand });
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }
    }
}