using System.Data.Common;

namespace Thesis.Relinq
{
    /// Represents a factory of PsqlQueryable objects.
    public static class PsqlQueryFactory
    {
        /// Creates a new PsqlQueryable instance.
        public static PsqlQueryable<T> Queryable<T>(DbConnection connection)
        {
            return new PsqlQueryable<T>(connection);
        }
    }
}