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
            connection.Dispose();
        }

        [Test]
        public void simple_select_test()
        {
            var foo = 
                from c in PsqlQueryFactory.Queryable<Customers>(connection)
                select c;
            
            var res = foo.ToList().Count();
            Assert.That(res, Is.EqualTo(0));
        }
    }
}