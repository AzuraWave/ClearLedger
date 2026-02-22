using ApplicationLayer.Interfaces.Repositories.Base;
using DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.Interfaces.Repositories
{
    public interface IInvoiceRepository : IRepository<Invoice>
    {
        Task<List<Invoice>> GetByClientAsync(Guid clientId, Guid organizationId);
        Task<List<Invoice>> GetByProjectAsync(Guid projectId, Guid organizationId);

        Task<Invoice?> GetByIdAsync(Guid invoiceId, Guid organizationId);

        Task<string> GetNextInvoiceNumberAsync( Guid organizationId, Guid clientId);
        IQueryable<Invoice> QueryByClient(Guid organizationId, Guid clientId);
        IQueryable<Invoice> QueryByProject(Guid organizationId, Guid projectId);
        IQueryable<Invoice> QueryAll(Guid organizationId);

        Task<decimal> GetOrgInvoicesForCurrentMonth(Guid organizationId);

        
    }
}
