using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.DTOs.Client
{
    public class ClientOverviewDto
    {
        public decimal TotalInvoiced { get; set; }
        public decimal TotalPaid { get; set; }

        public int ActiveProjects { get; set; }
    }
}
