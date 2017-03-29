using Npgsql;
using NUnit.Framework;

namespace Thesis.Relinq.UnitTests
{
    [TestFixture]
    public class ThesisTestsBase
    {
        protected NpgsqlConnection connection;

        [SetUp]
        public void Setup()
        {
            NpgsqlConnectionAdapter adapter = new NpgsqlConnectionAdapter
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
            {
                connection.Dispose();
            }
        }
    }
}