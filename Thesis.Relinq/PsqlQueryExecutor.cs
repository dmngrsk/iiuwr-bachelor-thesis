using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;
using Remotion.Linq;

namespace Thesis.Relinq
{
    public class PsqlQueryExecutor : IQueryExecutor
    {
        private readonly NpgsqlConnection _connection;

        public PsqlQueryExecutor (NpgsqlConnection connection)
        {
            _connection = connection;
        }

        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
        {
            var commandData = PsqlQueryGenerator.GeneratePsqlQuery(queryModel);
            var query = commandData.CreateQuery(_connection);
            
            query.Connection.Open();
            List<T> rows = new List<T>();

            using (var reader = query.ExecuteReader())
            {
                while (reader.Read())
                {
                    var columnSchema = reader.GetColumnSchema();

                    var row = new object[reader.FieldCount];
                }
            }

            query.Connection.Close();
            query.Dispose();
            return rows;
        }

        public T ExecuteScalar<T>(QueryModel queryModel)
        {
            return ExecuteCollection<T>(queryModel).Single();
        }

        public T ExecuteSingle<T>(QueryModel queryModel, bool returnDefaultWhenEmpty)
        {
            return returnDefaultWhenEmpty ?
                ExecuteCollection<T>(queryModel).SingleOrDefault() : 
                ExecuteCollection<T>(queryModel).Single();
        }
    }
}