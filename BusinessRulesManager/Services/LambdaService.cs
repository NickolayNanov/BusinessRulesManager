using BusinessRulesManager.Models;
using BusinessRulesManager.RulesEngine;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace BusinessRulesManager.Services
{
    public interface ILambdaService
    {
        string CreateLambda(BusinessRuleDefinition businessRuleDefinition);
    }

    public class LambdaService : ILambdaService
    {
        private readonly Dictionary<string, Func<object, bool>> cache = new();

        public string CreateLambda(BusinessRuleDefinition businessRuleDefinition)
        {
            var types =
                Assembly.GetExecutingAssembly().GetTypes()
                 .Where(mytype => mytype.GetInterfaces().Contains(typeof(IRulesEngineModel))).ToList();

            Expression<Func<object, bool>> lambda = CreateLambdaExpression<Account>(businessRuleDefinition.Conditions);

            var visitor = new ExpressionToStringVisitor();
            visitor.Visit(lambda);

            return visitor.GetStringRepresentation();
        }

        // should be used directly or idk
        public Func<object, bool> BuildRule<T>(BusinessRuleDefinition definition, string identifier)
        {
            string ruleKey = GetRuleKey(definition, identifier);

            if (cache.TryGetValue(ruleKey, out var func)) 
            {
                return func;
            }
            else
            {
                cache[ruleKey] = CompileRule<T>(definition.Conditions);
            }

            return cache[ruleKey];
        }

        private string GetRuleKey(BusinessRuleDefinition definition, string identifier)
        {
            return $"{identifier}-{definition.Id}-{definition.Name}-{definition.Conditions.Count}";
        }

        private Func<object, bool> CompileRule<T>(List<Condition> conditions)
        {
            Expression<Func<object, bool>> lambda = CreateLambdaExpression<T>(conditions);
            return lambda.Compile();
        }

        private Expression<Func<object, bool>> CreateLambdaExpression<T>(List<Condition> conditions)
        {
            var parameter = Expression.Parameter(typeof(object), "x");
            var castedParameter = Expression.Convert(parameter, typeof(T));
            Expression expr = null;
            LogicalOperator logcalOperator;
            foreach (var condition in conditions.OrderBy(x => x.Priority))
            {
                ValidateCondition<T>(condition); // Validate each rule

                Expression binaryExpr = BuildExpressionForCondition<T>(castedParameter, condition);
                var op = condition.LogicalOperator;
                
                if (conditions.IndexOf(condition) == 0)
                {
                    logcalOperator = condition.LogicalOperator;
                }
                else
                {
                    logcalOperator = conditions.Where(c => c.Priority < condition.Priority).OrderBy(x => x.Priority).Last().LogicalOperator;
                }

                if (condition.AdditionalConditions != null && condition.AdditionalConditions.Count != 0)
                {
                    List<Expression> additionalExpressions
                    = condition.AdditionalConditions.Select(cond => BuildExpressionForCondition<T>(castedParameter, cond)).ToList();

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

        private Expression BuildExpressionForCondition<T>(Expression parameter, Condition condition)
        {
            var member = Expression.Property(parameter, condition.PropertyName);
            var convertedValue = ConvertToType(condition.Value, member.Type);

            return condition.Operator switch
            {
                Operator.GreaterThan => Expression.GreaterThan(member, Expression.Constant(convertedValue)),
                Operator.GreaterThanOrEqualTo => Expression.GreaterThanOrEqual(member, Expression.Constant(convertedValue)),
                Operator.LessThan => Expression.LessThan(member, Expression.Constant(convertedValue)),
                Operator.LessThanOrEqualTo => Expression.LessThanOrEqual(member, Expression.Constant(convertedValue)),
                Operator.Equals => Expression.Equal(member, Expression.Constant(convertedValue)),
                Operator.In => BuildInExpression(member, condition),
                Operator.Between => BuildBetweenExpression(member, condition),
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

        private void ValidateCondition<T>(Condition condition)
        {
            if (string.IsNullOrEmpty(condition.PropertyName))
                throw new InvalidOperationException("Condition property name is required.");

            var propertyInfo
                = typeof(T).GetProperty(condition.PropertyName) ?? throw new InvalidOperationException($"Property {condition.PropertyName} not found in type {typeof(T).Name}.");
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
    }
}
