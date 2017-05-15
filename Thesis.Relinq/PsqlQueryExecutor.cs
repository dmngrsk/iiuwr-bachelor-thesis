using Npgsql;
using Remotion.Linq;
using System.Collections.Generic;
using System.Linq;
using Thesis.Relinq.PsqlQueryGeneration;
using Thesis.Relinq.NpgsqlWrapper;

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
            var dbSchema = new NpgsqlDatabaseSchema(_connection);
            var commandData = PsqlGeneratingQueryModelVisitor.GeneratePsqlQuery(queryModel, dbSchema);
            var query = commandData.CreateQuery(_connection);

            return NpgsqlRowConverter<T>.ReadAllRows(_connection, query);
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