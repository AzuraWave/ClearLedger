using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.DTOs.Transactions.Query
{
    public sealed class LedgerEntryQueryDto
    {
        public Guid OrganizationId { get; set; }
        public Guid? ClientId { get; set; }
        public Guid? ProjectId { get; set; }
        public string? Search { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public bool IncludeVoided { get; set; } = false;

        public string SortBy { get; set; } = "DateDesc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }


}
