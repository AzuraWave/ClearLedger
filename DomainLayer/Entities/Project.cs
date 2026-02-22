using DomainLayer.Entities.Base;
using DomainLayer.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomainLayer.Entities
{
    public class Project : EditableTrackedEntity
    {
        public Guid ClientId { get; set; }
        public Client? Client { get; set; }

        public Guid OrganizationId { get; set; }
        public Organization? Organization { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public ProjectStatus Status { get; set; } = ProjectStatus.Active;

        public decimal Balance { get; set; } = 0m;

        public ICollection<Invoice> Invoices { get; set; } = [];
        public ICollection<Adjustment> Adjustments { get; set; } = [];
        public ICollection<Discount> Discounts { get; set; } = [];
    }
}
