using DomainLayer.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.DTOs.Projects
{
    public class ProjectReadDto
    {

        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public ProjectStatus ProjectStatus { get; set; }

        public Guid organizationId { get; set; }
        public Guid clientId { get; set; }

        public decimal Balance { get; set; }
    }
}
