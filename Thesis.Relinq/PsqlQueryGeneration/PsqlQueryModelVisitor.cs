using System;
using System.Linq.Expressions;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;
using System.Linq;

namespace Thesis.Relinq.PsqlQueryGeneration
{
    public class PsqlQueryModelVisitor : QueryModelVisitorBase
    {
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index)
        {
            throw new NotImplementedException();
        }

        public override void VisitSelectClause(SelectClause selectClause, QueryModel queryModel)
        {
            throw new NotImplementedException();
        }
        
        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
        {
            throw new NotImplementedException();
        }
    }
}