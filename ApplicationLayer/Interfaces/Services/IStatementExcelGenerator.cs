using ApplicationLayer.DTOs.Transactions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.Interfaces.Services
{
    public interface IStatementExcelGenerator
    {
        public byte[] Generate(StatementOfAccountDto dto);
    }
}
