using DomainLayer.Entities.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomainLayer.Entities
{
    public class ClientPaymentHeader : TrackedEntity
    {
        
        public Guid ClientId { get; set; }
        public Client Client { get; set; } = null!;

        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; } = null!;
        public decimal TotalAmount { get; set; }
        public DateTime Date { get; set; }
        public string? Reference { get; set; }

        public bool IsVoided { get; set; }
        public DateTime? VoidedAt { get; set; }

        public ICollection<ClientPaymentAllocation> Allocations { get; set; } = [];
    }

  

}
