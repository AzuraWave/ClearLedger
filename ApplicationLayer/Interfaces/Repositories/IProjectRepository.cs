using ApplicationLayer.Interfaces.Repositories.Base;
using DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.Interfaces.Repositories
{
    public interface IProjectRepository : IRepository<Project>
    {

        Task<List<Project>> GetByClientAsync(Guid clientId, Guid orgId, bool includeArchived = false);

        Task<Project?> GetByIdAsync(Guid projectId, Guid orgId);

        Task ArchiveAsync(Guid projectId, Guid orgId, Guid updatedBy);

        Task<int> GetProjectTotalByOrg(Guid organizationId, bool includeArchived = false);

        Task<IQueryable<Project>> QueryAll(Guid organizationId);
    }
}
