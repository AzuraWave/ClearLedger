using ApplicationLayer.Interfaces.Repositories;
using DomainLayer.Entities;
using InfrastructureLayer.Context;
using InfrastructureLayer.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfrastructureLayer.Repositories
{
    public class InvoiceRepository : Repository<Invoice>, IInvoiceRepository
    {
        public InvoiceRepository(LedgerDbContext db) : base(db)
        {
        }

        public Task<List<Invoice>> GetByClientAsync(Guid clientId, Guid organizationId)
        {
            throw new NotImplementedException();
        }

        public async Task<Invoice?> GetByIdAsync(Guid invoiceId, Guid organizationId)
        {
            return await _db.Invoices
                   .Include(i => i.Lines)
                   .Include(i => i.Client)
                    .Include(i => i.Project)
                   .FirstOrDefaultAsync(i => i.Id == invoiceId && i.OrganizationId == organizationId);
        }

        public Task<List<Invoice>> GetByProjectAsync(Guid projectId, Guid organizationId)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetNextInvoiceNumberAsync(Guid organizationId, Guid clientId)
        {
            var year = DateTime.UtcNow.Year;
            int nextNumber;

            string orgName = await _db.Organizations
                .Where(o => o.Id == organizationId)
                .Select(o => o.Name)
                .FirstOrDefaultAsync() ?? "ORG";

            string clientName = await _db.Clients
                .Where(c => c.Id == clientId)
                .Select(c => c.Name)
                .FirstOrDefaultAsync() ?? "CLIENT";

            const int maxRetries = 3;
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                using var transaction = await _db.Database.BeginTransactionAsync();

                try
                {
                    var counter = await _db.Set<InvoiceNumberCounter>()
                        .FirstOrDefaultAsync(c => c.OrganizationId == organizationId
                                               && c.ClientId == clientId
                                               && c.Year == year);

                    if (counter == null)
                    {
                        counter = new InvoiceNumberCounter
                        {
                            OrganizationId = organizationId,
                            ClientId = clientId,
                            Year = year,
                            LastNumber = 1,
                            RowVersion = new byte[8] // Initialize RowVersion for SQLite compatibility
                        };
                        _db.Add(counter);
                        nextNumber = 1;
                    }
                    else
                    {
                        counter.LastNumber++;
                        nextNumber = counter.LastNumber;
                    }

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Format: ORG-CLIENT-YEAR-XXXX
                    return $"{orgName.Substring(0, 4)}-{clientName.Substring(0, 4)}-{year}-{nextNumber:D4}";
                }
                catch (DbUpdateConcurrencyException)
                {
                    // retry
                    if (attempt == maxRetries - 1) throw;
                }
            }

            throw new InvalidOperationException("Could not generate invoice number after retries.");

        }

        public async Task<decimal> GetOrgInvoicesForCurrentMonth(Guid organizationId)
        {
            return await _db.Invoices
                .Where(i => i.OrganizationId == organizationId && i.CreatedAt.Month == DateTime.UtcNow.Month && i.CreatedAt.Year == DateTime.UtcNow.Year && !i.IsVoided)
                .SumAsync(i => i.TotalAmount);
        }

        public IQueryable<Invoice> QueryAll(Guid organizationId)
        {
            return _db.Invoices
                   .Include(i => i.Lines)
                   .Where(i => i.OrganizationId == organizationId)
                   .AsQueryable();
        }

        public IQueryable<Invoice> QueryByClient(Guid organizationId, Guid clientId)
        {
            return _db.Invoices
                   .Include(i => i.Lines)
                   .Where(i => i.OrganizationId == organizationId && i.ClientId == clientId)
                   .AsQueryable();
        }

        public IQueryable<Invoice> QueryByProject(Guid organizationId, Guid projectId)
        {
            return _db.Invoices
                           .Include(i => i.Lines)
                           .Where(i => i.OrganizationId == organizationId && i.ProjectId == projectId)
                           .AsQueryable();
        }
    }
}
