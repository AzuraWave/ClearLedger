using DomainLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfrastructureLayer.Configuration
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
            builder.ToTable("Invoice");

            builder.HasKey(i => i.Id);

            builder.Property(i => i.InvoiceNumber)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.HasIndex(i => new { i.OrganizationId, i.InvoiceNumber })
                   .IsUnique();

            builder.Property(i => i.Date)
                   .IsRequired();

            builder.Property(i => i.TotalAmount)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(i => i.Reference)
                   .HasMaxLength(255);

            builder.Property(i => i.Status)
                   .IsRequired();

            builder.HasOne(i => i.Organization)
                   .WithMany(c => c.Invoices)
                   .HasForeignKey(i => i.OrganizationId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(i => i.Client)
                   .WithMany(c => c.Invoices)
                   .HasForeignKey(i => i.ClientId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(i => i.Project)
                   .WithMany(p => p.Invoices)
                   .HasForeignKey(i => i.ProjectId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(i => i.Lines)
                   .WithOne(l => l.Invoice)
                   .HasForeignKey(l => l.InvoiceId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
