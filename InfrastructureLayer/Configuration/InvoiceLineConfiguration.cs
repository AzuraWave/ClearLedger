using DomainLayer.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfrastructureLayer.Configuration
{
    public class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
    {
        public void Configure(EntityTypeBuilder<InvoiceLine> builder)
        {
            builder.ToTable("InvoiceLines");

            builder.HasKey(l => l.Id);

            builder.Property(l => l.Description)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(l => l.Quantity)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(l => l.UnitPrice)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(l => l.LineTotal)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();
        }
    }
}
