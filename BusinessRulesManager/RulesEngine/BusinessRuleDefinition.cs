using BusinessRulesManager.Data;
using System.ComponentModel.DataAnnotations;

namespace BusinessRulesManager.RulesEngine
{
    public class BusinessRuleDefinition : Entity<int>
    {
        [Required]
        [MinLength(5)]
        public string Description { get; set; }

        [Required]
        [MinLength(3)]
        public string Name { get; set; }

        [Required]
        public string ObjectType { get; set; }

        public List<Condition> Conditions { get; set; }
    }
}
