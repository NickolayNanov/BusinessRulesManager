using BusinessRulesManager.Data;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace BusinessRulesManager.RulesEngine
{
    public class Condition : Entity<int>
    {
        [Required]
        public string PropertyName { get; set; }

        [Required]
        public Operator Operator { get; set; }

        [Required]
        public string Value { get; set; }

        [Required]
        public string DataType { get; set; }

        [Required]
        public int Priority { get; set; }

        public LogicalOperator LogicalOperator { get; set; }

        public string MinValue { get; set; }

        public string MaxValue { get; set; }

        public string ValuesList { get; set; }

        public int? ParentConditionId { get; set; }

        public Condition ParentCondition { get; set; }

        public int IdBusinessRuleDefinition { get; set; }

        public bool IsNested { get; set; }

        public List<Condition> NestedConditions { get; set; } = new List<Condition>();

        public BusinessRuleDefinition BusinessRuleDefinition { get; set; }

        public List<Condition> AdditionalConditions { get; set; } = new List<Condition>();
    }

    public enum Operator
    {
        NotEqualTo,
        Equals,
        GreaterThan,
        LessThan,
        GreaterThanOrEqualTo,
        LessThanOrEqualTo,
        In,
        Between
    }

    public enum LogicalOperator
    {
        OR,
        AND
    }
}
