using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq;
using Remotion.Linq.Parsing.Structure;

namespace Thesis.Relinq
{
    public class PsqlQueryable<T> : QueryableBase<T>
    {
        private static IQueryProvider CreateQueryProvider(DbConnection connection)
        {
            return new DefaultQueryProvider(
                typeof(PsqlQueryable<>), 
                QueryParser.CreateDefault(), 
                new PsqlQueryExecutor(connection));
        }

        public PsqlQueryable(DbConnection connection)
            : base(CreateQueryProvider(connection)) { }

        public PsqlQueryable(IQueryProvider provider)
            : base(provider) { }
            
        public PsqlQueryable (IQueryProvider provider, Expression expression)
            : base(provider, expression) { }
    }
}