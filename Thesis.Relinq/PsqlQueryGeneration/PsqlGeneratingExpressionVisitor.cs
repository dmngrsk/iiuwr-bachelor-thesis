using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Parsing;

namespace Thesis.Relinq.PsqlQueryGeneration
{
    public class PsqlGeneratingExpressionVisitor : RelinqExpressionVisitor
    {
        private readonly StringBuilder _psqlExpressionBuilder;
        private readonly PsqlGeneratingQueryModelVisitor _queryModelVisitor;
        private bool _visitorTriggeredByMemberVisitor = false;
        private bool _conditionalStart = true;

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
                { "Reverse",                            "REVERSE({0})" },
                { "Length",                             "LENGTH({0})" },
                
                { "Concat",                             "CONCAT({0})" },

                { "Substring",                          "SUBSTRING({0} FROM {1}+1)" },
                { "SubstringFor",                       "SUBSTRING({0} FROM {1}+1 FOR {2})" },

                { "Replace",                            "REPLACE({0}, {1}, {2})" },

                { "Trim",                               "TRIM(both {1} from {0})" },
                { "TrimStart",                          "TRIM(leading {1} from {0})" },
                { "TrimEnd",                            "TRIM(trailing {1} from {0})" },

                { "Contains",                           "{0} LIKE '%' || {1} || '%'" },
                { "StartsWith",                         "{0} LIKE {1} || '%'" },
                { "EndsWith",                           "{0} LIKE '%' || {1}" }
            };

        private PsqlGeneratingExpressionVisitor(PsqlGeneratingQueryModelVisitor queryModelVisitor)
        {
            _psqlExpressionBuilder = new StringBuilder();
            _queryModelVisitor = queryModelVisitor;

            _visitorTriggeredByMemberVisitor = false;
            _conditionalStart = true;
        }

        public static string GetPsqlExpression(Expression linqExpression,
            PsqlGeneratingQueryModelVisitor queryModelVisitor)
        {
            var visitor = new PsqlGeneratingExpressionVisitor(queryModelVisitor);
            visitor.Visit(linqExpression);
            return visitor.GetPsqlExpression();
        }



        protected override Expression VisitQuerySourceReference(QuerySourceReferenceExpression expression)
        {
            if (_visitorTriggeredByMemberVisitor)
            {
                string typeName = expression.ReferencedQuerySource.ItemType.Name;
                _psqlExpressionBuilder.Append($"\"{_queryModelVisitor.DbSchema.GetTableName(typeName)}\"");
            }

            else if (expression.Type.FullName.Contains("IGrouping")) // WIP.
            {
                var insideType = expression.Type.GetGenericArguments()[0];

                var sth = this.GetNestedPsqlExpression((expression.ReferencedQuerySource as MainFromClause).FromExpression);

                var groupResultOperator = ((expression.ReferencedQuerySource as MainFromClause)
                    .FromExpression as SubQueryExpression)
                    .QueryModel.ResultOperators[0] as GroupResultOperator;

                this.Visit(groupResultOperator.ElementSelector);
                _psqlExpressionBuilder.Append(", ");
                this.Visit(groupResultOperator.KeySelector);

                // throw new NotImplementedException("This LINQ provider does not provide grouping yet.");
            }

            else if (expression.ReferencedQuerySource is GroupJoinClause)
            {
                var itemType = expression.ReferencedQuerySource.ItemType.GetGenericArguments()[0];
                var tableName = _queryModelVisitor.DbSchema.GetTableName(itemType.Name);

                var properties = itemType.GetPublicSettableProperties();
                var rowNames = properties.Select(x =>
                {
                    var columnName = _queryModelVisitor.DbSchema.GetColumnName(x.Name);
                    return $"\"{tableName}\".\"{columnName}\" AS \"{itemType.Name}.{x.Name}\"";
                });

                _psqlExpressionBuilder.Append(string.Join(", ", rowNames));

                var joinClause = (expression.ReferencedQuerySource as GroupJoinClause).JoinClause;
                var outerKeySelector = this.GetNestedPsqlExpression(joinClause.OuterKeySelector);
                var innerKeySelector = this.GetNestedPsqlExpression(joinClause.InnerKeySelector).Replace(tableName, $"temp_{tableName}");
                
                _psqlExpressionBuilder.Append(
                    $", (SELECT COUNT(*) FROM {tableName} AS \"temp_{tableName}\" " + 
                    $"WHERE {innerKeySelector} = {outerKeySelector}) " +
                    $"AS \"{itemType.Name}.__GROUP_COUNT\"");
            }

            else
            {
                var itemType = expression.ReferencedQuerySource.ItemType;
                var tableName = _queryModelVisitor.DbSchema.GetTableName(itemType.Name);

                var properties = itemType.GetPublicSettableProperties();
                var rowNames = properties.Select(x =>
                    $"\"{tableName}\".\"{_queryModelVisitor.DbSchema.GetColumnName(x.Name)}\" AS {x.Name}");

                _psqlExpressionBuilder.Append(string.Join(", ", rowNames));
            }

            return expression;
        }

