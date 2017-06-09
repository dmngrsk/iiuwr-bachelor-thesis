using System.Collections.Generic;

namespace Thesis.Relinq.PsqlQueryGeneration
{
    /// Aggregates parameters of a query while assigning them unique names to use in a raw statement.
    public class QueryParametersAggregator
    {
        public Dictionary<string, object> Parameters { get; } 

        public QueryParametersAggregator()
        {
            this.Parameters = new Dictionary<string, object>();
        }

        /// Adds an object that is a query parameter and returns the name that got assigned to it.
        public string AddParameter(object parameterValue)
        {
            var newParameterName = $"p{Parameters.Count + 1}";
            Parameters[newParameterName] = parameterValue;

            return newParameterName;
        }
    }
}