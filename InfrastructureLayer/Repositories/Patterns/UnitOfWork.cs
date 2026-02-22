using ApplicationLayer.Interfaces.Patterns;
using InfrastructureLayer.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfrastructureLayer.Repositories.Patterns
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly LedgerDbContext _dbContext;
        public UnitOfWork(LedgerDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
