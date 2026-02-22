using DomainLayer.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ApplicationLayer.DTOs.Projects
{
    public class ProjectCreateDto
    {
        [Required]
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public ProjectStatus projectStatus { get; set; }

        public Guid clientId { get; set; }
        public Guid organizationId { get; set; }

    }
}
