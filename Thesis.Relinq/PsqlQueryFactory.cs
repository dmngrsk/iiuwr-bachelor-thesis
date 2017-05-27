using System.Data.Common;

namespace Thesis.Relinq
{
    public class PsqlQueryFactory
    {
        public static PsqlQueryable<T> Queryable<T>(DbConnection connection)
        {
            return new PsqlQueryable<T>(connection);
        }
    }
}