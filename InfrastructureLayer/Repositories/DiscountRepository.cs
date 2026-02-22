using ApplicationLayer.Interfaces.Repositories;
using DomainLayer.Entities;
using InfrastructureLayer.Context;
using InfrastructureLayer.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfrastructureLayer.Repositories
{
    public class DiscountRepository : Repository<Discount>, IDiscountRepository
    {
        public DiscountRepository(LedgerDbContext db) : base(db)
        {
        }

        public Task<List<Discount>> GetByClientAsync(Guid clientId, Guid organizationId)
        {
            throw new NotImplementedException();
        }

        public async Task<Discount?> GetByIdAsync(Guid id, Guid organizationId)
        {
            return await _db.Discounts
                            .FirstOrDefaultAsync(d => d.Id == id && d.OrganizationId == organizationId);
        }

        public Task<List<Discount>> GetByProjectAsync(Guid projectId, Guid organizationId)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Discount> QueryAll(Guid organizationId)
        => _db.Discounts
                   .Where(d => d.OrganizationId == organizationId)
                   .AsQueryable();

        public IQueryable<Discount> QueryByClient(Guid organizationId, Guid clientId)
        => _db.Discounts
                   .Where(d => d.OrganizationId == organizationId && d.ClientId == clientId)
                   .AsQueryable();

        public IQueryable<Discount> QueryByProject(Guid organizationId, Guid projectId)
        => _db.Discounts
                   .Where(d => d.OrganizationId == organizationId && d.ProjectId == projectId)
                   .AsQueryable();
    }
}
