using Npgsql;
using NUnit.Framework;
using System.Linq;
using System.Reflection;
using Thesis.Relinq.UnitTests.Models;

namespace Thesis.Relinq.UnitTests
{
    [TestFixture]
    public class IntegrationTests : ThesisTestsBase
    {
        [Test]
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
            var expected = NpgsqlRowConverter<Customers>.ReadAllRows(connection, psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtension.AreEqualByJson(expected, actual);
            AssertExtension.AreEqualByJson(expected, actual2);
        }

        [Test]
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
            var rowConverterType = typeof(NpgsqlRowConverter<>).MakeGenericType(myQuery.ElementType);
            var rowConverterMethod = rowConverterType.GetMethod(
                "ReadAllRows", new [] { typeof(NpgsqlConnection), typeof(string) });

            // Act
            var expected = rowConverterMethod.Invoke(this, new object[] { connection, psqlCommand });
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();
            
            // Assert
            AssertExtension.AreEqualByJson(expected, actual);
            AssertExtension.AreEqualByJson(expected, actual2);
        }

        [Test]
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
            var expected = NpgsqlRowConverter<Customers>.ReadAllRows(connection, psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();
            
            // Assert
            AssertExtension.AreEqualByJson(expected, actual);
            AssertExtension.AreEqualByJson(expected, actual2);
        }

        [Test]
        public void select_with_multiple_wheres()
        {
            // Arrange
            var myQuery = 
                from e in PsqlQueryFactory.Queryable<Employees>(connection)
                where e.EmployeeID > 5 && e.City == "London" 
                select e;

            var myQuery2 = PsqlQueryFactory.Queryable<Employees>(connection)
                .Where(e => e.EmployeeID > 5 && e.City == "London");

            var myQuery3 = 
                from e in PsqlQueryFactory.Queryable<Employees>(connection)
                where e.EmployeeID > 5
                where e.City == "London" 
                select e;

            var myQuery4 = PsqlQueryFactory.Queryable<Employees>(connection)
                .Where(e => e.EmployeeID > 5).Where(e => e.City == "London");
            
            string psqlCommand = "SELECT * FROM Employees " +
                "WHERE \"EmployeeID\" > 5 AND \"City\" = 'London';";

            // Act
            var expected = NpgsqlRowConverter<Employees>.ReadAllRows(connection, psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();
            var actual3 = myQuery3.ToArray();
            var actual4 = myQuery4.ToArray();

            // Assert
            AssertExtension.AreEqualByJson(expected, actual);
            AssertExtension.AreEqualByJson(expected, actual2);
            AssertExtension.AreEqualByJson(expected, actual3);
            AssertExtension.AreEqualByJson(expected, actual4);
        }

        [Test]
        public void select_empty_result_with_always_false_where()
        {
            // Arrange
            var myQuery = 
                from e in PsqlQueryFactory.Queryable<Employees>(connection)
                where 3 > 5
                select e;

            var myQuery2 = PsqlQueryFactory.Queryable<Employees>(connection)
                .Where(x => 3 > 5);

            string psqlCommand = "SELECT * FROM Employees " +
                "WHERE 3 > 5;";

            // Act
            var expected = NpgsqlRowConverter<Employees>.ReadAllRows(connection, psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtension.AreEqualByJson(expected, actual);
            AssertExtension.AreEqualByJson(expected, actual2);
        }

        [Test]
        public void select_with_multiple_orderings()
        {
            // Arrange
            var myQuery = 
                from c in PsqlQueryFactory.Queryable<Customers>(connection)
                orderby c.ContactName descending, c.City
                orderby c.Country ascending
                select c;

            var myQuery2 = PsqlQueryFactory.Queryable<Customers>(connection)
                .OrderByDescending(c => c.ContactName)
                .ThenBy(c => c.City)
                .OrderBy(c => c.Country);

            string psqlCommand = "SELECT * FROM Customers " +
                "ORDER BY \"Country\", \"ContactName\" DESC, \"City\";";

            // Act
            var expected = NpgsqlRowConverter<Customers>.ReadAllRows(connection, psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtension.AreEqualByJson(expected, actual);
            AssertExtension.AreEqualByJson(expected, actual2);
        }

        [Test]
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

            string psqlCommand = "SELECT \"ContactName\", \"OrderID\" " + 
                "FROM Customers t1 INNER JOIN Orders t2 ON " +
                "t1.\"CustomerID\" = t2.\"CustomerID\";";
            var rowConverterType = typeof(NpgsqlRowConverter<>).MakeGenericType(myQuery.ElementType);
            var rowConverterMethod = rowConverterType.GetMethod(
                "ReadAllRows", new [] { typeof(NpgsqlConnection), typeof(string) });

            // Act
            var expected = rowConverterMethod.Invoke(this, new object[] { connection, psqlCommand });
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtension.AreEqualByJson(expected, actual);
            AssertExtension.AreEqualByJson(expected, actual2);
        }

        [Test]
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

         /* var myQuery2 = PsqlQueryFactory.Queryable<Orders>(connection)
                .SelectMany(o => PsqlQueryFactory.Queryable<Employees>(connection)
                    .Where(e => o.EmployeeID == e.EmployeeID)
                    .Select(e => new
                                 {
                                     Employee = o.EmployeeID,
                                     Order = o.OrderID
                                 })); */

            string psqlCommand = "SELECT ORDERS.\"EmployeeID\", ORDERS.\"OrderID\" " + 
                "FROM Orders, Employees WHERE Orders.\"EmployeeID\" = Employees.\"EmployeeID\";";
            var rowConverterType = typeof(NpgsqlRowConverter<>).MakeGenericType(myQuery.ElementType);
            var rowConverterMethod = rowConverterType.GetMethod(
                "ReadAllRows", new [] { typeof(NpgsqlConnection), typeof(string) });

            // Act
            var expected = rowConverterMethod.Invoke(this, new object[] { connection, psqlCommand });
            var actual = myQuery.ToArray();
         // var actual2 = myQuery2.ToArray();

            // Assert
            AssertExtension.AreEqualByJson(expected, actual);
         // AssertExtension.AreEqualByJson(expected, actual2);
        }
    }
}