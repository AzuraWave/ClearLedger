using DomainLayer.Entities.Base;
using DomainLayer.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomainLayer.Entities
{
    public class Invoice : TrackedEntity
    {
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; } = null!;

        public Guid? ClientId { get; set; }
        public Client Client { get; set; } = null!;

        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        public string InvoiceNumber { get; set; } = null!;

        public InvoiceStatus Status { get; set; }

        public DateTime Date { get; set; }
        public string? Reference { get; set; }

        public decimal TotalAmount { get; set; }

        public bool IsVoided { get; set; }
        public DateTime? VoidedAt { get; set; }


        public ICollection<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();

    }
}
