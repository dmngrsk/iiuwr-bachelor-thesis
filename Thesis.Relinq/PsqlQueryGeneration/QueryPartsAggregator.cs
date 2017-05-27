using System;
using System.Collections.Generic;
using System.Text;
using Remotion.Linq.Clauses;

namespace Thesis.Relinq.PsqlQueryGeneration
{
    public class QueryPartsAggregator
    {
        private string SelectPart { get; set; }
        private List<string> FromParts { get; set; }
        private List<string> WhereParts { get; set; }
        private List<string> OrderByParts { get; set; }
        private List<string> PagingParts { get; set; }
        private List<string> GroupByParts { get; set; }

        private List<string> SubQueries { get; set; }
        private List<string> SubQueryLinkActions { get; set; }
        private bool _visitingSubQueryExpression;
        private QueryPartsAggregator _subQueryExpressionPartsAggregator;

        public QueryPartsAggregator()
        {
            FromParts = new List<string>();
            WhereParts = new List<string>();
            OrderByParts = new List<string>();
            PagingParts = new List<string>();
            GroupByParts = new List<string>();

            SubQueries = new List<string>();
            SubQueryLinkActions = new List<string>();
            _visitingSubQueryExpression = false;
        }

        public void SetSelectPart(string selectPart)
        {
            if (_visitingSubQueryExpression)
            {
                _subQueryExpressionPartsAggregator.SetSelectPart(selectPart);
            }
            else
            {
                SelectPart = selectPart;
            }
        }

        public void SetSelectPartAsScalar(string scalarPartFormat)
        {
            if (_visitingSubQueryExpression)
            {
                _subQueryExpressionPartsAggregator.SetSelectPartAsScalar(scalarPartFormat);
            }
            else
            {
                SelectPart = string.Format(scalarPartFormat, SelectPart);
            }
        }

        public void AddFromPart(string fromPart)
        {
            if (_visitingSubQueryExpression)
            {
                _subQueryExpressionPartsAggregator.AddFromPart(fromPart);
            }
            else
            {
                FromParts.Add(fromPart);
            }
        }

        public void AddWherePart(string formatString, params object[] args)
        {
            if (_visitingSubQueryExpression)
            {
                _subQueryExpressionPartsAggregator.AddWherePart(formatString, args);
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
            if (_visitingSubQueryExpression)
            {
                _subQueryExpressionPartsAggregator.AddOrderByPart(orderings);
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

        public void AddInnerJoinPart(string leftMember, string rightMember)
        {
            if (_visitingSubQueryExpression)
            {
                _subQueryExpressionPartsAggregator.AddInnerJoinPart(leftMember, rightMember);
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

        public void AddOuterJoinPart(string outerMember, string innerMember)
        {
            if (_visitingSubQueryExpression)
            {
                _subQueryExpressionPartsAggregator.AddOuterJoinPart(outerMember, innerMember);
            }
            else
            {
                var outerSource = outerMember.Split('.')[0];
                var innerSource = innerMember.Split('.')[0];
                var groupJoinPart = 
                    $"{outerSource} LEFT OUTER JOIN {innerSource} ON ({outerMember} = {innerMember})";

                // We're using the fact that the left source table was already added in AddFromPart
                var index = FromParts.IndexOf(outerSource);
                FromParts[index] = groupJoinPart;

                OrderByParts.Add(outerMember);
                OrderByParts.Add(innerMember);
            }
        }


        public void AddPagingPart(string limitter, string count)
        {
            if (_visitingSubQueryExpression)
            {
                _subQueryExpressionPartsAggregator.AddPagingPart(limitter, count);
            }
            else
            {
                PagingParts.Add($"{limitter} {count}");
            }
        }

        public void OpenSubQueryExpressionPartsAggregator()
        {
            _visitingSubQueryExpression = true;
            _subQueryExpressionPartsAggregator = new QueryPartsAggregator();
        }

        public void CloseSubQueryExpressionPartsAggregator()
        {
            SubQueries.Add(_subQueryExpressionPartsAggregator.BuildPsqlString().Trim(';'));
            _visitingSubQueryExpression = false;
        }

        public void AddSubQueryLinkAction(string queryLinkAction)
        {
            SubQueryLinkActions.Add(queryLinkAction);
            _visitingSubQueryExpression = false;
        }

        public string BuildPsqlString()
        {
            this.WrapSubQueryExpressionsToQueryParts();

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

        private void WrapSubQueryExpressionsToQueryParts()
        {
            if (SubQueries.Count != SubQueryLinkActions.Count)
            {
                throw new ArgumentException(
                    "Amounts of subqueries and the actions to take with them are not equal.");
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