using DomainLayer.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.DTOs.Projects.Query
{
    public sealed class ProjectQueryDto
    {
        public Guid OrganizationId { get; set; }
        public Guid? ClientId { get; set; }
        public string? Search { get; set; }

        public ProjectStatus? Status { get; set; }

        public string SortBy { get; set; } = "NameAsc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }
}
