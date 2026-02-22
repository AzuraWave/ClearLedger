using DomainLayer.Entities.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomainLayer.Entities
{
    public class ClientPaymentAllocation : Entity
    {
        public Guid ClientPaymentHeaderId { get; set; }
        public ClientPaymentHeader Header { get; set; } = null!;
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        public decimal Amount { get; set; }

        
    }
}
