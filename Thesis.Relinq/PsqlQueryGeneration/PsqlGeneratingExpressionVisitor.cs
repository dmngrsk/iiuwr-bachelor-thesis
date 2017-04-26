using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Thesis.Relinq.PsqlQueryGeneration
{
    public class PsqlGeneratingExpressionVisitor : RelinqExpressionVisitor
    {
        private readonly StringBuilder _psqlExpression = new StringBuilder();
        private readonly NpgsqlParameterAggregator _parameterAggregator;
        private readonly NpgsqlDatabaseSchema _dbSchema;

        private readonly static Dictionary<ExpressionType, string> _binaryExpressionOperatorsToString = 
            new Dictionary<ExpressionType, string>()
            {
                { ExpressionType.Equal,                 " = " },
                { ExpressionType.NotEqual,              " != " },
                { ExpressionType.GreaterThan,           " > " },
                { ExpressionType.GreaterThanOrEqual,    " >= " },
                { ExpressionType.LessThan,              " < " },
                { ExpressionType.LessThanOrEqual,       " <= " },

                { ExpressionType.Add,                   " + " },
                { ExpressionType.AddChecked,            " + " }, 
                { ExpressionType.Subtract,              " - " }, 
                { ExpressionType.SubtractChecked,       " - " },
                { ExpressionType.Multiply,              " * " },
                { ExpressionType.MultiplyChecked,       " * " },
                { ExpressionType.Divide,                " / " },
                { ExpressionType.Modulo,                " % " },

                { ExpressionType.And,                   " & " },
                { ExpressionType.Or,                    " | " },
                { ExpressionType.ExclusiveOr,           " # " },
                { ExpressionType.LeftShift,             " << " },
                { ExpressionType.RightShift,            " >> " },

                { ExpressionType.AndAlso,               " AND " },
                { ExpressionType.OrElse,                " OR " }
            };

        private readonly static Dictionary<string, string> _methodCallNamesToString = 
            new Dictionary<string, string>()
            {
                { "Equals",                             "{0} = {1}" },

                { "ToLower",                            "LOWER({0})" },
                { "ToUpper",                            "UPPER({0})" },
                { "Trim",                               "TRIM(both {1} from {0})" },
                { "TrimStart",                          "TRIM(leading {1} from {0})" },
                { "TrimEnd",                            "TRIM(trailing {1} from {0})" },

                { "Contains",                           "{0} LIKE '%' || {1} || '%'" },
                { "StartsWith",                         "{0} LIKE {1} || '%'" },
                { "EndsWith",                           "{0} LIKE '%' || {1}" },

                { "Length",                             "LENGTH({0})" },
                { "Concat",                             "CONCAT({0})" }

                // https://www.postgresql.org/docs/9.1/static/functions-string.html
            };

        private PsqlGeneratingExpressionVisitor(NpgsqlParameterAggregator parameterAggregator, 
            NpgsqlDatabaseSchema dbSchema)
        {
            _parameterAggregator = parameterAggregator;
            _dbSchema = dbSchema;
        }

        public static string GetPsqlExpression(Expression linqExpression, 
            NpgsqlParameterAggregator parameterAggregator, NpgsqlDatabaseSchema dbSchema)
        {
            var visitor = new PsqlGeneratingExpressionVisitor(parameterAggregator, dbSchema);
            visitor.Visit(linqExpression);
            return visitor.GetPsqlExpression();
        }

        private string GetPsqlExpression() => _psqlExpression.ToString();

        protected override Expression VisitBinary(BinaryExpression expression)
        {
            this.Visit(expression.Left);
    
            var isAddingStrings = expression.NodeType == ExpressionType.Add && 
                (expression.Left.Type == typeof(string)
                || expression.Right.Type == typeof(string));

            if (isAddingStrings)
                _psqlExpression.Append(" || ");
            else
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
            var parameterName = _parameterAggregator.AddParameter(expression.Value);
            _psqlExpression.Append($"@{parameterName}");
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
            if (expression.Expression.NodeType == ExpressionType.MemberAccess &&
                expression.Member.Name == "Length")
            {
                _psqlExpression.Append("LENGTH(");
                this.Visit(expression.Expression);
                _psqlExpression.Append(")");
            }
            else
            {
                this.Visit(expression.Expression);
                _psqlExpression.Append($".\"{_dbSchema.GetColumnName(expression.Member.Name)}\"");
            }

            return expression;
        }
        // Visits the children of the System.Linq.Expressions.MemberInitExpression.
        protected override Expression VisitMemberInit(MemberInitExpression expression)
        {
            return expression;
        }

        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            var methodName = expression.Method.Name;

            if (_methodCallNamesToString.ContainsKey(methodName))
            {
                this.Visit(expression.Object);
                var expressionAccumulator = new List<object>(new object[] { _psqlExpression.ToString() });
                _psqlExpression.Clear();

                foreach (var argument in expression.Arguments)
                {
                    this.Visit(argument);
                    expressionAccumulator.Add(_psqlExpression.ToString());
                    _psqlExpression.Clear();
                }

                var expectedArguments = Regex.Matches(
                    _methodCallNamesToString[methodName], "\\{([^}]+)\\}"
                );

                while (expressionAccumulator.Count < expectedArguments.Count) 
                {
                    expressionAccumulator.Add(string.Empty);
                }
                
                switch (methodName)
                {
                    case "Concat":
                        expressionAccumulator.RemoveAt(0);
                        _psqlExpression.AppendFormat(
                            _methodCallNamesToString[methodName],
                            string.Join(", ", expressionAccumulator.Select(x => x.ToString()))
                        );
                        break;

                    default:
                    _psqlExpression.AppendFormat(
                        _methodCallNamesToString[methodName], 
                        expressionAccumulator.ToArray()
                    );
                    break;
                }

                return expression;
            }

            throw new NotImplementedException(
                $"This LINQ provider does not provide the {methodName} method.");
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

            _psqlExpression.Append($"\"{_dbSchema.GetTableName(type)}\"");
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
            if (expression.NodeType == ExpressionType.Not)
            {
                _psqlExpression.Append("NOT (");
                this.Visit(expression.Operand);
                _psqlExpression.Append(")");
            }

            else
            {
                this.Visit(expression.Operand as MemberExpression);
            }
                
            return expression;
        }
    }
}