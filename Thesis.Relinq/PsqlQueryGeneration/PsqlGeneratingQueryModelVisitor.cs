using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;

namespace Thesis.Relinq.PsqlQueryGeneration
{
    public class PsqlGeneratingQueryModelVisitor : QueryModelVisitorBase
    {
        private readonly QueryPartsAggregator _queryParts;
        private readonly NpgsqlParameterAggregator _parameterAggregator;
        private readonly NpgsqlDatabaseSchema _dbSchema;

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

        private readonly static Dictionary<Type, string> _setOperators = 
            new Dictionary<Type, string>
            {
                { typeof(UnionResultOperator),          "({0}) UNION ({1})" },
                { typeof(ConcatResultOperator),         "({0}) UNION ALL ({1})" },
                { typeof(IntersectResultOperator),      "({0}) INTERSECT ({1})" },
                { typeof(ExceptResultOperator),         "({0}) EXCEPT ({1})" }
            };

        public QueryPartsAggregator QueryParts { get { return _queryParts; } }
        public NpgsqlParameterAggregator ParameterAggregator { get { return _parameterAggregator; } }
        public NpgsqlDatabaseSchema DbSchema { get { return _dbSchema; } }

        public PsqlGeneratingQueryModelVisitor(NpgsqlDatabaseSchema dbSchema) : base()
        {
            _queryParts = new QueryPartsAggregator();
            _parameterAggregator = new NpgsqlParameterAggregator();
            _dbSchema = dbSchema;
        }

        public NpgsqlCommandData GetPsqlCommand()
        {
            return new NpgsqlCommandData(_queryParts.BuildPsqlString(), _parameterAggregator.Parameters);
        }

        public static NpgsqlCommandData GeneratePsqlQuery(QueryModel queryModel, NpgsqlDatabaseSchema dbSchema)
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
            _queryParts.AddFromPart($"\"{_dbSchema.GetTableName(fromPart)}\"");

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

        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, GroupJoinClause groupJoinClause)
        {
            
            base.VisitJoinClause(joinClause, queryModel, groupJoinClause);
        }


        public override void VisitAdditionalFromClause(AdditionalFromClause fromClause, QueryModel queryModel, int index)
        {
            var fromPart = fromClause.ItemType.Name;
            _queryParts.AddFromPart($"\"{_dbSchema.GetTableName(fromPart)}\"");

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



        /// Visits the QueryModel in a SubQueryExpression using this PsqlGeneratingQueryModelVisitor.
        private void VisitSubQueryExpression(Expression expression)
        {
            PsqlGeneratingExpressionVisitor.GetPsqlExpression(expression, this);
        }

        private string GetPsqlExpression(Expression expression)
        {
            return PsqlGeneratingExpressionVisitor.GetPsqlExpression(expression, this);
        }
    }
}