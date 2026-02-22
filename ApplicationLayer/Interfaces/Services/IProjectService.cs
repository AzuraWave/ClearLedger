using ApplicationLayer.DTOs.Projects;
using DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.Interfaces.Services
{
    public interface IProjectService
    {
        Task<Guid> CreateProjectAsync(ProjectCreateDto dto, Guid createdByUserId);
        Task<ProjectReadDto?> GetProjectAsync(Guid projectId);
        Task<IEnumerable<ProjectReadDto>> GetProjectsByClientAsync(Guid clientId, Guid orgId, bool isArchived = false);
        Task UpdateProjectAsync(ProjectUpdateDto dto, Guid updatedByUserId);
        Task ArchiveProjectAsync(Guid projectId, Guid updatedByUserId);

        Task <int> GetProjectsTotal(Guid organizationId, bool getArchived = false);






    }

}
