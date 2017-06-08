using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;

namespace Thesis.Relinq.PsqlQueryGeneration
{
    /// Visits re-linq's QueryModel object and LINQ expressions in contains and generates a PostgreSQL query that is equivalent.
    public class PsqlGeneratingQueryModelVisitor : QueryModelVisitorBase
    {
        private readonly QueryPartsAggregator _queryParts;
        private readonly QueryParametersAggregator _parameterAggregator;
        private readonly DbSchema _dbSchema;

        /// Contains a map that translates aggregating result operator types to a format string wrapping a PostgreSQL aggregating function and its parameters.
        private readonly static Dictionary<Type, string> _aggretatingOperators =
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
        private readonly static Dictionary<Type, string> _setOperators = 
            new Dictionary<Type, string>
            {
                { typeof(UnionResultOperator),          "({0}) UNION ({1})" },
                { typeof(ConcatResultOperator),         "({0}) UNION ALL ({1})" },
                { typeof(IntersectResultOperator),      "({0}) INTERSECT ({1})" },
                { typeof(ExceptResultOperator),         "({0}) EXCEPT ({1})" }
            };

        public QueryPartsAggregator QueryParts { get { return _queryParts; } }
        public QueryParametersAggregator ParameterAggregator { get { return _parameterAggregator; } }
        public DbSchema DbSchema { get { return _dbSchema; } }

        public PsqlGeneratingQueryModelVisitor(DbSchema dbSchema) : base()
        {
            _queryParts = new QueryPartsAggregator();
            _parameterAggregator = new QueryParametersAggregator();
            _dbSchema = dbSchema;
        }

        /// Returns the generated PostgreSQL query in the form of a raw statement and a dictionary of arguments it uses.
        public QueryCommand GetPsqlCommand()
        {
            return new QueryCommand(_queryParts.BuildQueryStatement(), _parameterAggregator.Parameters);
        }

        /// Creates an instance of the PsqlGenratingQueryModelVisitor based on provided DbSchema instance to visit the QueryModel provided as an argument and to generate a corresponding PostgreSQL query.
        public static QueryCommand GeneratePsqlQuery(QueryModel queryModel, DbSchema dbSchema)
        {
            var visitor = new PsqlGeneratingQueryModelVisitor(dbSchema);
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
            var fromPart = fromClause.ItemType.Name;
            _queryParts.AddFromPart($"\"{_dbSchema.GetMatchingTableName(fromPart)}\"");

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
            var fromPart = fromClause.ItemType.Name;
            _queryParts.AddFromPart($"\"{_dbSchema.GetMatchingTableName(fromPart)}\"");

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
            // TODO: https://www.tutorialspoint.com/linq/linq_query_operators.htm

            var operatorType = resultOperator.GetType();

            if (_aggretatingOperators.ContainsKey(operatorType))
            {
                _queryParts.SetSelectPartAsScalar(_aggretatingOperators[operatorType]);
                base.VisitResultOperator(resultOperator, queryModel, index);
            }

            else if (_setOperators.ContainsKey(operatorType))
            {
                dynamic subQueryResultOperator = Convert.ChangeType(resultOperator, operatorType);
                this.VisitSubQueryExpression(subQueryResultOperator.Source2); // Source2 is a SubQueryExpression.
                _queryParts.AddSubQueryLinkAction(_setOperators[operatorType]);
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

            else if (resultOperator is GroupResultOperator)
            {
                Console.WriteLine("foo");
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