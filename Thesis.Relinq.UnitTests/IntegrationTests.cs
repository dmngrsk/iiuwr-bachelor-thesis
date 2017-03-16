using Npgsql;
using NUnit.Framework;
using System.Linq;
using Thesis.Relinq.UnitTests.Models;

namespace Thesis.Relinq.UnitTests
{
    [TestFixture]
    public class IntegrationTests
    {
        private NpgsqlConnection connection;

        [SetUp]
        public void Setup()
        {
            PsqlConnectionAdapter adapter = new PsqlConnectionAdapter
            {
                Server = "localhost",
                Port = 5432,
                Username = "dmngrsk",
                Password = "qwerty",
                Database = "northwind"
            };
            
            connection = new NpgsqlConnection(adapter.ConnectionString);
        }

        [TearDown]
        public void TearDown()
        {
            if (connection != null)
                connection.Dispose();
        }

        [Test]
        public void simple_select_all_test()
        {
            // Arrange
            var myQuery = 
                from c in PsqlQueryFactory.Queryable<Customers>(connection)
                select c;
            
            string psqlCommand = "SELECT * FROM Customers;";

            // Act
            var expected = NpgsqlRowConverter<Customers>.ReadAllRows(connection, psqlCommand).ToArray();
            var actual = myQuery.ToArray();

            // Assert
            Assert.AreEqual(myQuery.ElementType, typeof(Customers));
            AssertExtension.AreEqualByJson(expected, actual);
        }

        [Test]
        public void select_columns_creating_an_anonymous_type_test()
        {
            // Arrange
            var myQuery = 
                from c in PsqlQueryFactory.Queryable<Customers>(connection)
                select new
                {
                    Name = c.ContactName,
                    City = c.City
                };
            
            string psqlCommand = "SELECT \"ContactName\", \"City\" FROM Customers;";

            // Act
            // var actual = myQuery.ToArray();
            // var expected = NpgsqlRowConverter</* ANONYMOUS TYPE GOES HERE */>.ReadAllRows(connection, psqlCommand).ToArray();

            // Assert
            // Assert.AreEqual(myQuery.ElementType, typeof(/* ANONYMOUS TYPE GOES HERE */));
            // AssertExtension.AreEqualByJson(expected, actual);
        }

        [Test]
        public void simple_where_test()
        {
            // Arrange
            var myQuery = 
                from c in PsqlQueryFactory.Queryable<Customers>(connection)
                where c.CustomerID == "PARIS" 
                select c;
            
            string psqlCommand = "SELECT * FROM Customers WHERE \"CustomerID\" = 'PARIS';";

            // Act
            var expected = NpgsqlRowConverter<Customers>.ReadAllRows(connection, psqlCommand).ToArray();
            var actual = myQuery.ToArray();

            // Assert
            Assert.AreEqual(myQuery.ElementType, typeof(Customers));
            AssertExtension.AreEqualByJson(expected, actual);
        }
    }
}