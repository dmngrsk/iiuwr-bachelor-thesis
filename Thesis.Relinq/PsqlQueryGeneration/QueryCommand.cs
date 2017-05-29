using System.Collections.Generic;

namespace Thesis.Relinq.PsqlQueryGeneration
{
    /// An anemic class that holds the generated query statement and its parameters that are being passed to the DbConnection.
    public class QueryCommand
    {
        public string Statement { get; private set; }
        public Dictionary<string, object> Parameters { get; private set; }
        
        public QueryCommand(string statement, Dictionary<string, object> parameters)
        {
            Statement = statement;
            Parameters = parameters;
        }
    }
}