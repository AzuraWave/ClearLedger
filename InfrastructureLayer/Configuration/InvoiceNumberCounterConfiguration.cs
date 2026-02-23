        using DomainLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfrastructureLayer.Configuration
{
    public class InvoiceNumberCounterConfiguration : IEntityTypeConfiguration<InvoiceNumberCounter>
    {
        public void Configure(EntityTypeBuilder<InvoiceNumberCounter> builder)
        {
            builder.HasIndex(x => new { x.OrganizationId, x.ClientId, x.Year })
                .IsUnique();

            // Configure RowVersion - for concurrency control
            // For SQLite compatibility, we don't use ValueGeneratedOnAddOrUpdate
            // The application code must set this value explicitly
            builder.Property(x => x.RowVersion)
                .IsConcurrencyToken()
                .IsRequired();

            builder.HasOne(x => x.Organization)
           .WithMany(x => x.invoiceNumberCounters)
           .HasForeignKey(x => x.OrganizationId)
           .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Client)
           .WithMany(x => x.invoiceNumberCounters)
           .HasForeignKey(x => x.ClientId)
           .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
