using ApplicationLayer.Interfaces.Repositories;
using ApplicationLayer.Interfaces.Repositories.Base;
using DomainLayer.Entities;
using InfrastructureLayer.Context;
using InfrastructureLayer.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfrastructureLayer.Repositories
{
    public class PaymentRepository : Repository<ClientPaymentHeader>, IPaymentRepository
    {
        public PaymentRepository(LedgerDbContext db) : base(db)
        {
        }

        public async Task<List<ClientPaymentHeader>> GetByClientAsync(Guid clientId, Guid organizationId)
        {
            return await _db.ClientPaymentHeaders
                .Include(c => c.Allocations)
                .Where(c => c.ClientId == clientId && c.OrganizationId == organizationId)
                .OrderByDescending(c => c.Date)
                .ToListAsync();
        }

        public async Task<ClientPaymentHeader?> GetByIdAsync(Guid id, Guid organizationId)
        {
            return await _db.ClientPaymentHeaders
            .Include(c => c.Allocations)
            .FirstOrDefaultAsync(c => c.Id == id && c.OrganizationId == organizationId);
        }

        public async Task<ClientPaymentHeader> GetByIdAsync(Guid id, Guid organizationId, bool includeAllocations)
        {
            IQueryable<ClientPaymentHeader> query = _db.ClientPaymentHeaders;

            if (includeAllocations)
            {
                query = query.Include(p => p.Allocations);
            }

            var entity = await query.FirstOrDefaultAsync(p => p.Id == id && p.OrganizationId == organizationId);

            if (entity == null)
            {
                throw new KeyNotFoundException($"ClientPaymentHeader with Id '{id}' not found for Organization '{organizationId}'.");
            }

            return entity;
        }

        public IQueryable<ClientPaymentHeader> QueryAll(Guid organizationId)
        => _db.ClientPaymentHeaders
                   .Include(p => p.Allocations)
                   .Where(p => p.OrganizationId == organizationId)
                   .AsQueryable();

        public IQueryable<ClientPaymentHeader> QueryByClient(Guid organizationId, Guid clientId)
        => _db.ClientPaymentHeaders
                   .Include(p => p.Allocations)
                   .Where(p => p.OrganizationId == organizationId && p.ClientId == clientId)
                   .AsQueryable();

        public IQueryable<ClientPaymentHeader> QueryByProject(Guid organizationId, Guid projectId)
        {
            return _db.ClientPaymentHeaders
                           .Include(p => p.Allocations)
                           .Where(p => p.OrganizationId == organizationId
                                       && p.Allocations.Any(a => a.ProjectId == projectId))
                           .AsQueryable();
        }


    }
}
