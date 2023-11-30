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

            //builder.Entity<Condition>()
            //    .Navigation(x => x.AdditionalConditions)
            //    .AutoInclude();

            base.OnModelCreating(builder);
        }
    }
}
