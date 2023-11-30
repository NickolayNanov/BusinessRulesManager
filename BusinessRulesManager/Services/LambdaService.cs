using BusinessRulesManager.RulesEngine;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace BusinessRulesManager.Services
{
    public interface ILambdaService
    {
        string CreateLambda(BusinessRuleDefinition businessRuleDefinition);
    }

    public class LambdaService : ILambdaService
    {
        private readonly ConcurrentDictionary<string, Func<object, bool>> cache = new ConcurrentDictionary<string, Func<object, bool>>();

        public string CreateLambda(BusinessRuleDefinition businessRuleDefinition)
        {
            Expression<Func<object, bool>> lambda = CreateLambdaExpression<Account>(businessRuleDefinition.Conditions);

            var visitor = new ExpressionToStringVisitor();
            visitor.Visit(lambda);

            return visitor.GetStringRepresentation();
        }

        // should be used directly or idk
        public Func<object, bool> BuildRule<T>(List<Condition> conditions)
        {
            string ruleKey = GetRuleKey(conditions);

            return cache.GetOrAdd(ruleKey, key => CompileRule<T>(conditions));
        }

        private string GetRuleKey(List<Condition> conditions)
        {
            // Concatenate rule properties to form a unique key
            return string.Join(";", conditions.Select(r => $"{r.PropertyName}-{r.Operator}-{r.Id}"));
        }

        private Func<object, bool> CompileRule<T>(List<Condition> conditions)
        {
            Expression<Func<object, bool>> lambda = CreateLambdaExpression<T>(conditions);
            return lambda.Compile();
        }

        private Expression<Func<object, bool>> CreateLambdaExpression<T>(List<Condition> conditions)
        {
            var parameter = Expression.Parameter(typeof(object), "x");
            UnaryExpression castedParameter = Expression.Convert(parameter, typeof(T));
            Expression expr = null;

            foreach (var condition in conditions)
            {
                ValidateCondition<T>(condition); // Validate each rule

                Expression binaryExpr = BuildExpressionForCondition<T>(castedParameter, condition);

                if (condition.AdditionalConditions != null && condition.AdditionalConditions.Any())
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
                    expr = condition.LogicalOperator == LogicalOperator.AND ?
                           Expression.AndAlso(expr, binaryExpr) :
                           Expression.OrElse(expr, binaryExpr);
                }
            }

            Expression<Func<object, bool>> lambda = Expression.Lambda<Func<object, bool>>(expr, parameter);
            return lambda;
        }

        private Expression BuildExpressionForCondition<T>(Expression parameter, Condition rule)
        {
            var member = Expression.Property(parameter, rule.PropertyName);
            var targetType = member.Type;

            object value = ConvertToType(rule.Value, targetType);
            object minValue = ConvertToType(rule.MinValue, targetType);
            object maxValue = ConvertToType(rule.MaxValue, targetType);

            Expression left = null, right = null;

            if (minValue != null)
                left = Expression.GreaterThanOrEqual(member, Expression.Constant(minValue));

            if (maxValue != null)
                right = Expression.LessThanOrEqual(member, Expression.Constant(maxValue));

            if (left != null && right != null)
                return Expression.AndAlso(left, right);

            switch (rule.Operator)
            {
                case Operator.GreaterThan:
                    return Expression.GreaterThan(member, Expression.Constant(value));
                case Operator.GreaterThanOrEqualTo:
                    return Expression.GreaterThanOrEqual(member, Expression.Constant(value));
                case Operator.LessThan:
                    return Expression.LessThan(member, Expression.Constant(value));
                case Operator.LessThanOrEqualTo:
                    return Expression.LessThanOrEqual(member, Expression.Constant(value));
                case Operator.Equals:
                    return Expression.Equal(member, Expression.Constant(value));
                case Operator.In:
                    var values = rule.ValuesList.Split(", ").ToList().ConvertAll(value => ConvertToType(value, targetType));
                    return BuildInExpression(member, values);
                case Operator.Between:
                    return BuildBetweenExpression(member, minValue, maxValue);
            }

            return left ?? right;
        }

        private Expression BuildInExpression(MemberExpression member, List<object> values)
        {
            var equalsExpressions = values.Select(value => Expression.Equal(member, Expression.Constant(value)));
            return equalsExpressions.Aggregate((current, next) => Expression.OrElse(current, next));
        }

        private Expression BuildBetweenExpression(MemberExpression member, object minValue, object maxValue)
        {
            var greaterThanMin = Expression.GreaterThanOrEqual(member, Expression.Constant(minValue));
            var lessThanMax = Expression.LessThanOrEqual(member, Expression.Constant(maxValue));
            return Expression.AndAlso(greaterThanMin, lessThanMax);
        }

        private void ValidateCondition<T>(Condition rule)
        {
            if (string.IsNullOrEmpty(rule.PropertyName))
                throw new InvalidOperationException("Rule property name is required.");

            var propertyInfo = typeof(T).GetProperty(rule.PropertyName);
            if (propertyInfo == null)
                throw new InvalidOperationException($"Property {rule.PropertyName} not found in type {typeof(T).Name}.");
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
                throw new InvalidOperationException("Error converting value", ex);
            }
        }
    }

    public class Account
    {
        public decimal Balance { get; set; }
        public DateTime CreationDate { get; set; }
        public bool IsActive { get; set; }
        public AccountType AccountType { get; set; }
        public int TransactionCount { get; set; }
        public int CreditScore { get; set; }
        public decimal LastTransactionAmount { get; set; }
        public int AccountAgeInYears { get; set; }
        public int NumberOfOverdrafts { get; set; }
        public decimal AverageMonthlyDeposit { get; set; }
    }


    public enum AccountType
    {
        Savings,
        Checking
    }

}
