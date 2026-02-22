using DomainLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfrastructureLayer.Configuration
{
    public class DiscountConfiguration : IEntityTypeConfiguration<Discount>
    {
        public void Configure(EntityTypeBuilder<Discount> builder)
        {
            builder.ToTable("Discounts");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Amount)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(d => d.Date)
                   .IsRequired();

            builder.Property(d => d.Reason)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.HasOne(d => d.Organization)
                   .WithMany(c => c.Discounts)
                   .HasForeignKey(d => d.OrganizationId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.Client)
                   .WithMany(c => c.Discounts)
                   .HasForeignKey(d => d.ClientId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.Project)
                   .WithMany(p => p.Discounts)
                   .HasForeignKey(d => d.ProjectId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(d => new { d.OrganizationId, d.Date });
        }
    }
}
