using DomainLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfrastructureLayer.Configuration
{
    public class ProjectConfiguration : IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<Project> builder)
        {
            builder.HasOne(p => p.Client)
                   .WithMany(c => c.Projects)
                   .HasForeignKey(p => p.ClientId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Organization)
                   .WithMany(o => o.Projects)
                   .HasForeignKey(p => p.OrganizationId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(p => p.Name).IsRequired();
            builder.Property(p => p.Status).IsRequired();
            builder.Property(p => p.ClientId).IsRequired();

            builder.HasIndex(p => new { p.OrganizationId, p.ClientId });
            builder.HasIndex(p => new { p.OrganizationId, p.ClientId, p.Name }).IsUnique();

            builder.Property(c => c.Balance).HasColumnType("decimal(18,2)").HasDefaultValue(0);


        }
    }
}
