using ApplicationLayer.Interfaces.Repositories;
using DomainLayer.Entities;
using DomainLayer.Enums;
using InfrastructureLayer.Context;
using InfrastructureLayer.Identity.User;
using InfrastructureLayer.Repositories.Base;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfrastructureLayer.Repositories
{
    public class ClientRepository : Repository<Client>, IClientRepository
    {
        private readonly UserManager<ApplicationUser> userManager;

        public ClientRepository(LedgerDbContext db, UserManager<ApplicationUser> UserManage) : base(db)
        {
            userManager = UserManage;
        }

        public async Task ArchiveAsync(Client client)
        {
            client.Status = ClientStatus.Archived;
            _db.Set<Client>().Update(client);
            await Task.CompletedTask;
        }

        public async Task<Client> GetClientByProject(
            Guid projectId,
            Guid organizationId,
            bool includeArchived = false)
        {
            var query = _db.Set<Client>()
                .Where(c => c.Projects.Any(p => p.Id == projectId && p.OrganizationId == organizationId));
            if (!includeArchived)
                query = query.Where(c => c.Status != ClientStatus.Archived);
            return await query.FirstOrDefaultAsync() 
                ?? throw new InvalidOperationException($"No client found for project ID {projectId}");
        }

        public async Task<Client?> GetByIdAsync(
            Guid clientId,
            Guid organizationId,
            bool includeArchived = false)
        {
            var query = _db.Set<Client>()
                .Where(c => c.Id == clientId && c.OrganizationId == organizationId);

            if (!includeArchived)
                query = query.Where(c => c.Status != ClientStatus.Archived);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<Client>> GetByOrganizationAsync(
            Guid organizationId,
            bool includeArchived = false)
        {
            var query = _db.Set<Client>()
                .Where(c => c.OrganizationId == organizationId);

            if (!includeArchived)
                query = query.Where(c => c.Status == ClientStatus.Active);

            return await query.ToListAsync();
        }


        public async Task DeleteClientUserAsync(Guid clientId)
        {
            // Find all users that are linked to this client
            var users = userManager.Users
                .Where(u => u.ClientId.HasValue && u.ClientId.Value == clientId)
                .ToList();

            foreach (var user in users)
            {
                var result = await userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to delete user {user.Id}: {errors}");
                }
            }
        }

        public async Task ArchiveClientUserAsync(Guid clientId)
        {
            // "Archive" by locking out the user account 
            var users = userManager.Users
                .Where(u => u.ClientId.HasValue && u.ClientId.Value == clientId)
                .ToList();

            foreach (var user in users)
            {
                // Set LockoutEnd to a far future date to effectively disable sign-in
                user.LockoutEnd = DateTimeOffset.MaxValue;
                user.IsArchived = true;

                var result = await userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to archive (lockout) user {user.Id}: {errors}");
                }
            }
        }

        public Task<IQueryable<Client>> QueryAll(Guid organizationId)
        {
            return Task.FromResult(_db.Set<Client>().Where(c => c.OrganizationId == organizationId).AsNoTracking());
        }
    }
}
