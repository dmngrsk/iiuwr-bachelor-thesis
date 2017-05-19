using System.Collections.Generic;
using Npgsql;

namespace Thesis.Relinq.PsqlQueryGeneration
{
    public class NpgsqlCommandData
    {
        public NpgsqlCommandData(string statement, Dictionary<string, object> parameters)
        {
            Statement = statement;
            Parameters = parameters;
        }

        public string Statement { get; private set; }
        public Dictionary<string, object> Parameters { get; private set; }

        public NpgsqlCommand CreateQuery(NpgsqlConnection connection)
        {
            var query = new NpgsqlCommand();
            query.Connection = connection;
            
            query.CommandText = Statement;

            foreach (var parameter in Parameters)
            {
                query.Parameters.AddWithValue(parameter.Key, parameter.Value);
            }

            return query;
        }
    }
}