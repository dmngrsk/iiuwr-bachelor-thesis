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
            List<T> rows = new List<T>();
            
            var commandData = PsqlQueryGenerator.GeneratePsqlQuery(queryModel);
            
            var query = commandData.CreateQuery(_connection);
            query.CommandText = (commandData.Statement);

            query.Connection.Open();

            using (var reader = query.ExecuteReader())
            {
                var columnSchema = reader.GetColumnSchema();
                var objectConverter = new NpgsqlRowConverter<T>(columnSchema);

                while (reader.Read())
                {
                    var row = new object[reader.FieldCount];
                    reader.GetValues(row);
                    
                    var obj = objectConverter.ConvertArrayToObject(row);
                    rows.Add(obj);
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