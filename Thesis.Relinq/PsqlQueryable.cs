using System.Linq;
using System.Linq.Expressions;
using Npgsql;
using Remotion.Linq;

namespace Thesis.Relinq
{
    public class PsqlQueryable<T> : QueryableBase<T>
    {
        private static IQueryExecutor CreateExecutor(NpgsqlConnection connection) =>
            new PsqlQueryExecutor(connection);

        public PsqlQueryable (NpgsqlConnection connection)
            : base(CreateExecutor(connection)) { }
            
        public PsqlQueryable (IQueryProvider provider, Expression expression)
            : base(provider, expression) { }
    }
}