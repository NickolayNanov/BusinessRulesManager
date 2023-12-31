﻿using BusinessRulesManager.Models;
using BusinessRulesManager.RulesEngine;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace BusinessRulesManager.Services
{
    public interface ILambdaService
    {
        string CreateLambda(BusinessRuleDefinition businessRuleDefinition);

        Task<Func<object, bool>> BuildRuleAsync(BusinessRuleDefinition definition, string identifier, Type type);

        Task<string> CreateLambdaExpressionAsString(BusinessRuleDefinition businessRuleDefinition);
    }

    public class LambdaService : ILambdaService
    {
        private readonly Dictionary<string, Func<object, bool>> cache = new();

        public string CreateLambda(BusinessRuleDefinition businessRuleDefinition)
        {
            var types =
                Assembly.GetExecutingAssembly().GetTypes()
                 .Where(mytype => mytype.GetInterfaces().Contains(typeof(IRulesEngineModel))).ToList();

            Expression<Func<object, bool>> lambda = CreateLambdaExpression(businessRuleDefinition.Conditions, typeof(Account));

            var visitor = new ExpressionToStringVisitor();
            visitor.Visit(lambda);

            return visitor.GetStringRepresentation();
        }

        // should be used directly or idk
        public async Task<Func<object, bool>> BuildRuleAsync(BusinessRuleDefinition definition, string identifier, Type type)
        {
            return await Task.Run(() =>
            {
                string ruleKey = GetRuleKey(definition, identifier);

                if (cache.TryGetValue(ruleKey, out var func))
                {
                    return func;
                }
                else
                {
                    cache[ruleKey] = CompileRule(definition.Conditions, type);
                }

                return cache[ruleKey];
            });
        }

        private string GetRuleKey(BusinessRuleDefinition definition, string identifier)
        {
            return $"{identifier}-{definition.Id}-{definition.Name}-{definition.Conditions.Count}";
        }

        private Func<object, bool> CompileRule(List<Condition> conditions, Type type)
        {
            Expression<Func<object, bool>> lambda = CreateLambdaExpression(conditions, type);
            return lambda.Compile();
        }

        private Expression<Func<object, bool>> CreateLambdaExpression(List<Condition> conditions, Type objectType)
        {
            int initialParameterCounter = 1;

            var parameter = Expression.Parameter(typeof(object), $"x{initialParameterCounter}");
            var castedParameter = Expression.Convert(parameter, objectType);
            Expression expr = null;
            LogicalOperator logcalOperator;

            foreach (var condition in conditions.OrderBy(x => x.Priority))
            {
                ValidateCondition(condition, objectType); // Validate each rule

                string propertyName = condition.PropertyName;
                Type propertyType = objectType.GetProperty(propertyName).PropertyType;
                bool isClass = propertyType.IsClass;
                bool isCollection = propertyType.IsAssignableTo(typeof(IEnumerable<>));

                Expression binaryExpr = null;

                if (isClass)
                {
                    var newParameter = Expression.Parameter(typeof(object), $"x{++initialParameterCounter}");
                    var newCastedParameter = Expression.Convert(newParameter, propertyType);
                    binaryExpr = BuildExpressionForCondition(newCastedParameter, condition);
                }
                else if (isCollection)
                {

                }
                else
                {
                    binaryExpr = BuildExpressionForCondition(castedParameter, condition);
                }

                var op = condition.LogicalOperator;

                if (conditions.IndexOf(condition) == 0)
                {
                    logcalOperator = condition.LogicalOperator;
                }
                else
                {
                    logcalOperator = conditions.Where(c => c.Priority < condition.Priority).OrderBy(x => x.Priority).Last().LogicalOperator;
                }

                if (condition.AdditionalConditions != null && condition.AdditionalConditions.Count != 0 && !isClass)
                {
                    List<Expression> additionalExpressions
                    = condition.AdditionalConditions.Select(cond => BuildExpressionForCondition(castedParameter, cond)).ToList();

                    foreach (var additionalExpression in additionalExpressions)
                    {
                        binaryExpr = Expression.AndAlso(binaryExpr, additionalExpression);
                    }
                }

                if (expr == null)
                {
                    expr = binaryExpr;
                }
                else
                {
                    expr = logcalOperator == LogicalOperator.AND ?
                           Expression.AndAlso(expr, binaryExpr) :
                           Expression.OrElse(expr, binaryExpr);
                }
            }

            Expression<Func<object, bool>> lambda = Expression.Lambda<Func<object, bool>>(expr, parameter);
            return lambda;
        }

        private Expression BuildExpressionForCondition(Expression parameter, Condition condition)
        {
            MemberExpression member;
            object convertedValue;

            if (condition.AdditionalConditions is not null && condition.AdditionalConditions.Any())
            {
                member = Expression.Property(parameter, condition?.AdditionalConditions[0].PropertyName);
                convertedValue = ConvertToType(condition?.AdditionalConditions[0].Value, member.Type);
            }
            else
            {
                member = Expression.Property(parameter, condition.PropertyName);
                convertedValue = ConvertToType(condition.Value, member.Type);
            }

            return condition.Operator switch
            {
                Operator.GreaterThan => Expression.GreaterThan(member, Expression.Constant(convertedValue)),
                Operator.GreaterThanOrEqualTo => Expression.GreaterThanOrEqual(member, Expression.Constant(convertedValue)),
                Operator.LessThan => Expression.LessThan(member, Expression.Constant(convertedValue)),
                Operator.LessThanOrEqualTo => Expression.LessThanOrEqual(member, Expression.Constant(convertedValue)),
                Operator.Equals => Expression.Equal(member, Expression.Constant(convertedValue)),
                Operator.In => BuildInExpression(member, condition),
                Operator.Between => BuildBetweenExpression(member, condition),
                Operator.NotEqualTo => Expression.NotEqual(member, Expression.Constant(convertedValue)),
                _ => null,
            };
        }

        private BinaryExpression BuildInExpression(MemberExpression member, Condition condition)
        {
            var values = condition.ValuesList.Split(", ").ToList().ConvertAll(value => ConvertToType(value, member.Type));

            var equalsExpressions = values.Select(value => Expression.Equal(member, Expression.Constant(value)));
            return equalsExpressions.Aggregate((current, next) => Expression.OrElse(current, next));
        }

        private BinaryExpression BuildBetweenExpression(MemberExpression member, Condition condition)
        {
            object convertedMinValue = ConvertToType(condition.MinValue, member.Type);
            object ConvertedMaxValue = ConvertToType(condition.MaxValue, member.Type);

            Expression left = null, right = null;

            if (convertedMinValue != null)
                left = Expression.GreaterThanOrEqual(member, Expression.Constant(convertedMinValue));

            if (ConvertedMaxValue != null)
                right = Expression.LessThanOrEqual(member, Expression.Constant(ConvertedMaxValue));

            if (left != null && right != null)
                return Expression.AndAlso(left, right);

            var greaterThanMin = Expression.GreaterThanOrEqual(member, Expression.Constant(convertedMinValue));
            var lessThanMax = Expression.LessThanOrEqual(member, Expression.Constant(ConvertedMaxValue));

            return Expression.AndAlso(greaterThanMin, lessThanMax);
        }

        private void ValidateCondition(Condition condition, Type type)
        {
            if (string.IsNullOrEmpty(condition.PropertyName))
                throw new InvalidOperationException("Condition property name is required.");

            var propertyInfo
                = type.GetProperty(condition.PropertyName) ?? throw new InvalidOperationException($"Property {condition.PropertyName} not found in type {type.Name}.");
        }

        private object ConvertToType(string value, Type targetType)
        {
            try
            {
                if (string.IsNullOrEmpty(value))
                    return null;

                if (targetType == typeof(DateTime))
                    return DateTime.Parse(value);

                if (targetType.IsEnum)
                    return Enum.Parse(targetType, value);

                return Convert.ChangeType(value, targetType);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error converting value: {value} to type: {targetType.ToString()}", ex);
            }
        }

        public async Task<string> CreateLambdaExpressionAsString(BusinessRuleDefinition businessRuleDefinition)
        {
            return await Task.Run(() =>
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append("x => ");

                for (int i = 0; i < businessRuleDefinition.Conditions.Count; i++)
                {
                    Condition condition = businessRuleDefinition.Conditions[i];

                    string expressionStr = $"({GenerateConditionString(condition, businessRuleDefinition.ObjectType)})";
                    string logicalOperator = condition.LogicalOperator == LogicalOperator.AND ? " AND " : " OR ";

                    if (i < businessRuleDefinition.Conditions.Count - 1)
                    {
                        expressionStr += logicalOperator;
                    }

                    stringBuilder.Append(expressionStr);
                }

                return stringBuilder.ToString();
            });
        }

        private string GenerateConditionString(Condition condition, string objectType)
        {
            StringBuilder localSB = new();

            bool additionalConditionsExist = condition.AdditionalConditions is not null && condition.AdditionalConditions.Count > 0;

            var propertyType = Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(x => x.Name == condition.DataType);

            bool isClass = propertyType.IsClass;

            if (isClass && additionalConditionsExist)
            {
                foreach (var additionalCondition in condition.AdditionalConditions)
                {
                    var str = $"x.{condition.PropertyName}.{additionalCondition.PropertyName} {GetOperatorString(additionalCondition.Operator)} {FormatValue(additionalCondition.Value, additionalCondition.DataType)}";
                    localSB.Append(str);
                    return localSB.ToString();
                }
            }
            else
            {
                AppendSimplePropertyExpression(condition, localSB);
            }

            if (additionalConditionsExist)
            {
                foreach (var additionalCondition in condition.AdditionalConditions)
                {
                    bool isLast = condition.AdditionalConditions.IndexOf(additionalCondition) == condition.AdditionalConditions.Count - 1;
                    AppendSimpleAdditionalConditionExpression(isLast, localSB, additionalCondition, objectType);
                }
            }

            string returnString = localSB.ToString();
            localSB.Clear(); // mby delete?
            return returnString;
        }

        private void AppendSimpleAdditionalConditionExpression(bool isLast, StringBuilder localSB, Condition additionalCondition, string objectType)
        {
            localSB.Append(" AND ");
            string additionalConditionStr = GenerateConditionString(additionalCondition, objectType);
            localSB.Append(additionalConditionStr);

            if (!isLast)
            {
                localSB.Append(" AND ");
            }
        }

        private void AppendSimplePropertyExpression(Condition condition, StringBuilder localSB)
        {
            localSB.Append('(');

            string currentExpressionString = $"x.{condition.PropertyName} {GetOperatorString(condition.Operator)} {FormatValue(condition.Value, condition.DataType)}";
            localSB.Append(currentExpressionString);

            localSB.Append(')');
        }

        private string GetOperatorString(Operator op)
        {
            return op switch
            {
                Operator.Equals => "==",
                Operator.NotEqualTo => "!=",
                Operator.GreaterThan => ">",
                Operator.LessThan => "<",
                Operator.GreaterThanOrEqualTo => ">=",
                Operator.LessThanOrEqualTo => "<=",
                // Additional handling may be needed for 'In' and 'Between' operators
                _ => throw new NotImplementedException($"Operator {op} is not implemented."),
            };
        }

        private string FormatValue(string value, string dataType)
        {
            // Logic to format value based on dataType
            // Example: Wrap string values in quotes
            return dataType == "string" ? $"\"{value}\"" : value;
        }
    }
}
