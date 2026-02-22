using DomainLayer.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfrastructureLayer.Identity.User
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public Guid? OrganizationId { get; set; }
        public Organization? Organization { get; set; }

        public Guid? ClientId { get; set; }
        public Client? Client { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}".Trim();

        public bool IsArchived { get; set; } = false;
    }
}
