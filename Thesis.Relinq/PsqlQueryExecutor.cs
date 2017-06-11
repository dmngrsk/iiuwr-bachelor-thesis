using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Dapper;
using Remotion.Linq;
using Thesis.Relinq.PsqlQueryGeneration;

namespace Thesis.Relinq
{
    /// Represents a bridge between re-linq and this PostgreSQL LINQ provider. Used by re-linq to create an IQueryProvider that executes queries.
    public class PsqlQueryExecutor : IQueryExecutor
    {
        private readonly DbConnection _connection;

        public PsqlQueryExecutor(DbConnection connection)
        {
            _connection = connection;
        }

        /// Translates re-linq's QueryModel object to a PostgreSQL query, executes it and returns the result.
        ///
        /// Returns: A collection of items being the result of the query.
        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
        {
            _connection.Open();

            var psqlCommand = PsqlGeneratingQueryModelVisitor.GeneratePsqlQuery(queryModel);
            var result = typeof(T).IsAnonymous() 
                ? _connection.QueryAnonymous<T>(psqlCommand.Statement, psqlCommand.Parameters)
                : _connection.Query<T>(psqlCommand.Statement, psqlCommand.Parameters);

            _connection.Close();
            return result;
        }

        /// Translates re-linq's QueryModel object to a PostgreSQL query, executes it and returns the result.
        ///
        /// Returns: An item being the result of the query.
        public T ExecuteScalar<T>(QueryModel queryModel)
        {
            return ExecuteCollection<T>(queryModel).Single();
        }

        /// Translates re-linq's QueryModel object to a PostgreSQL query, executes it and returns the result.
        ///
        /// Returns: An item being the result of the query.
        public T ExecuteSingle<T>(QueryModel queryModel, bool returnDefaultWhenEmpty)
        {
            return returnDefaultWhenEmpty ?
                ExecuteCollection<T>(queryModel).SingleOrDefault() : 
                ExecuteCollection<T>(queryModel).Single();
        }
    }
}