using DomainLayer.Entities.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace DomainLayer.Entities
{
    public class InvoiceLine : TrackedEntity
    {
        public Guid InvoiceId { get; set; }
        public Invoice Invoice { get; set; } = null!;

        public string Description { get; set; } = null!;

        public decimal Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal LineTotal { get; set; }
    }

}
