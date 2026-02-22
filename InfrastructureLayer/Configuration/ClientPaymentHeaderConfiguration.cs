using DomainLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfrastructureLayer.Configuration
{
    public class ClientPaymentHeaderConfiguration : IEntityTypeConfiguration<ClientPaymentHeader>
    {
        public void Configure(EntityTypeBuilder<ClientPaymentHeader> builder)
        {
            builder.HasMany(h => h.Allocations)
                .WithOne(a => a.Header)
                .HasForeignKey(a => a.ClientPaymentHeaderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.TotalAmount)
                .HasPrecision(18, 2);

            builder.HasIndex(h => new { h.OrganizationId, h.ClientId });
            builder.HasIndex(h => h.Date);
        }
    }
}
