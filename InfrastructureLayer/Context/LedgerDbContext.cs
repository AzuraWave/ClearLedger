using System;
using System.Collections.Generic;
using System.Text;

namespace InfrastructureLayer.Context
{
    using DomainLayer.Entities;
    using InfrastructureLayer.Identity.Roles;
    using InfrastructureLayer.Identity.User;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

    public class LedgerDbContext : IdentityDbContext<ApplicationUser, Roles, Guid>
    {
        public LedgerDbContext(DbContextOptions<LedgerDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(LedgerDbContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Organization> Organizations => Set<Organization>();
        public DbSet<Client> Clients => Set<Client>();
        public DbSet<Project> Projects => Set<Project>();

        public DbSet<ClientPaymentHeader> ClientPaymentHeaders => Set<ClientPaymentHeader>();
        public DbSet<ClientPaymentAllocation> ClientPaymentAllocations => Set<ClientPaymentAllocation>();
    
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<Adjustment> Adjustments => Set<Adjustment>();
        public DbSet<Discount> Discounts => Set<Discount>();
        public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
        public DbSet<InvoiceNumberCounter> InvoiceNumberCounters => Set<InvoiceNumberCounter>();
    }

}
