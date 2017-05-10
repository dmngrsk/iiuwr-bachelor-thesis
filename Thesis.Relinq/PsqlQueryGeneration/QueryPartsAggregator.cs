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

            SubQueries = new List<string>();
            SubQueryLinkActions = new List<string>();
            _subQueryOpen = false;
        }

        private string SelectPart { get; set; }
        private List<string> FromParts { get; set; }
        private List<string> WhereParts { get; set; }
        private List<string> OrderByParts { get; set; }
        private List<string> PagingParts { get; set; }

        private List<string> SubQueries { get; set; }
        private List<string> SubQueryLinkActions { get; set; }

        private bool _subQueryOpen;
        private QueryPartsAggregator _subQuery;

        public void SetSelectPart(string selectPart)
        {
            if (_subQueryOpen)
            {
                _subQuery.SetSelectPart(selectPart);
            }
            else
            {
                SelectPart = selectPart.Contains(".") ? selectPart : "*";
            }
        }

        public void SetSelectPartAsScalar(string scalarPartFormat)
        {
            if (_subQueryOpen)
            {
                _subQuery.SetSelectPartAsScalar(scalarPartFormat);
            }
            else
            {
                SelectPart = string.Format(scalarPartFormat, SelectPart);
            }
        }

        public void AddFromPart(string fromPart)
        {
            if (_subQueryOpen)
            {
                _subQuery.AddFromPart(fromPart);
            }
            else
            {
                FromParts.Add(fromPart);
            }
        }

        public void AddJoinPart(string leftMember, string rightMember)
        {
            if (_subQueryOpen)
            {
                _subQuery.AddJoinPart(leftMember, rightMember);
            }
            else
            {
                var leftSource = leftMember.Split('.')[0];
                var rightSource = rightMember.Split('.')[0];
                var joinPart = 
                    $"{leftSource} INNER JOIN {rightSource} ON ({leftMember} = {rightMember})";

                // We're using the fact that the left source table was already added in AddFromPart
                var index = FromParts.IndexOf(leftSource);
                FromParts[index] = joinPart;
            }
        }

        public void AddWherePart(string formatString, params object[] args)
        {
            if (_subQueryOpen)
            {
                _subQuery.AddWherePart(formatString, args);
            }
            else
            {
                if (formatString != string.Empty)
                {
                    WhereParts.Add(string.Format(formatString, args));
                }
            }
        }

        public void AddOrderByPart(IEnumerable< Tuple<string, OrderingDirection> > orderings)
        {
            if (_subQueryOpen)
            {
                _subQuery.AddOrderByPart(orderings);
            }
            else
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
        }

        public void AddPagingPart(string limitter, string count)
        {
            if (_subQueryOpen)
            {
                _subQuery.AddPagingPart(limitter, count);
            }
            else
            {
                PagingParts.Add($"{limitter} {count}");
            }
        }

        public void OpenSubQuery()
        {
            _subQueryOpen = true;
            _subQuery = new QueryPartsAggregator();
        }

        public void CloseSubQuery()
        {
            SubQueries.Add(_subQuery.BuildPsqlString().Trim(';'));
            _subQueryOpen = false;
        }

        public void AddSubQueryLinkAction(string queryLinkAction)
        {
            SubQueryLinkActions.Add(queryLinkAction);
            _subQueryOpen = false;
        }

        public string BuildPsqlString()
        {
            WrapSubQueriesToQueryParts();

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

            var psqlQuery = stringBuilder.ToString();

            for (int i = SubQueries.Count - 1; i >= 0; i--)
            {
                var subQuery = SubQueries[i];
                var subQueryLinkFormatter = SubQueryLinkActions[i];
                psqlQuery = string.Format(subQueryLinkFormatter, psqlQuery, subQuery);
            }

            return $"{psqlQuery};";
        }

        private void WrapSubQueriesToQueryParts()
        {
            if (SubQueries.Count != SubQueryLinkActions.Count)
            {
                throw new ArgumentException(
                    "Amount of subqueries and the actions to take with them is not equal.");
            }

            for (int i = SubQueries.Count - 1; i >= 0; i--)
            {
                var subQuery = SubQueries[i];
                var subQueryAction = SubQueryLinkActions[i];
                if (subQueryAction.Contains("EXISTS"))
                {
                    WhereParts.Add(string.Format(subQueryAction, subQuery));
                    SubQueries.RemoveAt(i);
                    SubQueryLinkActions.RemoveAt(i);
                }
            }
        }
    }
}