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
                select new { c.CustomerID };

            var myQuery2 = PsqlQueryFactory.Queryable<Customers>(connection)
                .Select(c => new { c.CustomerID });
            
            string psqlCommand = "SELECT COUNT(*) FROM Customers;";

            // Act
            var expected = NpgsqlRowConverter<int>.ReadScalar(connection, psqlCommand);
            var actual = myQuery.Count();
            var actual2 = myQuery2.Count();

            // Assert
            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expected, actual2);
        }
    }
}