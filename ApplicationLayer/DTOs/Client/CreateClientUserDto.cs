using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ApplicationLayer.DTOs.Client
{
    public class CreateClientUserDto
    {
        [Required]
        public string? FirstName { get; set; }
        [Required]
        public string? LastName { get; set; }
        [Required]
        public Guid OrganizationId { get; set; }

        [Required]
        public Guid ClientId { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string TemporaryPassword { get; set; } = null!;

        public string? Role { get; set; }
    }
}
