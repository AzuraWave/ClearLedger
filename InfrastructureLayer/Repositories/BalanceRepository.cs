using ApplicationLayer.Interfaces.Repositories;
using DomainLayer.Entities;
using InfrastructureLayer.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfrastructureLayer.Repositories
{
    public class BalanceRepository : IBalanceRepository
    {
        private readonly LedgerDbContext _context;
        public BalanceRepository(LedgerDbContext _db)
        {
            _context = _db;

        }
        public async Task<decimal> GetClientBalanceAsync(Guid clientId, Guid organizationId)
        {
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == clientId && c.OrganizationId == organizationId);
            if (client == null)
                throw new InvalidOperationException($"Client with ID {clientId} not found in organization {organizationId}");

            return client.Balance;
        }

        public async Task<decimal> GetProjectBalanceAsync(Guid projectId, Guid organizationId)
        {
            var project = await _context.Projects
                  .FirstOrDefaultAsync(p => p.Id == projectId && p.OrganizationId == organizationId);

            if (project == null)
                throw new InvalidOperationException($"Project with ID {projectId} not found in organization {organizationId}");

            return project.Balance;
        }

        public async Task SetClientBalanceAsync(Guid client, Guid organization, decimal balance)
        {
            await _context.Clients
                .Where(c => c.Id == client && c.OrganizationId == organization)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.Balance, balance));
        }

        public async Task SetProjectBalanceAsync(Guid project, Guid organization, decimal balance)
        {
            await _context.Projects
                .Where(p => p.Id == project && p.OrganizationId == organization)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Balance, balance));
        }

        public async Task UpdateClientBalanceAsync(Guid clientId, Guid organizationId, decimal amount, bool isPositive)
        {
            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.Id == clientId && c.OrganizationId == organizationId);

            if (client == null)
                throw new InvalidOperationException($"Client with ID {clientId} not found in organization {organizationId}");

            client.Balance += isPositive ? amount : -amount;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProjectBalanceAsync(Guid projectId, Guid organizationId, decimal amount, bool isPositive)
        {
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId && p.OrganizationId == organizationId);

            if (project == null)
                throw new InvalidOperationException($"Project with ID {projectId} not found in organization {organizationId}");

            project.Balance += isPositive ? amount : -amount;
            await _context.SaveChangesAsync();
        }
    }
}
