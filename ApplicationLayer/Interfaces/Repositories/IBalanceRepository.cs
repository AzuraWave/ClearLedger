using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.Interfaces.Repositories
{
    public interface IBalanceRepository
    {
            Task<decimal> GetClientBalanceAsync(Guid clientId, Guid organizationId);
            Task<decimal> GetProjectBalanceAsync(Guid projectId, Guid organizationId);

            Task UpdateClientBalanceAsync(Guid clientId, Guid organizationId, decimal amount, bool isPositive);
            Task UpdateProjectBalanceAsync(Guid projectId, Guid organizationId, decimal amount, bool isPositive);

        Task SetClientBalanceAsync(Guid client, Guid organization, decimal balance);
        Task SetProjectBalanceAsync(Guid project, Guid organization, decimal balance);
    }
}
