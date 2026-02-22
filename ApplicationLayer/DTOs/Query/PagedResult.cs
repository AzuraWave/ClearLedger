using ApplicationLayer.DTOs.Transactions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.DTOs.Query
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int TotalCount { get; set; }

        public PagedResult(IEnumerable<T> items, int total)
        {
            Items = items;
            TotalCount = total;
        }

        public static implicit operator PagedResult<T>(PagedResult<TransactionDto> v)
        {
            throw new NotImplementedException();
        }
    }
}
