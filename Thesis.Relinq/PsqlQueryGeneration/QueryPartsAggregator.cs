using System;
using System.Collections.Generic;
using System.Text;
using Remotion.Linq.Clauses;

namespace Thesis.Relinq.PsqlQueryGeneration
{
    public class QueryPartsAggregator
    {
        public QueryPartsAggregator()
        {
            FromParts = new List<string>();
            WhereParts = new List<string>();
            OrderByParts = new List<string>();
        }

        public string SelectPart { get; private set; }
        private List<string> FromParts { get; set; }
        private List<string> WhereParts { get; set; }
        private List<string> OrderByParts { get; set; }

        public void SetSelectPart(string selectPart)
        {
            SelectPart = selectPart;
        }

        public void AddFromPart(IQuerySource querySource)
        {
            int index = querySource.ItemType.ToString().LastIndexOf('.') + 1;
            string fromPart = querySource.ItemType.ToString().Substring(index);
            FromParts.Add(fromPart);
        }

        public void AddWherePart(string formatString, params object[] args)
        {
            WhereParts.Add(string.Format(formatString, args));
        }

        public void AddOrderByPart(IEnumerable< Tuple<string, OrderingDirection> > orderings)
        {
            foreach (var ordering in orderings)
            {
                string orderByPart = 
                    (ordering.Item2 == OrderingDirection.Desc) ?
                    ordering.Item1 + " DESC" :
                    ordering.Item1;

                OrderByParts.Add(orderByPart);
            }
        }

        public string BuildPsqlString()
        {
            var stringBuilder = new StringBuilder();

            if (string.IsNullOrEmpty(SelectPart))
                throw new InvalidOperationException("A query must have a SELECT part.");

            if (FromParts.Count == 0)
                throw new InvalidOperationException("A query must have at least one FROM part.");

            stringBuilder.AppendFormat("SELECT {0}", SelectPart);
            
            stringBuilder.AppendFormat(" FROM {0}", string.Join(", ", FromParts));

            if (WhereParts.Count > 0)
                stringBuilder.AppendFormat(" WHERE {0}", string.Join(" AND ", WhereParts));

            if (OrderByParts.Count > 0)
                stringBuilder.AppendFormat(" ORDER BY {0}", string.Join(", ", OrderByParts));

            return stringBuilder.Append(";").ToString();
        }
    }
}