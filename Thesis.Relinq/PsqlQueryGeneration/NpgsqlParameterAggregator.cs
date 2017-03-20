using System.Collections.Generic;

namespace Thesis.Relinq.PsqlQueryGeneration
{
    public class NpgsqlParameterAggregator
    {
        private readonly List<NpgsqlParameter> _parameters = 
            new List<NpgsqlParameter>();

        public string AddParameter(object parameterValue)
        {
            var parameter = new NpgsqlParameter($"p{_parameters.Count + 1}", parameterValue);
            _parameters.Add(parameter);

            return parameter.Name;
        }

        public NpgsqlParameter[] GetParameters()
        {
            return _parameters.ToArray();
        }
    }
}