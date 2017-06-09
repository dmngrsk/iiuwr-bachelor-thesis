using System;
using Npgsql;

namespace Thesis.Relinq.Tests.Helpers
{
    public abstract class TestClassBase : IDisposable
    {
        protected readonly NpgsqlConnection Connection;

        protected TestClassBase()
        {
            var adapter = new NpgsqlConnectionAdapter
            {
                Server = "localhost",
                Port = 5432,
                Username = "dmngrsk",
                Password = "qwerty",
                Database = "northwind"
            };
            
            Connection = adapter.GetConnection();
        }

        public void Dispose()
        {
            if (Connection != null)
            {
                Connection.Dispose();
            }
        }
    }
}