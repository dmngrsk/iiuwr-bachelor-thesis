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

            var result = TypeIsAnonymous(typeof(T)) ?
                _connection.Query(commandData.Statement, commandData.Parameters)
                    .Select(x => (IDictionary<string, object>)x)
                    .Select(d => d.Values.ToArray())
                    .Select(row => (T)Activator.CreateInstance(typeof(T), row)) :
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

        private bool TypeIsAnonymous(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.GetCustomAttribute<CompilerGeneratedAttribute>() != null
                && typeInfo.IsGenericType
                && typeInfo.Name.Contains("AnonymousType")
                && (typeInfo.Name.StartsWith("<>") || typeInfo.Name.StartsWith("VB$"))
                && typeInfo.Attributes.HasFlag(TypeAttributes.NotPublic);
        }
    }
}