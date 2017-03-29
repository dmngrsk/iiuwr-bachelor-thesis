using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;
using Remotion.Linq;
using Thesis.Relinq.PsqlQueryGeneration;

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
            
            var commandData = PsqlGeneratingQueryModelVisitor.GeneratePsqlQuery(queryModel);
            var query = commandData.CreateQuery(_connection);

            return NpgsqlRowConverter<T>.ReadAllRows(_connection, query);
        }

        public T ExecuteScalar<T>(QueryModel queryModel)
        {
            List<T> rows = new List<T>();
            
            var commandData = PsqlGeneratingQueryModelVisitor.GeneratePsqlQuery(queryModel);
            var query = commandData.CreateQuery(_connection);

            return NpgsqlRowConverter<T>.ReadScalar(_connection, query);
        }

        public T ExecuteSingle<T>(QueryModel queryModel, bool returnDefaultWhenEmpty)
        {
            return returnDefaultWhenEmpty ?
                ExecuteCollection<T>(queryModel).SingleOrDefault() : 
                ExecuteCollection<T>(queryModel).Single();
        }
    }
}