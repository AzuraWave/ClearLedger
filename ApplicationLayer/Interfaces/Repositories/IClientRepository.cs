using ApplicationLayer.Interfaces.Repositories.Base;
using DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.Interfaces.Repositories
{
    public interface IClientRepository : IRepository<Client>
    {

        Task<Client> GetClientByProject(Guid projectId, Guid organizationId, bool includeArchived = false);
        Task<IReadOnlyList<Client>> GetByOrganizationAsync(Guid organizationId, bool includeArchived = false);
        Task<Client?> GetByIdAsync(Guid clientId, Guid organizationId, bool includeArchived = false);
        Task ArchiveAsync(Client client);
        Task DeleteClientUserAsync(Guid clientId);
        Task ArchiveClientUserAsync(Guid clientId);

        Task<IQueryable<Client>> QueryAll(Guid organizationId);

    }
}
