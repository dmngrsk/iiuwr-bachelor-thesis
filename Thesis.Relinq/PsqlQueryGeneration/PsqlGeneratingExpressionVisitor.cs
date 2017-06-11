using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;
using Thesis.Relinq.Attributes;

namespace Thesis.Relinq.PsqlQueryGeneration
{
    /// Visits LINQ expressions and generates corresponding PostgreSQL query parts.
    public class PsqlGeneratingExpressionVisitor : RelinqExpressionVisitor
    {
        private readonly StringBuilder _psqlExpressionBuilder;
        private readonly PsqlGeneratingQueryModelVisitor _queryModelVisitor;
        private bool _visitorTriggeredByMemberVisitor;
        private bool _readingConditionalStatement;
        private bool _renamingColumns;

        /// Contains a map that translates binary expression types to equivalent PostgreSQL query parts.
        private static readonly Dictionary<ExpressionType, string> BinaryExpressionOperatorsToString = 
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

        /// Contains a map that translates a C# method name to a format string wrapping a PostgreSQL function and its parameters.
        private static readonly Dictionary<string, string> MethodCallNamesToString = 
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
            _readingConditionalStatement = false;
            _renamingColumns = false;
        }

        /// Creates an instance of the PsqlGeneratingExpressionVisitor based on provided PsqlGeneratingQueryModelVisitor instance to visit the LINQ expression provided as an argument and to generate a corresponding part of the PostgreSQL query.
        public static string GetPsqlExpression(Expression linqExpression,
            PsqlGeneratingQueryModelVisitor queryModelVisitor)
        {
            var visitor = new PsqlGeneratingExpressionVisitor(queryModelVisitor);
            visitor.Visit(linqExpression);
            return visitor.GetPsqlExpression();
        }



        protected override Expression VisitQuerySourceReference(QuerySourceReferenceExpression expression)
        {
            if (expression.ReferencedQuerySource is GroupJoinClause)
            {
                var itemType = expression.ReferencedQuerySource.ItemType.GetGenericArguments()[0];
                var tableName = itemType.GetTypeInfo().GetCustomAttribute<TableAttribute>().Name;

                var properties = itemType.GetPublicSettableProperties();
                var rowNames = properties.Select(x =>
                {
                    var columnName = x.GetCustomAttribute<ColumnAttribute>().Name;
                    return $"\"{tableName}\".\"{columnName}\" AS \"{itemType.Name}.{x.Name}\"";
                });

                _psqlExpressionBuilder.Append(string.Join(", ", rowNames));

                var joinClause = (expression.ReferencedQuerySource as GroupJoinClause).JoinClause;
                var outerKeySelector = this.GetNestedPsqlExpression(joinClause.OuterKeySelector);
                var innerKeySelector = this.GetNestedPsqlExpression(joinClause.InnerKeySelector).Replace(tableName, $"temp_{tableName}");
                
                _psqlExpressionBuilder.Append(
                    $", (SELECT COUNT(*) FROM \"{tableName}\" AS \"temp_{tableName}\" " + 
                    $"WHERE {innerKeySelector} = {outerKeySelector}) " +
                    $"AS \"{itemType.Name}.__GROUP_COUNT\"");
            }

            else if (_visitorTriggeredByMemberVisitor)
            {
                var tableName = expression.ReferencedQuerySource.ItemType.GetTypeInfo().GetCustomAttribute<TableAttribute>().Name;
                _psqlExpressionBuilder.Append($"\"{tableName}\"");
            }

            else
            {
                var itemType = expression.ReferencedQuerySource.ItemType;
                var tableName = itemType.GetTypeInfo().GetCustomAttribute<TableAttribute>().Name;

                var rowNames = itemType
                    .GetPublicSettableProperties()
                    .Select(x => $"\"{tableName}\".\"{x.GetCustomAttribute<ColumnAttribute>().Name}\" AS \"{x.Name}\"");

                _psqlExpressionBuilder.Append(string.Join(", ", rowNames));
            }

            return expression;
        }

