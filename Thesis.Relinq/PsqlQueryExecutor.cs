using Dapper;
using Remotion.Linq;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Thesis.Relinq.PsqlQueryGeneration;

namespace Thesis.Relinq
{
    public class PsqlQueryExecutor : IQueryExecutor
    {
        private readonly DbConnection _connection;

        public PsqlQueryExecutor (DbConnection connection)
        {
            _connection = connection;
        }

        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
        {
            _connection.Open();

            var dbSchema = new DbSchema(_connection);
            var psqlCommand = PsqlGeneratingQueryModelVisitor.GeneratePsqlQuery(queryModel, dbSchema);

            var result = typeof(T).IsAnonymous() ?
                _connection.QueryAnonymous<T>(psqlCommand.Statement, psqlCommand.Parameters) :
                _connection.Query<T>(psqlCommand.Statement, psqlCommand.Parameters);

            _connection.Close();
            return result;
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