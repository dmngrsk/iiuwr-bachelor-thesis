using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;

namespace Thesis.Relinq.PsqlQueryGeneration
{
    public class PsqlGeneratingQueryModelVisitor : QueryModelVisitorBase
    {
        private readonly QueryPartsAggregator _queryParts = new QueryPartsAggregator();
        private readonly NpgsqlParameterAggregator _parameterAggregator = new NpgsqlParameterAggregator();

        public static NpgsqlCommandData GeneratePsqlQuery(QueryModel queryModel)
        {
            var visitor = new PsqlGeneratingQueryModelVisitor();
            visitor.VisitQueryModel(queryModel);
            return visitor.GetPsqlCommand();
        }

        public NpgsqlCommandData GetPsqlCommand() =>
            new NpgsqlCommandData(_queryParts.BuildPsqlString(), _parameterAggregator.GetParameters());

        public override void VisitQueryModel(QueryModel queryModel)
        {
            queryModel.SelectClause.Accept(this, queryModel);
            queryModel.MainFromClause.Accept(this, queryModel);
            this.VisitBodyClauses(queryModel.BodyClauses, queryModel);
            this.VisitResultOperators(queryModel.ResultOperators, queryModel);
        }

        public override void VisitAdditionalFromClause(AdditionalFromClause fromClause, QueryModel queryModel, int index)
        {
            _queryParts.AddFromPart(fromClause);
            base.VisitAdditionalFromClause(fromClause, queryModel, index);
        }

        public override void VisitGroupJoinClause(GroupJoinClause groupJoinClause, QueryModel queryModel, int index)
        {
            throw new NotImplementedException();
        }

        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, int index)
        {
            _queryParts.AddJoinPart(
                GetPsqlExpression(joinClause.OuterKeySelector),
                GetPsqlExpression(joinClause.InnerKeySelector));

            base.VisitJoinClause(joinClause, queryModel, index);
        }

        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, GroupJoinClause groupJoinClause)
        {
            throw new NotImplementedException();
        }

        public override void VisitMainFromClause(MainFromClause fromClause, QueryModel queryModel)
        {
            _queryParts.AddFromPart(fromClause);
            base.VisitMainFromClause(fromClause, queryModel);
        }

        public override void VisitOrderByClause(OrderByClause orderByClause, QueryModel queryModel, int index)
        {
            _queryParts.AddOrderByPart(orderByClause.Orderings.Select(o => 
                new Tuple<string, OrderingDirection>(GetPsqlExpression(o.Expression), o.OrderingDirection)));
           
            base.VisitOrderByClause(orderByClause, queryModel, index);
        }

        private static Dictionary<Type, string> _resultOperatorsToString =
            new Dictionary<Type, string>()
            {
                { typeof(CountResultOperator), "COUNT({0})" },
                { typeof(AverageResultOperator), "AVG({0})"},
                { typeof(SumResultOperator), "SUM({0})"},
                { typeof(MinResultOperator), "MIN({0})"},
                { typeof(MaxResultOperator), "MAX({0})"}
            };

        public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index)
        {
            // TODO: https://www.tutorialspoint.com/linq/linq_query_operators.htm
            _queryParts.SetSelectPartAsScalar(_resultOperatorsToString[resultOperator.GetType()]);
            base.VisitResultOperator(resultOperator, queryModel, index);
        }

        public override void VisitSelectClause(SelectClause selectClause, QueryModel queryModel)
        {
            _queryParts.SetSelectPart(GetPsqlExpression(selectClause.Selector));
            base.VisitSelectClause(selectClause, queryModel);
        }

        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
        {
            _queryParts.AddWherePart(GetPsqlExpression(whereClause.Predicate));
            base.VisitWhereClause(whereClause, queryModel, index);
        }

        private string GetPsqlExpression(Expression expression) =>
            PsqlGeneratingExpressionTreeVisitor.GetPsqlExpression(expression, _parameterAggregator);
    }
}