using ApplicationLayer.Interfaces.Repositories.Base;
using DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.Interfaces.Repositories
{
    public interface IPaymentRepository : IRepository<ClientPaymentHeader>
    {
        Task<ClientPaymentHeader?> GetByIdAsync(Guid id, Guid organizationId);
        Task<List<ClientPaymentHeader>> GetByClientAsync(Guid clientId, Guid organizationId);

        Task<ClientPaymentHeader> GetByIdAsync( Guid id, Guid organizationId, bool includeAllocations);
        IQueryable<ClientPaymentHeader> QueryByClient(Guid organizationId, Guid clientId);
        IQueryable<ClientPaymentHeader> QueryByProject(Guid organizationId, Guid projectId);
        IQueryable<ClientPaymentHeader> QueryAll(Guid organizationId);
    }
}
