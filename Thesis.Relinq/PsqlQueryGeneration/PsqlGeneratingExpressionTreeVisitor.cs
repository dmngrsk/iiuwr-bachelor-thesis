using System;
using System.Linq.Expressions;
using System.Text;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionVisitors;
using Remotion.Linq.Parsing;

namespace Thesis.Relinq.PsqlQueryGeneration
{
    public class PsqlGeneratingExpressionTreeVisitor : RelinqExpressionVisitor
    {
        private readonly StringBuilder _psqlExpression = new StringBuilder();
        private readonly ParameterAggregator _parameterAggregator;

        public static string GetPsqlExpression(Expression linqExpression, ParameterAggregator parameterAggregator)
        {
            var visitor = new PsqlGeneratingExpressionTreeVisitor(parameterAggregator);
            visitor.Visit(linqExpression);
            return visitor.GetPsqlExpression();
        }

        private string GetPsqlExpression() => _psqlExpression.ToString();

        private PsqlGeneratingExpressionTreeVisitor(ParameterAggregator parameterAggregator)
        {
            _parameterAggregator = parameterAggregator;
        }

        // FOR ALL METHODS:
        // Parameters:
        //   expression:
        //     The expression to visit.
        //
        // Returns:
        //     The modified expression, if it or any subexpression was modified; otherwise,
        //     returns the original expression.

        protected override Expression VisitBinary(BinaryExpression expression)
        {
            this.Visit(expression.Left);

            // TODO: filling the rest of this, moving to a dictionary
            switch (expression.NodeType)
            {
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    _psqlExpression.Append(" + ");
                    break; 

                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    _psqlExpression.Append("  ");
                    break;

                case ExpressionType.Divide:
                    _psqlExpression.Append("   ");
                    break;

                case ExpressionType.Equal:
                    _psqlExpression.Append(" = ");
                    break;

                default:
                    throw new ArgumentException("{0} is not supported.", expression.Type.ToString());
            }

            this.Visit(expression.Right);
            return expression;
        }
        // Visits the children of the System.Linq.Expressions.BlockExpression.
        protected override Expression VisitBlock(BlockExpression expression)
        {
            return expression;
        }
        // Visits the children of the System.Linq.Expressions.ConditionalExpression.
        protected override Expression VisitConditional(ConditionalExpression expression)
        {
            return expression;
        }
        
        protected override Expression VisitConstant(ConstantExpression expression)
        {
            // TODO: type check
            if (expression.Type == typeof(string))
            {
                _psqlExpression.Append("'");
                _psqlExpression.Append(expression.Value);
                _psqlExpression.Append("'");
            }

            else
                _psqlExpression.Append(expression.Value);

            return expression;
        }
        // Visits the System.Linq.Expressions.DebugInfoExpression.
        protected override Expression VisitDebugInfo(DebugInfoExpression expression)
        {
            return expression;
        }
        // Visits the System.Linq.Expressions.DefaultExpression.
        protected override Expression VisitDefault(DefaultExpression expression)
        {
            return expression;
        }
        // Visits the children of the extension expression.
        protected override Expression VisitExtension(Expression expression)
        {
            return expression;
        }
        // Visits the children of the System.Linq.Expressions.GotoExpression.
        protected override Expression VisitGoto(GotoExpression expression)
        {
            return expression;
        }
        // Visits the children of the System.Linq.Expressions.IndexExpression.
        protected override Expression VisitIndex(IndexExpression expression)
        {
            return expression;
        }
        // Visits the children of the System.Linq.Expressions.InvocationExpression.
        protected override Expression VisitInvocation(InvocationExpression expression)
        {
            return expression;
        }
        // Visits the children of the System.Linq.Expressions.LabelExpression.
        protected override Expression VisitLabel(LabelExpression expression)
        {
            return expression;
        }
        // Visits the children of the System.Linq.Expressions.Expression`1.
        protected override Expression VisitLambda<T>(Expression<T> expression)
        {
            return expression;
        }
        // Visits the children of the System.Linq.Expressions.ListInitExpression.
        protected override Expression VisitListInit(ListInitExpression expression)
        {
            return expression;
        }
        // Visits the children of the System.Linq.Expressions.LoopExpression.
        protected override Expression VisitLoop(LoopExpression expression)
        {
            return expression;
        }
        
        protected override Expression VisitMember(MemberExpression expression)
        {
            _psqlExpression.Append("\"");
            _psqlExpression.Append(expression.Member.Name);
            _psqlExpression.Append("\"");
            return expression;
        }
        // Visits the children of the System.Linq.Expressions.MemberInitExpression.
        protected override Expression VisitMemberInit(MemberInitExpression expression)
        {
            return expression;
        }
        // Visits the children of the System.Linq.Expressions.MethodCallExpression.
        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            return expression;
        }

        protected override Expression VisitNew(NewExpression expression)
        {
            this.Visit(expression.Arguments[0]);
            
            for (int i = 1; i < expression.Arguments.Count; i++)
            {
                _psqlExpression.Append(", ");
                this.Visit(expression.Arguments[i]);
            }

            return expression;
        }
        // Visits the children of the System.Linq.Expressions.NewArrayExpression.
        protected override Expression VisitNewArray(NewArrayExpression expression)
        {
            return expression;
        }
        // Visits the System.Linq.Expressions.ParameterExpression.
        protected override Expression VisitParameter(ParameterExpression expression)
        {
            return expression;
        }

        protected override Expression VisitQuerySourceReference(QuerySourceReferenceExpression expression)
        {
            _psqlExpression.Append("*");
            return expression;
        }
        // Visits the children of the System.Linq.Expressions.RuntimeVariablesExpression.
        protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression expression)
        {
            return expression;
        }
        // Visits the children of the System.Linq.Expressions.SwitchExpression.
        protected override Expression VisitSwitch(SwitchExpression expression)
        {
            return expression;
        }
        // Visits the children of the System.Linq.Expressions.TryExpression.
        protected override Expression VisitTry(TryExpression expression)
        {
            return expression;
        }
        // Visits the children of the System.Linq.Expressions.TypeBinaryExpression.
        protected override Expression VisitTypeBinary(TypeBinaryExpression expression)
        {
            return expression;
        }
        // Visits the children of the System.Linq.Expressions.UnaryExpression.
        protected override Expression VisitUnary(UnaryExpression expression)
        {
            return expression;
        }
    }
}