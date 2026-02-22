using InfrastructureLayer.Identity.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace InfrastructureLayer.Configuration
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder
            .Property(u => u.OrganizationId)
            .IsRequired();

            builder
            .HasIndex(u => u.NormalizedUserName)
            .IsUnique(false);

            builder
                .HasIndex(u => u.NormalizedEmail)
                .IsUnique(false);

    
            builder
                .HasIndex(u => new { u.OrganizationId, u.NormalizedUserName })
                .IsUnique()
                .HasFilter("[IsArchived] = 0");

            
            builder
                .HasIndex(u => new { u.OrganizationId, u.NormalizedEmail })
                .IsUnique()
                .HasFilter("[IsArchived] = 0");
        }
    }
}
