using DocumentFormat.OpenXml.Spreadsheet;
using DomainLayer.Entities;
using InfrastructureLayer.Identity.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfrastructureLayer.Configuration
{
    public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
    {
        public void Configure(EntityTypeBuilder<Organization> builder)
        {
            builder.Property(o => o.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(o => o.ApiKeyHash).HasMaxLength(256).IsUnicode(false).IsRequired(false);
            builder.Property(o => o.DefaultAutomationUserId).IsRequired(false);

            builder.Property(o => o.CreatedAt)
                   .IsRequired();

            
            builder.HasMany(o => o.Clients)
                   .WithOne(c => c.Organization)
                   .HasForeignKey(c => c.OrganizationId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(o => o.Projects)
                     .WithOne(p => p.Organization)
                     .HasForeignKey(p => p.OrganizationId)
                     .OnDelete(DeleteBehavior.Restrict);


            builder.HasMany(o => o.PaymentHeaders)
                     .WithOne(ph => ph.Organization)
                     .HasForeignKey(ph => ph.OrganizationId)
                     .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(o => o.Name).IsUnique();
        }
    }
}
