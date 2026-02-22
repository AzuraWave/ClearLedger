using DomainLayer.Entities;
using ApplicationLayer.Interfaces.Repositories;
using InfrastructureLayer.Context;
using InfrastructureLayer.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using ApplicationLayer.DTOs.Organization;

namespace InfrastructureLayer.Repositories
{
    public class OrganizationRepository : Repository<Organization>, IOrganizationRepository
    {

        public OrganizationRepository(LedgerDbContext db) : base(db)
        {
        }

        public async Task AddApiKeyAsync(Guid organizationId ,string hashedApi)
        {
            var org = await _db.Organizations.FindAsync(organizationId);
            if (org == null)
                throw new Exception("Organization not found");

            org.ApiKeyHash = hashedApi;
        }

        public async Task<OrganizationReadDto?> GetOrganizationAsync(Guid organizationId)
        {
            return await _db.Organizations
                .Where(o => o.Id == organizationId)
                .Select(o => new OrganizationReadDto
                {
                    Id = o.Id,
                    Name = o.Name
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<decimal> GetOrganizationBalanceAsync(Guid organizationId)
        {
            return await _db.Clients
                .Where(c => c.OrganizationId == organizationId)
                .AsNoTracking()
                .SumAsync(c => c.Balance);
        }
    }
}
