using System;
using Npgsql;

namespace Thesis.Relinq.Tests.Helpers
{
    public abstract class TestsBase : IDisposable
    {
        protected NpgsqlConnection connection;

        public TestsBase()
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

        public void Dispose()
        {
            if (connection != null)
            {
                connection.Dispose();
            }
        }
    }
}