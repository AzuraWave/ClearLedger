using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.DTOs.Transactions
{
    public class StatementOfAccountDto
    {
        public Guid ClientId { get; set; }
        public string ClientName { get; set; } = null!;
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public decimal OpeningBalance { get; set; }
        public List<TransactionDto> Transactions { get; set; } = new();
        public decimal ClosingBalance => OpeningBalance + Transactions.Sum(t => t.Amount);
    }
}
