using ApplicationLayer.DTOs.Organization;
using ApplicationLayer.Interfaces.Repositories.Base;
using DomainLayer.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.Interfaces.Repositories
{
    public interface IOrganizationRepository : IRepository<Organization>
    {

        public Task<OrganizationReadDto?> GetOrganizationAsync(Guid organiztionId);

        public Task<decimal> GetOrganizationBalanceAsync(Guid organizationId);

        public Task AddApiKeyAsync(Guid organizationId,  string hashedApi);
    }

}
