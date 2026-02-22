using DomainLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfrastructureLayer.Configuration
{
    public class ClientConfiguration : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {

            builder.Property(c => c.Balance).HasColumnType("decimal(18,2)").HasDefaultValue(0);

            builder.Property(c => c.Name).IsRequired();
            builder.Property(c => c.Status).IsRequired();

            builder.HasIndex(c => new { c.OrganizationId, c.Status });
            builder.HasIndex(c => c.Name);
        }
    }
}
