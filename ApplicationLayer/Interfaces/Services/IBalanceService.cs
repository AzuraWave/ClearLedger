using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.Interfaces.Services
{
    public interface IBalanceService
    {
        Task RecalculateProjectAsync(Guid projectId, Guid organizationId);
        Task RecalculateClientAsync(Guid clientId, Guid organizationId);

        Task AdjustClientBalanceAsync(Guid clientId, decimal amount, Guid organizationId, bool isPositive);
        Task AdjustProjectBalanceAsync(Guid projectId, decimal amount, Guid organizationId, bool isPositive);
    
        
    }
}
