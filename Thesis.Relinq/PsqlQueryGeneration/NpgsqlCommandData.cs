using Npgsql;

namespace Thesis.Relinq.PsqlQueryGeneration
{
    public class NpgsqlCommandData
    {
        public NpgsqlCommandData(string statement, NpgsqlParameter[] parameters)
        {
            Statement = statement;
            Parameters = parameters;
        }

        public string Statement { get; private set; }
        public NpgsqlParameter[] Parameters { get; private set; }

        public NpgsqlCommand CreateQuery(NpgsqlConnection connection)
        {
            var query = new NpgsqlCommand();
            query.Connection = connection;
            
            query.CommandText = Statement;

            foreach (var parameter in Parameters)
                query.Parameters.AddWithValue(parameter.Name, parameter.Value);

            return query;
        }
    }
}