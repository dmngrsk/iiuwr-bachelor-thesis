using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;
using Thesis.Relinq.Attributes;

namespace Thesis.Relinq.PsqlQueryGeneration
{
    /// Visits re-linq's QueryModel object and LINQ expressions in contains and generates a PostgreSQL query that is equivalent.
    public class PsqlGeneratingQueryModelVisitor : QueryModelVisitorBase
    {
        private readonly QueryPartsAggregator _queryParts;
        private readonly QueryParametersAggregator _parameterAggregator;

        /// Contains a map that translates aggregating result operator types to a format string wrapping a PostgreSQL aggregating function and its parameters.
        private static readonly Dictionary<Type, string> AggretatingOperators =
            new Dictionary<Type, string>()
            {
                { typeof(CountResultOperator),          "COUNT({0})" },
                { typeof(AverageResultOperator),        "AVG({0})" },
                { typeof(SumResultOperator),            "SUM({0})" },
                { typeof(MinResultOperator),            "MIN({0})" },
                { typeof(MaxResultOperator),            "MAX({0})" },
                { typeof(DistinctResultOperator),       "DISTINCT({0})" },
            };

        /// Contains a map that translates set-based result operator types to a format string wrapping a PostgreSQL set operation and its parameters.
        private static readonly Dictionary<Type, string> SetOperators = 
            new Dictionary<Type, string>
            {
                { typeof(UnionResultOperator),          "({0}) UNION ({1})" },
                { typeof(ConcatResultOperator),         "({0}) UNION ALL ({1})" },
                { typeof(IntersectResultOperator),      "({0}) INTERSECT ({1})" },
                { typeof(ExceptResultOperator),         "({0}) EXCEPT ({1})" }
            };

        public QueryPartsAggregator QueryParts => _queryParts;
        public QueryParametersAggregator ParameterAggregator => _parameterAggregator;

        public PsqlGeneratingQueryModelVisitor() : base()
        {
            _queryParts = new QueryPartsAggregator();
            _parameterAggregator = new QueryParametersAggregator();
        }

        /// Returns the generated PostgreSQL query in the form of a raw statement and a dictionary of arguments it uses.
        public QueryCommand GetPsqlCommand()
        {
            return new QueryCommand(_queryParts.BuildQueryStatement(), _parameterAggregator.Parameters);
        }

        /// Creates an instance of the PsqlGenratingQueryModelVisitor to visit the QueryModel provided as an argument and to generate a corresponding PostgreSQL query.
        public static QueryCommand GeneratePsqlQuery(QueryModel queryModel)
        {
            var visitor = new PsqlGeneratingQueryModelVisitor();
            visitor.VisitQueryModel(queryModel);
            return visitor.GetPsqlCommand();
        }



        public override void VisitQueryModel(QueryModel queryModel)
        {
            queryModel.SelectClause.Accept(this, queryModel);
            queryModel.MainFromClause.Accept(this, queryModel);
            this.VisitBodyClauses(queryModel.BodyClauses, queryModel);
            this.VisitResultOperators(queryModel.ResultOperators, queryModel);
        }

        public override void VisitSelectClause(SelectClause selectClause, QueryModel queryModel)
        {
            _queryParts.SetSelectPart(GetPsqlExpression(selectClause.Selector));
            base.VisitSelectClause(selectClause, queryModel);
        }

        public override void VisitMainFromClause(MainFromClause fromClause, QueryModel queryModel)
        {
            var tableName = fromClause.ItemType.GetTypeInfo().GetCustomAttribute<TableAttribute>().Name;
            _queryParts.AddFromPart($"\"{tableName}\"");

            base.VisitMainFromClause(fromClause, queryModel);
        }

        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
        {
            _queryParts.AddWherePart(GetPsqlExpression(whereClause.Predicate));
            base.VisitWhereClause(whereClause, queryModel, index);
        }

