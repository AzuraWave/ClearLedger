using ApplicationLayer.Interfaces.Repositories.Base;
using DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.Interfaces.Repositories
{
    public interface IDiscountRepository : IRepository<Discount>
    {
        Task<List<Discount>> GetByProjectAsync(Guid projectId, Guid organizationId);
        Task<List<Discount>> GetByClientAsync(Guid clientId, Guid organizationId);

        Task<Discount?> GetByIdAsync(Guid id, Guid organizationId);
        IQueryable<Discount> QueryByClient(Guid organizationId, Guid clientId);
        IQueryable<Discount> QueryByProject(Guid organizationId, Guid projectId);
        IQueryable<Discount> QueryAll(Guid organizationId);
    }
}
