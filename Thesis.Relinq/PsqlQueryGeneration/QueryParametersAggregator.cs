using System.Collections.Generic;

namespace Thesis.Relinq.PsqlQueryGeneration
{
    public class QueryParametersAggregator
    {
        public Dictionary<string, object> Parameters { get; private set; } 

        public QueryParametersAggregator()
        {
            this.Parameters = new Dictionary<string, object>();
        }

        public string AddParameter(object parameterValue)
        {
            var newParameterName = $"p{Parameters.Count + 1}";
            Parameters[newParameterName] = parameterValue;

            return newParameterName;
        }
    }
}