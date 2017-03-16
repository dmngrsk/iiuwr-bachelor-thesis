using Npgsql;

namespace Thesis.Relinq.PsqlQueryGeneration
{
    public class PsqlCommandData
    {
        public PsqlCommandData(string statement, object[] parameters)
        {
            Statement = statement;
            Parameters = parameters;
        }

        public string Statement { get; private set; }
        public object[] Parameters { get; private set; }

        public NpgsqlCommand CreateQuery(NpgsqlConnection connection)
        {
            var query = new NpgsqlCommand();
            query.Connection = connection;
            
            query.CommandText = Statement;

            foreach (var parameter in Parameters)
                query.Parameters.Add(parameter);

            return query;
        }
    }
}