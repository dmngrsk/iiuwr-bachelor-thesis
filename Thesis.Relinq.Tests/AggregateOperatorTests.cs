using NUnit.Framework;
using System.Linq;
using Thesis.Relinq.NpgsqlWrapper;
using Thesis.Relinq.Tests.Helpers;
using Thesis.Relinq.Tests.Models;

namespace Thesis.Relinq.Tests
{
    [TestFixture]
    public class AggregateOperatorTests : ThesisTestsBase
    {
        [Test]
        public void count()
        {
            // Arrange
            var myQuery = 
                from c in PsqlQueryFactory.Queryable<Customers>(connection)
                select c;

            var myQuery2 = PsqlQueryFactory.Queryable<Customers>(connection)
                .Select(c => c);
            
            string psqlCommand = "SELECT COUNT(*) FROM Customers;";

            // Act
            var expected = NpgsqlRowConverter<int>.ReadAllRows(connection, psqlCommand).Single();
            var actual = myQuery.Count();
            var actual2 = myQuery2.Count();

            // Assert
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expected, actual2);
        }

        [Test]
        public void average()
        {
            // Arrange
            var myQuery = 
                from e in PsqlQueryFactory.Queryable<Employees>(connection)
                select new decimal(e.EmployeeID);

            var myQuery2 = PsqlQueryFactory.Queryable<Employees>(connection)
                .Select(e => new decimal(e.EmployeeID));
            
            string psqlCommand = "SELECT AVG(\"EmployeeID\") FROM Employees;";

            // Act
            var expected = NpgsqlRowConverter<decimal>.ReadAllRows(connection, psqlCommand).Single();
            var actual = myQuery.Average();
            var actual2 = myQuery2.Average();

            // Assert
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expected, actual2);
        }

        [Test]
        public void sum()
        {
            // Arrange
            var myQuery = 
                from e in PsqlQueryFactory.Queryable<Employees>(connection)
                select (int)e.EmployeeID;

            var myQuery2 = PsqlQueryFactory.Queryable<Employees>(connection)
                .Select(e => (int)e.EmployeeID);
            
            string psqlCommand = "SELECT SUM(\"EmployeeID\") FROM Employees;";

            // Act
            var expected = NpgsqlRowConverter<int>.ReadAllRows(connection, psqlCommand).Single();
            var actual = myQuery.Sum();
            var actual2 = myQuery2.Sum();

            // Assert
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expected, actual2);
        }
        
        [Test]
        public void min()
        {
            // Arrange
            var myQuery = 
                from e in PsqlQueryFactory.Queryable<Employees>(connection)
                select (int)e.EmployeeID;

            var myQuery2 = PsqlQueryFactory.Queryable<Employees>(connection)
                .Select(e => (int)e.EmployeeID);
            
            string psqlCommand = "SELECT MIN(\"EmployeeID\") FROM Employees;";

            // Act
            var expected = NpgsqlRowConverter<int>.ReadAllRows(connection, psqlCommand).Single();
            var actual = myQuery.Min();
            var actual2 = myQuery2.Min();

            // Assert
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expected, actual2);
        }

        [Test]
        public void max()
        {
            // Arrange
            var myQuery = 
                from e in PsqlQueryFactory.Queryable<Employees>(connection)
                select (int)e.EmployeeID;

            var myQuery2 = PsqlQueryFactory.Queryable<Employees>(connection)
                .Select(e => (int)e.EmployeeID);
            
            string psqlCommand = "SELECT MAX(\"EmployeeID\") FROM Employees;";

            // Act
            var expected = NpgsqlRowConverter<int>.ReadAllRows(connection, psqlCommand).Single();
            var actual = myQuery.Max();
            var actual2 = myQuery2.Max();

            // Assert
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expected, actual2);
        }

        [Test]
        public void distinct()
        {
            // Arrange
            var myQuery =
                (from c in PsqlQueryFactory.Queryable<Customers>(connection)
                select c.City).Distinct();

            var myQuery2 = PsqlQueryFactory.Queryable<Customers>(connection)
                .Select(c => c.City)
                .Distinct();

            var psqlCommand = "SELECT DISTINCT(\"City\") FROM Customers;";

            // Act
            var expected = NpgsqlRowConverter<string>.ReadAllRows(connection, psqlCommand);
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expected, actual2);
        }
    }
}