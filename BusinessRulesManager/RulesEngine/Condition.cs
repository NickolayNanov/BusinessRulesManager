using BusinessRulesManager.Data;
using System.ComponentModel.DataAnnotations;

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

        public LogicalOperator LogicalOperator { get; set; }

        public string MinValue { get; set; }

        public string MaxValue { get; set; }

        [Required]
        public string DataType { get; set; }

        public string ValuesList { get; set; }

        public int? ParentConditionId { get; set; }

        public Condition ParentCondition { get; set; }

        public int IdBusinessRuleDefinition { get; set; }

        public BusinessRuleDefinition BusinessRuleDefinition { get; set; }

        [Required]
        public int Priority { get; set; }

        public List<Condition> AdditionalConditions { get; set; }
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
