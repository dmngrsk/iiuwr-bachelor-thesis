using Npgsql;
using NUnit.Framework;
using System.Linq;
using Thesis.Relinq.UnitTests.Models;

namespace Thesis.Relinq.UnitTests
{
    [TestFixture]
    public class ResultOperatorTests : ThesisTestsBase
    {
        [Test]
        public void select_count()
        {
            // Arrange
            var myQuery = 
                from c in PsqlQueryFactory.Queryable<Customers>(connection)
                select c;

            var myQuery2 = PsqlQueryFactory.Queryable<Customers>(connection)
                .Select(c => c);
            
            string psqlCommand = "SELECT COUNT(*) FROM Customers;";

            // Act
            var expected = NpgsqlRowConverter<int>.ReadScalar(connection, psqlCommand);
            var actual = myQuery.Count();
            var actual2 = myQuery2.Count();

            // Assert
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expected, actual2);
        }

        [Test]
        public void select_average()
        {
            // Arrange
            var myQuery = 
                from e in PsqlQueryFactory.Queryable<Employees>(connection)
                select new decimal(e.EmployeeID);

            var myQuery2 = PsqlQueryFactory.Queryable<Employees>(connection)
                .Select(e => new decimal(e.EmployeeID));
            
            string psqlCommand = "SELECT AVG(\"EmployeeID\") FROM Employees;";

            // Act
            var expected = NpgsqlRowConverter<decimal>.ReadScalar(connection, psqlCommand);
            var actual = myQuery.Average();
            var actual2 = myQuery2.Average();

            // Assert
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expected, actual2);
        }
    }
}