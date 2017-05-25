using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Thesis.Relinq.Tests.Helpers;
using Thesis.Relinq.Tests.Models;
using Xunit;

namespace Thesis.Relinq.Tests
{
    public class IntegrationTests : TestsBase
    {
        [Fact]
        public void select_all()
        {
            // Arrange
            var myQuery = 
                from c in PsqlQueryFactory.Queryable<Customers>(connection)
                select c;

            var myQuery2 = PsqlQueryFactory.Queryable<Customers>(connection)
                .Select(c => c);
            
            string psqlCommand = "SELECT * FROM Customers;";

            // Act
            var expected = connection.Query<Customers>(psqlCommand).ToArray();
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
                from c in PsqlQueryFactory.Queryable<Customers>(connection)
                select new
                {
                    Name = c.ContactName,
                    City = c.City
                };

            var myQuery2 = PsqlQueryFactory.Queryable<Customers>(connection)
                .Select(c => new { Name = c.ContactName, City = c.City });
            
            string psqlCommand = "SELECT \"ContactName\", \"City\" FROM Customers;";
            var queryMethod = typeof(ExtensionMethods)
                .GetMethod("QueryAnonymous", new[] { typeof(DbConnection), typeof(string) })
                .MakeGenericMethod(myQuery.ElementType);

            // Act
            var expected = queryMethod.Invoke(null, new object[] { connection, psqlCommand });
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
                from c in PsqlQueryFactory.Queryable<Customers>(connection)
                where c.CustomerID == "PARIS" 
                select c;
                
            var myQuery2 = PsqlQueryFactory.Queryable<Customers>(connection)
                .Where(c => c.CustomerID == "PARIS");
            
            string psqlCommand = "SELECT * FROM Customers WHERE \"CustomerID\" = 'PARIS';";

            // Act
            var expected = connection.Query<Customers>(psqlCommand).ToArray();
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
                from e in PsqlQueryFactory.Queryable<Employees>(connection)
                where e.EmployeeID > 5 && e.City == "London" 
                select e;

            var myQuery2 = PsqlQueryFactory.Queryable<Employees>(connection)
                .Where(e => e.EmployeeID > 5 && e.City == "London");

            string psqlCommand = 
                "SELECT * FROM Employees " +
                "WHERE \"EmployeeID\" > 5 AND \"City\" = 'London';";

            // Act
            var expected = connection.Query<Employees>(psqlCommand).ToArray();
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
                from e in PsqlQueryFactory.Queryable<Employees>(connection)
                where e.EmployeeID > 5
                where e.City == "London" 
                select e;

            var myQuery2 = PsqlQueryFactory.Queryable<Employees>(connection)
                .Where(e => e.EmployeeID > 5)
                .Where(e => e.City == "London");
            
            string psqlCommand = 
                "SELECT * FROM Employees " +
                "WHERE \"EmployeeID\" > 5 AND \"City\" = 'London';";

            // Act
            var expected = connection.Query<Employees>(psqlCommand).ToArray();
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
            var rowCountBeforeQuery = connection.Database.Count();
            var myQuery = 
                from e in PsqlQueryFactory.Queryable<Employees>(connection)
                where e.City == "London'; DROP TABLE Employees;" 
                select e;

            // Act
            var expected = new Employees[0];
            var actual = myQuery.ToArray();
            var rowCountAfterQuery = connection.Database.Count();

            // Arrange
            Assert.True(rowCountBeforeQuery > 0);
            Assert.Equal(rowCountBeforeQuery, rowCountAfterQuery);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void handles_string_addition()
        {
            var myQuery = 
                from e in PsqlQueryFactory.Queryable<Employees>(connection)
                select e.FirstName + " " + e.LastName + " has ID: " + e.EmployeeID;

            var myQuery2 = PsqlQueryFactory.Queryable<Employees>(connection)
                .Select(e => e.FirstName + " " + e.LastName + " has ID: " + e.EmployeeID);

            var psqlCommand = 
                "SELECT \"FirstName\" || ' ' || \"LastName\" || " +
                "' has ID: ' || \"EmployeeID\" FROM employees;";

            // Act
            var expected = connection.Query<string>(psqlCommand).ToArray();
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
                from c in PsqlQueryFactory.Queryable<Employees>(connection)
                where !(c.EmployeeID < 7 && c.EmployeeID > 3)
                select c;

            var myQuery2 = PsqlQueryFactory.Queryable<Employees>(connection)
                .Where(c => !(c.EmployeeID < 7 && c.EmployeeID > 3));
            
            string psqlCommand = 
                "SELECT * FROM Employees " +
                "WHERE NOT (\"EmployeeID\" < 7 AND \"EmployeeID\" > 3);";

            // Act
            var expected = connection.Query<Employees>(psqlCommand).ToArray();
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
                from e in PsqlQueryFactory.Queryable<Employees>(connection)
                where 3 > 5
                select e;

            var myQuery2 = PsqlQueryFactory.Queryable<Employees>(connection)
                .Where(x => 3 > 5);

            string psqlCommand = "SELECT * FROM Employees WHERE 3 > 5;";

            // Act
            var expected = connection.Query<Employees>(psqlCommand).ToArray();
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
                from e in PsqlQueryFactory.Queryable<Employees>(connection)
                orderby e.City, e.EmployeeID descending
                select e;

            var myQuery2 = PsqlQueryFactory.Queryable<Employees>(connection)
                .OrderBy(e => e.City)
                .ThenByDescending(e => e.EmployeeID);

            string psqlCommand = 
                "SELECT * FROM Employees " +
                "ORDER BY \"City\", \"EmployeeID\" DESC;";

            // Act
            var expected = connection.Query<Employees>(psqlCommand).ToArray();
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
                from e in PsqlQueryFactory.Queryable<Employees>(connection)
                orderby e.EmployeeID descending
                orderby e.City
                select e;

            var myQuery2 = PsqlQueryFactory.Queryable<Employees>(connection)
                .OrderByDescending(e => e.EmployeeID)
                .OrderBy(e => e.City);

            string psqlCommand = 
                "SELECT * FROM Employees " +
                "ORDER BY \"City\", \"EmployeeID\" DESC;";

            // Act
            var expected = connection.Query<Employees>(psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void select_with_join()
        {
            // Arrange
            var myQuery = 
                from c in PsqlQueryFactory.Queryable<Customers>(connection)
                join o in PsqlQueryFactory.Queryable<Orders>(connection)
                on c.CustomerID equals o.CustomerID
                select new
                {
                    Name = c.ContactName,
                    Order = o.OrderID
                };
            
            var myQuery2 = PsqlQueryFactory.Queryable<Customers>(connection)
                .Join(PsqlQueryFactory.Queryable<Orders>(connection),
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
            var expected = queryMethod.Invoke(null, new object[] { connection, psqlCommand });
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void select_with_additional_from()
        {
            // Arrange
            var myQuery =
                from o in PsqlQueryFactory.Queryable<Orders>(connection)
                from e in PsqlQueryFactory.Queryable<Employees>(connection)
                where o.EmployeeID == e.EmployeeID
                select new
                {
                    Employee = o.EmployeeID,
                    Order = o.OrderID
                };

            var myQuery2 = PsqlQueryFactory.Queryable<Orders>(connection)
                .SelectMany(o => 
                    PsqlQueryFactory.Queryable<Employees>(connection),
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
            var expected = queryMethod.Invoke(null, new object[] { connection, psqlCommand });
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void select_with_grouping()
        {
            var myQuery = 
                from c in PsqlQueryFactory.Queryable<Customers>(connection)
                group c by c.City into groups
                select new 
                {
                    City = groups.Key,
                    Customers = groups.ToArray()
                };

            var actual = myQuery.ToArray();
        }

        [Fact]
        public void select_with_group_join()
        {
            // Arrange
            var myQuery =
                from c in PsqlQueryFactory.Queryable<Customers>(connection)
                join o in PsqlQueryFactory.Queryable<Orders>(connection) 
                on c.CustomerID equals o.CustomerID into orders
                select new
                {
                    Customer = c.CustomerID,
                    Orders = orders
                };

            var myQuery2 = PsqlQueryFactory.Queryable<Customers>(connection)
                .GroupJoin(PsqlQueryFactory.Queryable<Orders>(connection),
                    c => c.CustomerID,
                    o => o.CustomerID,
                    (c, result) => new { Customer = c.CustomerID, Orders = result });

            string psqlCommand = // Required by the QueryAnonymous method to map properly.
                "SELECT \"customers\".\"CustomerID\" AS \"CustomerID\", " + 
                "\"orders\".\"OrderID\" AS \"Orders.OrderID\", " +
                "\"orders\".\"CustomerID\" AS \"Orders.CustomerID\", " +
                "\"orders\".\"EmployeeID\" AS \"Orders.EmployeeID\", " + 
                "\"orders\".\"OrderDate\" AS \"Orders.OrderDate\", " +
                "\"orders\".\"RequiredDate\" AS \"Orders.RequiredDate\", " +
                "\"orders\".\"ShippedDate\" AS \"Orders.ShippedDate\", " + 
                "\"orders\".\"ShipVia\" AS \"Orders.ShipVia\", " +
                "\"orders\".\"Freight\" AS \"Orders.Freight\", " +
                "\"orders\".\"ShipName\" AS \"Orders.ShipName\", " +
                "\"orders\".\"ShipAddress\" AS \"Orders.ShipAddress\", " +
                "\"orders\".\"ShipCity\" AS \"Orders.ShipCity\", " + 
                "\"orders\".\"ShipRegion\" AS \"Orders.ShipRegion\", " + 
                "\"orders\".\"ShipPostalCode\" AS \"Orders.ShipPostalCode\", " +
                "\"orders\".\"ShipCountry\" AS \"Orders.ShipCountry\", " +
                "(SELECT COUNT(*) from orders AS temp1_orders WHERE temp1_orders.\"CustomerID\" = customers.\"CustomerID\") AS \"Orders.Count\" " +
                "FROM customers LEFT OUTER JOIN orders ON customers.\"CustomerID\" = orders.\"CustomerID\" " +
                "ORDER BY customers.\"CustomerID\", orders.\"CustomerID\";";
            var queryMethod = typeof(ExtensionMethods)
                .GetMethod("QueryAnonymous", new[] { typeof(DbConnection), typeof(string) })
                .MakeGenericMethod(myQuery.ElementType);

            // Act
            var expected = queryMethod.Invoke(null, new object[] { connection, psqlCommand });
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            // AssertExtension.EqualByJson(expected, actual);
            // AssertExtension.EqualByJson(expected, actual2);
        }

        [Fact]
        public void select_with_case()
        {
            // Arrange
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

            var myQuery2 = PsqlQueryFactory.Queryable<Employees>(connection)
                .Where(e => e.EmployeeID < 8)
                .Select(e => new
                             {
                                 EmployeeID = e.EmployeeID, 
                                 CaseResult = (e.EmployeeID < 5 ? "smaller than five" :
                                               e.EmployeeID == 5 ? "equal to five" :
                                               "larger than five")
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
            var expected = queryMethod.Invoke(null, new object[] { connection, psqlCommand });
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }

        [Fact]
        public void fetches_data_from_tables_with_different_name_casing()
        {
            // Arrange
            var newConnection = new NpgsqlConnectionAdapter()
            {
                Server = "localhost",
                Port = 5432,
                Username = "dmngrsk",
                Password = "qwerty",
                Database = "ii"
            }
            .GetConnection();

            var myQuery = 
                from g in PsqlQueryFactory.Queryable<Grupa>(newConnection)
                where g.RodzajZajec.Equals('w')
                select g;

            var myQuery2 = PsqlQueryFactory.Queryable<Grupa>(newConnection)
                .Where(g => g.RodzajZajec.Equals('w'));

            var psqlCommand = // Required by the QueryAnonymous method to map properly.
                "SELECT kod_grupy AS KodGrupy, " +
                "kod_przed_sem AS KodPrzedSem, " +
                "kod_uz AS KodUz, " +
                "max_osoby AS MaxOsoby, " +
                "rodzaj_zajec AS RodzajZajec, " +
                "termin AS Termin, " +
                "sala AS Sala " +
                "FROM grupa WHERE grupa.rodzaj_zajec = 'w';";

            // Act
            newConnection.Open();
            var expected = newConnection.Query<Grupa>(psqlCommand).ToArray();
            newConnection.Close();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();
            
            // Assert
            AssertExtensions.EqualByJson(expected, actual);
            AssertExtensions.EqualByJson(expected, actual2);
        }
    }
}