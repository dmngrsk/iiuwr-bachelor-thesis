using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Remotion.Linq.Clauses;

namespace Thesis.Relinq.PsqlQueryGeneration
{
    /// Aggregates query parts and provides a method to generate a complete SQL statement.
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

        /// Sets the SELECT part of the SQL query.
        ///
        /// If a subquery parts aggregator is open, redirects the call to it instead.
        public void SetSelectPart(string selectPart)
        {
            if (_visitingSubQueryExpression)
            {
                _subQueryExpressionPartsAggregator.SetSelectPart(selectPart);
            }
            else if (!string.IsNullOrEmpty(selectPart))
            {
                SelectPart = selectPart;
            }
        }

        /// Wraps the SELECT part of a query using a format string provided in the argument.
        ///
        /// If a subquery parts aggregator is open, redirects the call to it instead.
        public void SetSelectPartAsScalar(string scalarPartFormat)
        {
            if (_visitingSubQueryExpression)
            {
                _subQueryExpressionPartsAggregator.SetSelectPartAsScalar(scalarPartFormat);
            }
            else
            {
                var renamingPartIndex = SelectPart.IndexOf("\" AS \"") + 1;
                SelectPart = renamingPartIndex > 0 ? SelectPart.Substring(0, renamingPartIndex) : SelectPart;

                SelectPart = string.Format(scalarPartFormat, SelectPart);
            }
        }

        /// Adds a FROM part of the SQL query.
        ///
        /// If a subquery parts aggregator is open, redirects the call to it instead.
        public void AddFromPart(string fromPart)
        {
            if (_visitingSubQueryExpression)
            {
                _subQueryExpressionPartsAggregator.AddFromPart(fromPart);
            }
            else if (FromParts.Where(x => x.Contains(fromPart)).Count() == 0)
            {
                FromParts.Add(fromPart);
            }
        }

        /// Adds a WHERE part of the SQL query.
        ///
        /// If a subquery parts aggregator is open, redirects the call to it instead.
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

        /// Adds an ORDER BY part of the SQL query.
        ///
        /// If a subquery parts aggregator is open, redirects the call to it instead.
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

        /// Adds an INNER JOIN part of the SQL query as FROM parts, while replacing corresponding FROM parts.
        ///
        /// If a subquery parts aggregator is open, redirects the call to it instead.
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

        /// Adds a LEFT/RIGHT JOIN part of the SQL query as FROM parts, while replacing corresponding FROM parts.
        ///
        /// If a subquery parts aggregator is open, redirects the call to it instead.
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

                var groupingPart = 
                    $"(SELECT COUNT(*) FROM {innerSource} AS \"temp_{innerSource.Substring(1)} " +
                    $"WHERE \"temp_{innerMember.Substring(1)} = {outerMember}) AS";
                if (SelectPart.Contains(groupingPart))
                {
                    OrderByParts.Add(outerMember);
                    OrderByParts.Add(innerMember);
                }
            }
        }

        /// Adds a paging part of the SQL query. 
        ///
        /// If a subquery parts aggregator is open, redirects the call to it instead.
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

        /// Opens a subquery parts aggregator.
        ///
        /// If a subquery parts aggregator is open and is also visiting a subquery, 
        /// redirects the call to it instead.
        public void OpenSubQueryExpressionPartsAggregator()
        {
            if (_visitingSubQueryExpression && _subQueryExpressionPartsAggregator._visitingSubQueryExpression)
            {
                _subQueryExpressionPartsAggregator.OpenSubQueryExpressionPartsAggregator();
            }
            else
            {
                _visitingSubQueryExpression = true;
                _subQueryExpressionPartsAggregator = new QueryPartsAggregator();
            }
        }

        /// Closes the subquery parts aggregator and adds a subquery as a part of this query.
        ///
        /// If a subquery parts aggregator is open and is also visiting a subquery, 
        /// redirects the call to it instead.
        public void CloseSubQueryExpressionPartsAggregator()
        {
            if (_visitingSubQueryExpression && _subQueryExpressionPartsAggregator._visitingSubQueryExpression)
            {
                _subQueryExpressionPartsAggregator.CloseSubQueryExpressionPartsAggregator();
            }
            else 
            {
                if (string.IsNullOrEmpty(SelectPart))
                {
                    SelectPart = _subQueryExpressionPartsAggregator.SelectPart;
                }
                else 
                {
                    SubQueries.Add(_subQueryExpressionPartsAggregator.BuildQueryStatement().Trim(';'));
                }

                _visitingSubQueryExpression = false;
            }
        }

        /// Adds a format string that specifies an action to take with a previously generated subquery part.
        ///
        /// If a subquery parts aggregator is open and is also visiting a subquery, 
        /// redirects the call to it instead.
        public void AddSubQueryLinkAction(string queryLinkAction)
        {
            if (_visitingSubQueryExpression && _subQueryExpressionPartsAggregator._visitingSubQueryExpression)
            {
                _subQueryExpressionPartsAggregator.AddSubQueryLinkAction(queryLinkAction);
            }
            else 
            {
                SubQueryLinkActions.Add(queryLinkAction);
                _visitingSubQueryExpression = false;
            }
        }

        /// Builds a SQL statement using all the query parts previously added.
        public string BuildQueryStatement()
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
            if (_subQueryExpressionPartsAggregator != null)
            {
                _subQueryExpressionPartsAggregator.WrapSubQueryExpressionsToQueryParts();            
            }

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