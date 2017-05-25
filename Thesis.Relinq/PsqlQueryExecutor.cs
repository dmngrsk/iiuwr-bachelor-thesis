using Dapper;
using Npgsql;
using Remotion.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Thesis.Relinq.PsqlQueryGeneration;
using System.Runtime.CompilerServices;
using System;
using System.Dynamic;
using System.Collections;
using System.Data.Common;

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
            _connection.Open();

            var dbSchema = new NpgsqlDatabaseSchema(_connection);
            var commandData = PsqlGeneratingQueryModelVisitor.GeneratePsqlQuery(queryModel, dbSchema);

            var result = typeof(T).IsAnonymous() ?
                _connection.QueryAnonymous<T>(commandData.Statement, commandData.Parameters) :
                _connection.Query<T>(commandData.Statement, commandData.Parameters);

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