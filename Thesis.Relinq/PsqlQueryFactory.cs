using Npgsql;

namespace Thesis.Relinq
{
    public class PsqlQueryFactory
    {
        public static PsqlQueryable<T> Queryable<T>(NpgsqlConnection connection)
        {
            return new PsqlQueryable<T>(connection);
        }
    }
}