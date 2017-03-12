using System;
using System.Linq.Expressions;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;
using System.Linq;

namespace Thesis.Relinq.PsqlQueryGeneration
{
    public class PsqlGeneratingQueryModelVisitor : QueryModelVisitorBase
    {
        private readonly QueryPartsAggregator _queryParts = new QueryPartsAggregator();
        private readonly ParameterAggregator _parameterAggregator = new ParameterAggregator();

        public static PsqlCommandData GeneratePsqlQuery(QueryModel queryModel)
        {
            var visitor = new PsqlGeneratingQueryModelVisitor();
            visitor.VisitQueryModel(queryModel);
            return visitor.GetPsqlCommand();
        }

        public PsqlCommandData GetPsqlCommand() =>
            new PsqlCommandData(_queryParts.BuildPsqlString(), _parameterAggregator.GetParameters());

        public override void VisitAdditionalFromClause(AdditionalFromClause fromClause, QueryModel queryModel, int index)
        {
            throw new NotImplementedException();
        }

        public override void VisitGroupJoinClause(GroupJoinClause groupJoinClause, QueryModel queryModel, int index)
        {
            throw new NotImplementedException();
        }

        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, int index)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public override void VisitOrdering(Ordering ordering, QueryModel queryModel, OrderByClause orderByClause, int index)
        {
            throw new NotImplementedException();
        }

        public override void VisitQueryModel(QueryModel queryModel)
        {
            queryModel.SelectClause.Accept(this, queryModel);
            queryModel.MainFromClause.Accept(this, queryModel);
            this.VisitBodyClauses(queryModel.BodyClauses, queryModel);
            this.VisitResultOperators(queryModel.ResultOperators, queryModel);
        }

        public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index)
        {
            // https://www.tutorialspoint.com/linq/linq_query_operators.htm
            
            if (resultOperator is CountResultOperator)
                _queryParts.SetSelectPart(string.Format("COUNT({0})", _queryParts.SelectPart));
            else
                throw new NotImplementedException();
            
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