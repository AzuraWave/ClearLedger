using DomainLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfrastructureLayer.Configuration
{
    public class ClientPaymentAllocationConfiguration : IEntityTypeConfiguration<ClientPaymentAllocation>
    {
        public void Configure(EntityTypeBuilder<ClientPaymentAllocation> builder)
        {
            builder.Property(x => x.Amount)
                .HasPrecision(18, 2);

            builder.HasIndex(a => new { a.ClientPaymentHeaderId, a.ProjectId });

        }
    }
}
