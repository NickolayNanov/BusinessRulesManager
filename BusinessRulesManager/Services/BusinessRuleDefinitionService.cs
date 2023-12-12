using AutoMapper;
using BusinessRulesManager.Data;
using BusinessRulesManager.RulesEngine;
using Microsoft.EntityFrameworkCore;

namespace BusinessRulesManager.Services
{
    public class BusinessRuleDefinitionService : BasicCrudService<BusinessRuleDefinition, int>, IBusinessRuleDefinitionService
    {
        public BusinessRuleDefinitionService(IMapper mapper, ApplicationDbContext context)
            : base(mapper, context)
        {
        }

        public override async Task<BusinessRuleDefinition> GetByIdAsync(int id)
        {
            return await context.BusinessRuleDefinitions
                .Include(x => x.Conditions)
                .ThenInclude(x => x.AdditionalConditions)
                .ThenInclude(x => x.AdditionalConditions)
                .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
