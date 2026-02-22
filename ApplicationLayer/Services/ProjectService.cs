using ApplicationLayer.DTOs.Projects;
using ApplicationLayer.Interfaces.Patterns;
using ApplicationLayer.Interfaces.Repositories;
using ApplicationLayer.Interfaces.Services;
using DomainLayer.Entities;
using DomainLayer.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository repo;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<ProjectService> _logger;
        public ProjectService(IProjectRepository repo, IUnitOfWork unitOfWork, ILogger<ProjectService> logger)
        {
            this.repo = repo;
            this.unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task ArchiveProjectAsync(Guid projectId, Guid updatedByUserId)
        {
            var project = await repo.GetByIdAsync(projectId)
            ?? throw new Exception("Project not found");

            project.Status = ProjectStatus.Archived;
            project.UpdatedAt = DateTime.UtcNow;
            project.UpdatedBy = updatedByUserId;

            await repo.UpdateAsync(project);
            await unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Project archived: {ProjectId}, UpdatedBy: {UserId}, Time: {Time}",
                projectId, updatedByUserId, DateTime.UtcNow);
        }

        public async Task<Guid> CreateProjectAsync(ProjectCreateDto dto, Guid createdByUserId)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Project name is required");

            var project = new Project
            {
                Id = Guid.NewGuid(),
                ClientId = dto.clientId,
                OrganizationId = dto.organizationId,
                Name = dto.Name,
                Description = dto.Description,
                CreatedBy = createdByUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = createdByUserId,
                Status = ProjectStatus.Active
            };

            await repo.AddAsync(project);
            await unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Project created: {ProjectId}, ClientId: {ClientId}, OrgId: {OrgId}, CreatedBy: {UserId}, Time: {Time}",
                project.Id, dto.clientId, dto.organizationId, createdByUserId, DateTime.UtcNow);


            return project.Id;
        }

        public async Task<ProjectReadDto?> GetProjectAsync(Guid projectId)
        {
            var project = await repo.GetByIdAsync(projectId);
            if (project == null)
            {
                _logger.LogDebug("GetProjectAsync: Project not found: {ProjectId}", projectId);
                return null;
            }

            var dto = new ProjectReadDto
            {
                Id = project.Id,
                Name = project.Name ?? string.Empty,
                clientId = project.ClientId,
                organizationId = project.OrganizationId,
                Description = project.Description,
                ProjectStatus = project.Status,
                Balance = project.Balance
               
            };

            return dto;
        }

        public async Task<IEnumerable<ProjectReadDto>> GetProjectsByClientAsync(Guid clientId, Guid orgId, bool isArchived = false)
        {
            var projects = await repo.GetByClientAsync(clientId, orgId, isArchived);
            var projectDtos = new List<ProjectReadDto>();
            foreach (var project in projects)
            {
                var dto = new ProjectReadDto
                {
                    Id = project.Id,
                    Name = project.Name ?? string.Empty,
                    Description = project.Description,
                    clientId = project.ClientId,
                    organizationId = project.OrganizationId,
                    ProjectStatus = project.Status,
                    Balance = project.Balance
                };
                projectDtos.Add(dto);
            }
            _logger.LogDebug("Retrieved {Count} projects for ClientId: {ClientId}, OrgId: {OrgId}", projectDtos.Count, clientId, orgId);

            return projectDtos;
        }

        public Task<int> GetProjectsTotal(Guid organizationId, bool getArchived = false)
        {
            var num = repo.GetProjectTotalByOrg(organizationId, getArchived);
            _logger.LogDebug("Total projects for OrgId: {OrgId}, IncludeArchived: {Archived}: {Total}", organizationId, getArchived, num);

            return num;
        }

        public async Task UpdateProjectAsync(ProjectUpdateDto dto, Guid updatedByUserId)
        {
            var project = await repo.GetByIdAsync(dto.Id)
                ?? throw new Exception("Project not found");

            var updatedFields = new List<string>();
            if (dto.Name != null) { project.Name = dto.Name; updatedFields.Add(nameof(dto.Name)); }
            if (dto.Description != null) { project.Description = dto.Description; updatedFields.Add(nameof(dto.Description)); }
            if (dto.ProjectStatus.HasValue) { project.Status = dto.ProjectStatus.Value; updatedFields.Add(nameof(dto.ProjectStatus)); }

            project.UpdatedAt = DateTime.UtcNow;
            project.UpdatedBy = updatedByUserId;

            await repo.UpdateAsync(project);
            await unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Project updated: {ProjectId}, UpdatedBy: {UserId}, Fields: {Fields}, Time: {Time}",
                dto.Id, updatedByUserId, string.Join(",", updatedFields), DateTime.UtcNow);
        }
    }

}
