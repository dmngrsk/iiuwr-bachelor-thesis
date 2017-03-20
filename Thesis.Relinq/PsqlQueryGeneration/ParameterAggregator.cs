using System.Collections.Generic;

namespace Thesis.Relinq.PsqlQueryGeneration
{
    public class ParameterAggregator
    {
        private readonly List<NamedParameter> _parameters = new List<NamedParameter>();

        public string AddParameter(object parameterValue)
        {
            var parameter = new NamedParameter($"p{_parameters.Count + 1}", parameterValue);
            _parameters.Add(parameter);

            return parameter.Name;
        }

        public NamedParameter[] GetParameters()
        {
            return _parameters.ToArray();
        }
    }
}