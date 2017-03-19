using System.Collections.Generic;
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

        private PsqlGeneratingExpressionTreeVisitor(ParameterAggregator parameterAggregator)
        {
            _parameterAggregator = parameterAggregator;
        }

        public static string GetPsqlExpression(Expression linqExpression, ParameterAggregator parameterAggregator)
        {
            var visitor = new PsqlGeneratingExpressionTreeVisitor(parameterAggregator);
            visitor.Visit(linqExpression);
            return visitor.GetPsqlExpression();
        }

        private string GetPsqlExpression() => _psqlExpression.ToString();

        private static Dictionary<ExpressionType, string> _binaryExpressionOperatorsToString = 
            new Dictionary<ExpressionType, string>()
            {
                { ExpressionType.Equal,                 " = " },
                { ExpressionType.NotEqual,              " != "},
                { ExpressionType.GreaterThan,           " > "},
                { ExpressionType.GreaterThanOrEqual,    " >= " },
                { ExpressionType.LessThan,              " < " },
                { ExpressionType.LessThanOrEqual,       " <= "},

                { ExpressionType.Add,                   " + " },
                { ExpressionType.AddChecked,            " + " }, 
                { ExpressionType.Subtract,              " - " }, 
                { ExpressionType.SubtractChecked,       " - " },
                { ExpressionType.Multiply,              " * " },
                { ExpressionType.MultiplyChecked,       " * " },
                { ExpressionType.Divide,                " / " },
                { ExpressionType.Modulo,                " % "},

                { ExpressionType.And,                   " AND "},
                { ExpressionType.AndAlso,               " AND "},
                { ExpressionType.Or,                    " OR "},
                { ExpressionType.OrElse,                " OR "}
            };

        protected override Expression VisitBinary(BinaryExpression expression)
        {
            this.Visit(expression.Left);
            _psqlExpression.Append(_binaryExpressionOperatorsToString[expression.NodeType]);
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
            _psqlExpression.Append($"'{expression.Value}'");
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
            this.Visit(expression.Expression);
            _psqlExpression.Append($".\"{expression.Member.Name}\"");
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
            string fullType = expression.ReferencedQuerySource.ItemType.ToString();
            int index = fullType.LastIndexOf('.') + 1;
            string type = fullType.Substring(index);

            _psqlExpression.Append(type);
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
        
        protected override Expression VisitUnary(UnaryExpression expression)
        {
            this.Visit(expression.Operand as MemberExpression);
            return expression;
        }
    }
}