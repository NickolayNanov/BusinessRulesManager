using BusinessRulesManager.RulesEngine;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BusinessRulesManager.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<BusinessRuleDefinition> BusinessRuleDefinitions { get; set; }

        public DbSet<Condition> Conditions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<BusinessRuleDefinition>()
                .Navigation(x => x.Conditions)
                .AutoInclude();

            builder.Entity<Condition>()
                .HasOne(x => x.ParentCondition)
                    .WithMany(x => x.AdditionalConditions)
                    .HasForeignKey(x => x.ParentConditionId);

            base.OnModelCreating(builder);
        }
    }
}
