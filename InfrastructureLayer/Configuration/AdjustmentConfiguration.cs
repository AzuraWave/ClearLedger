using DomainLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfrastructureLayer.Configuration
{
    public class AdjustmentConfiguration : IEntityTypeConfiguration<Adjustment>
    {
        public void Configure(EntityTypeBuilder<Adjustment> builder)
        {
            builder.ToTable("Adjustments");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Amount)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(a => a.Date)
                   .IsRequired();

            builder.Property(a => a.Reason)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.HasOne(a => a.Organization)
                   .WithMany(c => c.Adjustments)
                   .HasForeignKey(a => a.OrganizationId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Client)
                   .WithMany(c => c.Adjustments)
                   .HasForeignKey(a => a.ClientId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Project)
                   .WithMany(p => p.Adjustments)
                   .HasForeignKey(a => a.ProjectId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(a => new { a.OrganizationId, a.Date });
        }
    }
}
