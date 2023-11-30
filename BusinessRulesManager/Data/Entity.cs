using BusinessRulesManager.Data.Contracts;
using System.ComponentModel.DataAnnotations;

namespace BusinessRulesManager.Data
{
    public class Entity<TKey> : IEntity<TKey>
    {
        [Key]
        public TKey Id { get; set; }
    }
}