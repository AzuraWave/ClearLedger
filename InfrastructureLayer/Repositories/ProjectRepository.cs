using ApplicationLayer.Interfaces.Repositories;
using DomainLayer.Entities;
using DomainLayer.Enums;
using InfrastructureLayer.Context;
using InfrastructureLayer.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfrastructureLayer.Repositories
{
    public class ProjectRepository : Repository<Project>, IProjectRepository
    {

        public ProjectRepository(LedgerDbContext context) : base(context)
        {
        }

        public async Task ArchiveAsync(Guid projectId, Guid orgId, Guid updatedBy)
        {
            var project = await _db.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId && p.OrganizationId == orgId);

            if (project == null)
                return;

            if (project.Status == ProjectStatus.Archived)
                return;

            project.Status = ProjectStatus.Archived;
            project.UpdatedAt = DateTime.UtcNow;
            project.UpdatedBy = updatedBy;

            _db.Projects.Update(project);
            await _db.SaveChangesAsync();
        }

        public async Task<List<Project>> GetByClientAsync(Guid clientId, Guid orgId, bool includeArchived = false)
        {
            var query = _db.Projects
                .AsNoTracking()
                .Where(p => p.ClientId == clientId && p.OrganizationId == orgId);

            if (!includeArchived)
            {
                query = query.Where(p => p.Status != ProjectStatus.Archived);
            }

            return await query.ToListAsync();
        }

        public async Task<Project?> GetByIdAsync(Guid projectId, Guid orgId)
        {
            return await _db.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == projectId && p.OrganizationId == orgId);

        }

        public async Task<int> GetProjectTotalByOrg(Guid organizationId, bool includeArchived = false)
        {
            return await _db.Projects
                .AsNoTracking()
                .Where(p => p.OrganizationId == organizationId && (includeArchived || p.Status != ProjectStatus.Archived))
                .CountAsync();
        }

        public Task<IQueryable<Project>> QueryAll(Guid organizationId)
        {
            return Task.FromResult(_db.Projects
                .AsNoTracking()
                .Where(p => p.OrganizationId == organizationId)
                .AsQueryable());
        }
    }
}
