using System.Linq;
using Dapper;
using Thesis.Relinq.Tests.Helpers;
using Thesis.Relinq.Tests.Models;
using Xunit;

namespace Thesis.Relinq.Tests
{
    public class AggregateOperatorTests : TestClassBase
    {
        [Fact]
        public void count()
        {
            // Arrange
            var myQuery = 
                from c in PsqlQueryFactory.Queryable<Customers>(Connection)
                select c;

            var myQuery2 = PsqlQueryFactory.Queryable<Customers>(Connection)
                .Select(c => c);
            
            string psqlCommand = "SELECT COUNT(*) FROM Customers;";

            // Act
            var expected = Connection.Query<int>(psqlCommand).Single();
            var actual = myQuery.Count();
            var actual2 = myQuery2.Count();

            // Assert
            Assert.Equal(expected, actual);
            Assert.Equal(expected, actual2);
        }

        [Fact]
        public void average()
        {
            // Arrange
            var myQuery = 
                from e in PsqlQueryFactory.Queryable<Employees>(Connection)
                select new decimal(e.EmployeeID);

            var myQuery2 = PsqlQueryFactory.Queryable<Employees>(Connection)
                .Select(e => new decimal(e.EmployeeID));
            
            string psqlCommand = "SELECT AVG(\"EmployeeID\") FROM Employees;";

            // Act
            var expected = Connection.Query<decimal>(psqlCommand).Single();
            var actual = myQuery.Average();
            var actual2 = myQuery2.Average();

            // Assert
            Assert.Equal(expected, actual);
            Assert.Equal(expected, actual2);
        }

        [Fact]
        public void sum()
        {
            // Arrange
            var myQuery = 
                from e in PsqlQueryFactory.Queryable<Employees>(Connection)
                select (int)e.EmployeeID;

            var myQuery2 = PsqlQueryFactory.Queryable<Employees>(Connection)
                .Select(e => (int)e.EmployeeID);
            
            string psqlCommand = "SELECT SUM(\"EmployeeID\") FROM Employees;";

            // Act
            var expected = Connection.Query<int>(psqlCommand).Single();
            var actual = myQuery.Sum();
            var actual2 = myQuery2.Sum();

            // Assert
            Assert.Equal(expected, actual);
            Assert.Equal(expected, actual2);
        }
        
        [Fact]
        public void min()
        {
            // Arrange
            var myQuery = 
                from e in PsqlQueryFactory.Queryable<Employees>(Connection)
                select (int)e.EmployeeID;

            var myQuery2 = PsqlQueryFactory.Queryable<Employees>(Connection)
                .Select(e => (int)e.EmployeeID);
            
            string psqlCommand = "SELECT MIN(\"EmployeeID\") FROM Employees;";

            // Act
            var expected = Connection.Query<int>(psqlCommand).Single();
            var actual = myQuery.Min();
            var actual2 = myQuery2.Min();

            // Assert
            Assert.Equal(expected, actual);
            Assert.Equal(expected, actual2);
        }

        [Fact]
        public void max()
        {
            // Arrange
            var myQuery = 
                from e in PsqlQueryFactory.Queryable<Employees>(Connection)
                select (int)e.EmployeeID;

            var myQuery2 = PsqlQueryFactory.Queryable<Employees>(Connection)
                .Select(e => (int)e.EmployeeID);
            
            string psqlCommand = "SELECT MAX(\"EmployeeID\") FROM Employees;";

            // Act
            var expected = Connection.Query<int>(psqlCommand).Single();
            var actual = myQuery.Max();
            var actual2 = myQuery2.Max();

            // Assert
            Assert.Equal(expected, actual);
            Assert.Equal(expected, actual2);
        }

        [Fact]
        public void distinct()
        {
            // Arrange
            var myQuery =
                (from c in PsqlQueryFactory.Queryable<Customers>(Connection)
                select c.City).Distinct();

            var myQuery2 = PsqlQueryFactory.Queryable<Customers>(Connection)
                .Select(c => c.City)
                .Distinct();

            var psqlCommand = "SELECT DISTINCT(\"City\") FROM Customers;";

            // Act
            var expected = Connection.Query<string>(psqlCommand).ToArray();
            var actual = myQuery.ToArray();
            var actual2 = myQuery2.ToArray();

            // Assert
            Assert.Equal(expected, actual);
            Assert.Equal(expected, actual2);
        }
    }
}