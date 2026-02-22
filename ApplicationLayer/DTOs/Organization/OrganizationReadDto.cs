using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.DTOs.Organization
{
    public class OrganizationReadDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
    }
}
