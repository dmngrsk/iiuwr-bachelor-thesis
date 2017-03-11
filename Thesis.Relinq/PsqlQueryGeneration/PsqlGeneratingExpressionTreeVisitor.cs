using System;
using System.Linq.Expressions;
using System.Text;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionVisitors;
using Remotion.Linq.Parsing;

namespace Thesis.Relinq.PsqlQueryGeneration
{
    public class PsqlGeneratingExpressionTreeVisitor : ExpressionVisitor
    {
        StringBuilder _psqlExpression = new StringBuilder();
        private readonly ParameterAggregator _parameterAggregator;

        private PsqlGeneratingExpressionTreeVisitor(ParameterAggregator parameterAggregator)
        {
            _parameterAggregator = parameterAggregator;
        }

        // FOR ALL METHODS:
        // Parameters:
        //   node:
        //     The expression to visit.
        //
        // Returns:
        //     The modified expression, if it or any subexpression was modified; otherwise,
        //     returns the original expression.

        // Visits the children of the System.Linq.Expressions.BinaryExpression.
        protected override Expression VisitBinary(BinaryExpression node)
        {
            return node;
        }
        // Visits the children of the System.Linq.Expressions.BlockExpression.
        protected override Expression VisitBlock(BlockExpression node)
        {
            return node;
        }
        // Visits the children of the System.Linq.Expressions.ConditionalExpression.
        protected override Expression VisitConditional(ConditionalExpression node)
        {
            return node;
        }
        // Visits the System.Linq.Expressions.ConstantExpression.
        protected override Expression VisitConstant(ConstantExpression node)
        {
            return node;
        }
        // Visits the System.Linq.Expressions.DebugInfoExpression.
        protected override Expression VisitDebugInfo(DebugInfoExpression node)
        {
            return node;
        }
        // Visits the System.Linq.Expressions.DefaultExpression.
        protected override Expression VisitDefault(DefaultExpression node)
        {
            return node;
        }
        // Visits the children of the extension expression.
        protected override Expression VisitExtension(Expression node)
        {
            return node;
        }
        // Visits the children of the System.Linq.Expressions.GotoExpression.
        protected override Expression VisitGoto(GotoExpression node)
        {
            return node;
        }
        // Visits the children of the System.Linq.Expressions.IndexExpression.
        protected override Expression VisitIndex(IndexExpression node)
        {
            return node;
        }
        // Visits the children of the System.Linq.Expressions.InvocationExpression.
        protected override Expression VisitInvocation(InvocationExpression node)
        {
            return node;
        }
        // Visits the children of the System.Linq.Expressions.LabelExpression.
        protected override Expression VisitLabel(LabelExpression node)
        {
            return node;
        }
        // Visits the children of the System.Linq.Expressions.Expression`1.
        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            return node;
        }
        // Visits the children of the System.Linq.Expressions.ListInitExpression.
        protected override Expression VisitListInit(ListInitExpression node)
        {
            return node;
        }
        // Visits the children of the System.Linq.Expressions.LoopExpression.
        protected override Expression VisitLoop(LoopExpression node)
        {
            return node;
        }
        // Visits the children of the System.Linq.Expressions.MemberExpression.
        protected override Expression VisitMember(MemberExpression node)
        {
            return node;
        }
        // Visits the children of the System.Linq.Expressions.MemberInitExpression.
        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            return node;
        }
        // Visits the children of the System.Linq.Expressions.MethodCallExpression.
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            return node;
        }
        // Visits the children of the System.Linq.Expressions.NewExpression.
        protected override Expression VisitNew(NewExpression node)
        {
            return node;
        }
        // Visits the children of the System.Linq.Expressions.NewArrayExpression.
        protected override Expression VisitNewArray(NewArrayExpression node)
        {
            return node;
        }
        // Visits the System.Linq.Expressions.ParameterExpression.
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node;
        }
        // Visits the children of the System.Linq.Expressions.RuntimeVariablesExpression.
        protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node)
        {
            return node;
        }
        // Visits the children of the System.Linq.Expressions.SwitchExpression.
        protected override Expression VisitSwitch(SwitchExpression node)
        {
            return node;
        }
        // Visits the children of the System.Linq.Expressions.TryExpression.
        protected override Expression VisitTry(TryExpression node)
        {
            return node;
        }
        // Visits the children of the System.Linq.Expressions.TypeBinaryExpression.
        protected override Expression VisitTypeBinary(TypeBinaryExpression node)
        {
            return node;
        }
        // Visits the children of the System.Linq.Expressions.UnaryExpression.
        protected override Expression VisitUnary(UnaryExpression node)
        {
            return node;
        }
    }
}