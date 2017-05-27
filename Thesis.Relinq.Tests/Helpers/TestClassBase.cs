using System;
using Npgsql;

namespace Thesis.Relinq.Tests.Helpers
{
    public abstract class TestClassBase : IDisposable
    {
        protected NpgsqlConnection connection;

        public TestClassBase()
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