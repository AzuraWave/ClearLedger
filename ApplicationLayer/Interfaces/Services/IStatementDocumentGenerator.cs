using ApplicationLayer.DTOs.Transactions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.Interfaces.Services
{
    public interface IStatementDocumentGenerator
    {
        public byte[] Generate(StatementOfAccountDto dto);
    }
}
