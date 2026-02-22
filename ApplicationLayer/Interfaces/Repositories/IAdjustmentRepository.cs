using ApplicationLayer.Interfaces.Repositories.Base;
using DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.Interfaces.Repositories
{
    public interface IAdjustmentRepository : IRepository<Adjustment>
    {
        Task<List<Adjustment>> GetByProjectAsync(Guid projectId, Guid organizationId);
        Task<List<Adjustment>> GetByClientAsync(Guid clientId, Guid organizationId);

        Task<Adjustment?> GetByIdAsync(Guid id, Guid organizationId);
        IQueryable<Adjustment> QueryByClient(Guid organizationId, Guid clientId);
        IQueryable<Adjustment> QueryByProject(Guid organizationId, Guid projectId);
        IQueryable<Adjustment> QueryAll(Guid organizationId);
    }
}
