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
            PagingParts = new List<string>();
        }

        public string SelectPart { get; private set; }
        private List<string> FromParts { get; set; }
        private List<string> WhereParts { get; set; }
        private List<string> OrderByParts { get; set; }
        private List<string> PagingParts { get; set; }

        public void SetSelectPart(string selectPart)
        {
            SelectPart = selectPart.Contains(".") ? selectPart : "*";
        }

        public void SetSelectPartAsScalar(string scalarPartFormat)
        {
            SelectPart = string.Format(scalarPartFormat, SelectPart);
        }

        public void AddFromPart(string fromPart)
        {
            FromParts.Add(fromPart);
        }

        public void AddJoinPart(string leftMember, string rightMember)
        {
            var leftSource = leftMember.Split('.')[0];
            var rightSource = rightMember.Split('.')[0];
            var joinPart = 
                $"{leftSource} INNER JOIN {rightSource} ON ({leftMember} = {rightMember})";

            // We're using the fact that the left source table was already added in AddFromPart
            var index = FromParts.IndexOf(leftSource);
            FromParts[index] = joinPart;
        }

        public void AddWherePart(string formatString, params object[] args)
        {
            WhereParts.Add(string.Format(formatString, args));
        }

        public void AddOrderByPart(IEnumerable< Tuple<string, OrderingDirection> > orderings)
        {
            List<string> localOrderByParts = new List<string>();

            foreach (var ordering in orderings)
            {
                string orderByPart = 
                    (ordering.Item2 == OrderingDirection.Desc) ?
                    ordering.Item1 + " DESC" :
                    ordering.Item1;

                localOrderByParts.Add(orderByPart);
            }

            localOrderByParts.AddRange(OrderByParts);
            OrderByParts = localOrderByParts;
        }

        public void AddPagingPart(string limitter, string count)
        {
            PagingParts.Add($"{limitter} {count}");
        }

        public string BuildPsqlString()
        {
            var stringBuilder = new StringBuilder();

            if (string.IsNullOrEmpty(SelectPart))
                throw new InvalidOperationException("A query must have a SELECT part.");

            if (FromParts.Count == 0)
                throw new InvalidOperationException("A query must have at least one FROM part.");

            // TODO: Check if table's name can contains a dot
            stringBuilder.AppendFormat("SELECT {0}", SelectPart);
            
            stringBuilder.AppendFormat(" FROM {0}", string.Join(", ", FromParts));

            if (WhereParts.Count > 0)
                stringBuilder.AppendFormat(" WHERE {0}", string.Join(" AND ", WhereParts));

            if (OrderByParts.Count > 0)
                stringBuilder.AppendFormat(" ORDER BY {0}", string.Join(", ", OrderByParts));

            if (PagingParts.Count > 0)
                stringBuilder.AppendFormat(" {0}", string.Join(" ", PagingParts));

            return stringBuilder.Append(";").ToString();
        }
    }
}