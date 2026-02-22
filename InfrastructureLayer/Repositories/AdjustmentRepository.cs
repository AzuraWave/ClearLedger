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
    public class AdjustmentRepository : Repository<Adjustment>, IAdjustmentRepository
    {
        public AdjustmentRepository(LedgerDbContext db) : base(db)
        {
        }

        public Task<List<Adjustment>> GetByClientAsync(Guid clientId, Guid organizationId)
        {
            throw new NotImplementedException();
        }

        public async Task<Adjustment?> GetByIdAsync(Guid id, Guid organizationId)
        {
           return await _db.Adjustments.AsNoTracking()
                   .FirstOrDefaultAsync(a => a.Id == id && a.OrganizationId == organizationId);
        }

        public Task<List<Adjustment>> GetByProjectAsync(Guid projectId, Guid organizationId)
        {
            throw new NotImplementedException();
        }

        public IQueryable<Adjustment> QueryAll(Guid organizationId)
        => _db.Adjustments
                   .Where(a => a.OrganizationId == organizationId)
                   .AsQueryable();

        public IQueryable<Adjustment> QueryByClient(Guid organizationId, Guid clientId)
        => _db.Adjustments
                   .Where(a => a.OrganizationId == organizationId && a.ClientId == clientId)
                   .AsQueryable();

        public IQueryable<Adjustment> QueryByProject(Guid organizationId, Guid projectId)
        => _db.Adjustments
                   .Where(a => a.OrganizationId == organizationId && a.ProjectId == projectId)
                   .AsQueryable();
    }
}