        protected override Expression VisitSubQuery(SubQueryExpression expression)
        {
            _queryModelVisitor.QueryParts.OpenSubQueryExpressionPartsAggregator();
            _queryModelVisitor.VisitQueryModel(expression.QueryModel);
            _queryModelVisitor.QueryParts.CloseSubQueryExpressionPartsAggregator();
            return expression;
        }



        protected override Expression VisitBinary(BinaryExpression expression)
        {
            this.Visit(expression.Left);
    
            var isAddingStrings = expression.NodeType == ExpressionType.Add && 
                (expression.Left.Type == typeof(string)
                || expression.Right.Type == typeof(string));
                
            var middleDelimiter = isAddingStrings 
                ? " || " 
                : BinaryExpressionOperatorsToString[expression.NodeType];

            _psqlExpressionBuilder.Append(middleDelimiter);

            this.Visit(expression.Right);
            return expression;
        }
        
        protected override Expression VisitConditional(ConditionalExpression expression)
        {
            var renamingColumnsAccumulator = _renamingColumns;
            _renamingColumns = false;

            if (!_readingConditionalStatement)
            {
                _psqlExpressionBuilder.Append("CASE");
                _readingConditionalStatement = true;
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
                _readingConditionalStatement = false;
            }

            _renamingColumns = renamingColumnsAccumulator;
            return expression;
        }
        
        protected override Expression VisitConstant(ConstantExpression expression)
        {
            var parameterName = _queryModelVisitor.ParameterAggregator.AddParameter(expression.Value);
            _psqlExpressionBuilder.Append($"@{parameterName}");
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

                var columnName = expression.Member.GetCustomAttribute<ColumnAttribute>().Name;
                _psqlExpressionBuilder.Append($".\"{columnName}\"");

                if (_renamingColumns) _psqlExpressionBuilder.Append($" AS \"{expression.Member.Name}\"");
            }

            return expression;
        }
        
        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            var methodName = expression.Method.Name;

            if (MethodCallNamesToString.ContainsKey(methodName))
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

                var expectedArguments = Regex.Matches(MethodCallNamesToString[methodName], "\\{([^}]+)\\}");

                while (expressionAccumulator.Count < expectedArguments.Count) 
                {
                    expressionAccumulator.Add(string.Empty);
                }
                
                switch (methodName)
                {
                    case "Concat":
                        expressionAccumulator.RemoveAt(0);
                        _psqlExpressionBuilder.AppendFormat(
                            MethodCallNamesToString[methodName],
                            string.Join(", ", expressionAccumulator.Select(x => x.ToString())));
                        break;

                    case "Substring":
                        _psqlExpressionBuilder.AppendFormat(
                            expressionAccumulator.Count == 3
                                ? MethodCallNamesToString[methodName + "For"]
                                : MethodCallNamesToString[methodName],
                            expressionAccumulator.ToArray());
                        break;

                    default:
                        _psqlExpressionBuilder.AppendFormat(
                            MethodCallNamesToString[methodName], 
                            expressionAccumulator.ToArray());
                        break;
                }

                return expression;
            }

            throw new NotImplementedException($"This LINQ provider does not provide the {methodName} method.");
        }

        protected override Expression VisitNew(NewExpression expression)
        {
            _renamingColumns = true;
            this.Visit(expression.Arguments[0]);

            for (int i = 1; i < expression.Arguments.Count; i++)
            {
                _psqlExpressionBuilder.Append(", ");
                this.Visit(expression.Arguments[i]);
            }

            _renamingColumns = false;
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

        /// Returns the part of a PostgreSQL query that was generated using visitor methods. 
        private string GetPsqlExpression()
        {
            return _psqlExpressionBuilder.ToString();
        }

        /// Visits a LINQ expression using a new expression visitor and returns a part of the PostgreSQL query it represents.
        private string GetNestedPsqlExpression(Expression linqExpression)
        {
            var visitor = new PsqlGeneratingExpressionVisitor(_queryModelVisitor);
            visitor.Visit(linqExpression);
            return visitor.GetPsqlExpression();
        }
    }
}