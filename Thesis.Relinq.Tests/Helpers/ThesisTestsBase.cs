using Npgsql;
using NUnit.Framework;
using Thesis.Relinq.NpgsqlWrapper;

namespace Thesis.Relinq.Tests.Helpers
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
            
            connection = adapter.GetConnection();
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