        public override void VisitOrderByClause(OrderByClause orderByClause, QueryModel queryModel, int index)
        {
            _queryParts.AddOrderByPart(orderByClause.Orderings.Select(o => 
                new Tuple<string, OrderingDirection>(GetPsqlExpression(o.Expression), o.OrderingDirection)));
           
            base.VisitOrderByClause(orderByClause, queryModel, index);
        }

        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, int index)
        {
            _queryParts.AddInnerJoinPart(
                GetPsqlExpression(joinClause.OuterKeySelector),
                GetPsqlExpression(joinClause.InnerKeySelector));

            base.VisitJoinClause(joinClause, queryModel, index);
        }

        public override void VisitAdditionalFromClause(AdditionalFromClause fromClause, QueryModel queryModel, int index)
        {
            var tableName = fromClause.ItemType.GetTypeInfo().GetCustomAttribute<TableAttribute>().Name;
            _queryParts.AddFromPart($"\"{tableName}\"");

            base.VisitAdditionalFromClause(fromClause, queryModel, index);
        }

        public override void VisitGroupJoinClause(GroupJoinClause groupJoinClause, QueryModel queryModel, int index)
        {
            _queryParts.AddOuterJoinPart(
                GetPsqlExpression(groupJoinClause.JoinClause.OuterKeySelector),
                GetPsqlExpression(groupJoinClause.JoinClause.InnerKeySelector));

            base.VisitGroupJoinClause(groupJoinClause, queryModel, index);
        }

        public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index)
        {
            var operatorType = resultOperator.GetType();

            if (AggretatingOperators.ContainsKey(operatorType))
            {
                _queryParts.SetSelectPartAsScalar(AggretatingOperators[operatorType]);
                base.VisitResultOperator(resultOperator, queryModel, index);
            }

            else if (SetOperators.ContainsKey(operatorType))
            {
                dynamic subQueryResultOperator = Convert.ChangeType(resultOperator, operatorType);
                this.VisitSubQueryExpression(subQueryResultOperator.Source2); // Source2 is a SubQueryExpression.
                _queryParts.AddSubQueryLinkAction(SetOperators[operatorType]);
                base.VisitResultOperator(resultOperator, queryModel, index);
            }

            else if (resultOperator is TakeResultOperator || resultOperator is SkipResultOperator)
            {
                var limitter = resultOperator is TakeResultOperator ? "LIMIT" : "OFFSET";
                var constExpression = resultOperator is TakeResultOperator ?
                    (resultOperator as TakeResultOperator).Count :
                    (resultOperator as SkipResultOperator).Count;
                
                _queryParts.AddPagingPart(limitter, GetPsqlExpression(constExpression));
                base.VisitResultOperator(resultOperator, queryModel, index);
            }

            else if (resultOperator is AnyResultOperator)
            {
                _queryParts.AddSubQueryLinkAction("EXISTS ({0})");
            }

            else if (resultOperator is AllResultOperator)
            {
                var subQuery = (resultOperator as AllResultOperator);
                _queryParts.AddWherePart($"NOT ({GetPsqlExpression(subQuery.Predicate)})");
                _queryParts.AddSubQueryLinkAction("NOT EXISTS ({0})");
            }

            else
            {
                throw new NotImplementedException(
                    $"This LINQ provider does not provide the {resultOperator} result operator.");
            }
        }



        /// Visits the QueryModel nested in a SubQueryExpression using this PsqlGeneratingQueryModelVisitor.
        private void VisitSubQueryExpression(Expression expression)
        {
            PsqlGeneratingExpressionVisitor.GetPsqlExpression(expression, this);
        }

        /// Creates an instance of the PsqlGeneratingExpressionVisitor based on this PsqlGeneratingQueryModelVisitor instance to visit the LINQ expression provided as an argument and to generate a corresponding part of the PostgreSQL query.

        private string GetPsqlExpression(Expression expression)
        {
            return PsqlGeneratingExpressionVisitor.GetPsqlExpression(expression, this);
        }
    }
}