using System.Linq;
using System.Linq.Expressions;
using Npgsql;
using Remotion.Linq;
using Remotion.Linq.Parsing.Structure;

namespace Thesis.Relinq
{
    public class PsqlQueryable<T> : QueryableBase<T>
    {
        public PsqlQueryable(NpgsqlConnection connection)
            : base(new DefaultQueryProvider(
                typeof(PsqlQueryable<>), 
                QueryParser.CreateDefault(), 
                new PsqlQueryExecutor(connection)
            )) { }

        public PsqlQueryable(IQueryProvider provider)
            : base(provider) { }
            
        public PsqlQueryable (IQueryProvider provider, Expression expression)
            : base(provider, expression) { }
    }
}