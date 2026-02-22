using DomainLayer.Entities.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomainLayer.Entities
{
    public class Adjustment : TrackedEntity
    {
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; } = null!;

        public Guid ClientId { get; set; }
        public Client Client { get; set; } = null!;

        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        public decimal Amount { get; set; }

        public DateTime Date { get; set; }
        public string Reason { get; set; } = null!;

        public Guid? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }

        public bool IsPositive { get; set; }

    }
}
