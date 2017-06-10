using System;
using Npgsql;

namespace Thesis.Relinq.Tests.Helpers
{
    public abstract class TestClassBase : IDisposable
    {
        protected readonly NpgsqlConnection Connection;
        protected readonly NorthwindContext Context;

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
            
            this.Connection = adapter.GetConnection();
            this.Context = new NorthwindContext(Connection);
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