        protected override Expression VisitSubQuery(SubQueryExpression expression)
        {
            _queryModelVisitor.QueryParts.OpenSubQuery();
            _queryModelVisitor.VisitQueryModel(expression.QueryModel);
            _queryModelVisitor.QueryParts.CloseSubQuery();
            return expression;
        }



        protected override Expression VisitBinary(BinaryExpression expression)
        {
            this.Visit(expression.Left);
    
            var isAddingStrings = expression.NodeType == ExpressionType.Add && 
                (expression.Left.Type == typeof(string)
                || expression.Right.Type == typeof(string));

            if (isAddingStrings)
            {
                _psqlExpressionBuilder.Append(" || ");
            }
            else
            {
                _psqlExpressionBuilder.Append(_binaryExpressionOperatorsToString[expression.NodeType]);
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
            if (_conditionalStart)
            {
                _psqlExpressionBuilder.Append("CASE");
                _conditionalStart = false;
            }

            _psqlExpressionBuilder.Append(" WHEN ");
            this.Visit(expression.Test);
            _psqlExpressionBuilder.Append(" THEN ");
            this.Visit(expression.IfTrue);

            if (expression.IfFalse.NodeType == ExpressionType.Conditional)
            {
                this.Visit(expression.IfFalse);
            }
            else // If constant, then that means the switch block has ended.
            {
                _psqlExpressionBuilder.Append(" ELSE ");
                this.Visit(expression.IfFalse);
                _psqlExpressionBuilder.Append(" END");
                _conditionalStart = true;
            }

            return expression;
        }
        
        protected override Expression VisitConstant(ConstantExpression expression)
        {
            var parameterName = _queryModelVisitor.ParameterAggregator.AddParameter(expression.Value);
            _psqlExpressionBuilder.Append($"@{parameterName}");
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
            Console.WriteLine("Hello, world!");
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
            if (expression.Expression.NodeType == ExpressionType.MemberAccess && expression.Member.Name == "Length")
            {
                var visitedExpression = this.GetNestedPsqlExpression(expression.Expression);
                _psqlExpressionBuilder.Append($"LENGTH({visitedExpression})");
            }
            else
            {
                _visitorTriggeredByMemberVisitor = true;
                this.Visit(expression.Expression);
                _visitorTriggeredByMemberVisitor = false;

                var columnName = _queryModelVisitor.DbSchema.GetColumnName(expression.Member.Name);
                _psqlExpressionBuilder.Append($".\"{columnName}\"");
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
                var expressionAccumulator = new List<object>(new object[] { _psqlExpressionBuilder.ToString() });
                _psqlExpressionBuilder.Clear();

                foreach (var argument in expression.Arguments)
                {
                    this.Visit(argument);
                    expressionAccumulator.Add(_psqlExpressionBuilder.ToString());
                    _psqlExpressionBuilder.Clear();
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
                        _psqlExpressionBuilder.AppendFormat(
                            _methodCallNamesToString[methodName],
                            string.Join(", ", expressionAccumulator.Select(x => x.ToString()))
                        );
                        break;

                    case "Substring":
                        if (expressionAccumulator.Count == 3)
                        {
                            _psqlExpressionBuilder.AppendFormat(
                                _methodCallNamesToString[methodName + "For"],
                                expressionAccumulator.ToArray()
                            );
                        }
                        else // if (expressionAccumulator.Count == 2)
                        {
                            _psqlExpressionBuilder.AppendFormat(
                                _methodCallNamesToString[methodName],
                                expressionAccumulator.ToArray()
                            );
                        }
                        break;

                    default:
                        _psqlExpressionBuilder.AppendFormat(
                            _methodCallNamesToString[methodName], 
                            expressionAccumulator.ToArray()
                        );
                        break;
                }

                return expression;
            }

            throw new NotImplementedException($"This LINQ provider does not provide the {methodName} method.");
        }

        protected override Expression VisitNew(NewExpression expression)
        {
            this.Visit(expression.Arguments[0]);

            if (expression.Members != null)
            {
                _psqlExpressionBuilder.Append($" AS {expression.Members[0].Name}");
            
                for (int i = 1; i < expression.Members.Count; i++)
                {
                    _psqlExpressionBuilder.Append(", ");

                    this.Visit(expression.Arguments[i]);
                    if (!(expression.Arguments[i] is QuerySourceReferenceExpression))
                    {
                        _psqlExpressionBuilder.Append($" AS {expression.Members[i].Name}");
                    }
                }
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
                var visitedExpression = this.GetNestedPsqlExpression(expression.Operand);
                _psqlExpressionBuilder.Append($"NOT ({visitedExpression})");
            }
            else
            {
                this.Visit(expression.Operand as MemberExpression);
            }
                
            return expression;
        }


        private string GetPsqlExpression()
        {
            return _psqlExpressionBuilder.ToString();
        }

        private string GetNestedPsqlExpression(Expression linqExpression)
        {
            var visitor = new PsqlGeneratingExpressionVisitor(_queryModelVisitor);
            visitor.Visit(linqExpression);
            return visitor.GetPsqlExpression();
        }
    }
}