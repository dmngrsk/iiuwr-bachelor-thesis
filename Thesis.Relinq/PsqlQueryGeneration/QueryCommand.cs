using System.Collections.Generic;

namespace Thesis.Relinq.PsqlQueryGeneration
{
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