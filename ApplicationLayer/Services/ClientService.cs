using ApplicationLayer.DTOs.Client;
using ApplicationLayer.Interfaces.Patterns;
using ApplicationLayer.Interfaces.Repositories;
using ApplicationLayer.Interfaces.Services;
using DomainLayer.Entities;
using DomainLayer.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace ApplicationLayer.Services
{
    public class ClientService : IClientService
    {
        private readonly IClientRepository _clientRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ClientService> _logger;

        public ClientService(IUnitOfWork unitOfWork, IClientRepository clientRepository, ILogger<ClientService> logger)
        {
            _clientRepository = clientRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Guid> CreateClientAsync(Guid organizationId, ClientCreateDto dto, Guid createdByUserId)
        {
            var client = new Client
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                Name = dto.Name,
                PhoneNumber = dto.PhoneNumber,
                BillingEmail = dto.BillingEmail,
                Address = dto.Address,
                Notes = dto.Notes,
                Status = ClientStatus.Active,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdByUserId,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = createdByUserId

            };

            await _clientRepository.AddAsync(client);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Client created {ClientId} in Org {OrgId} by {UserId}",
        client.Id, organizationId, createdByUserId);
            return client.Id;
        }

        public async Task<ClientReadDto?> GetClientAsync(Guid clientId, Guid organizationId, bool getArchived = false)
        {
            var client = await _clientRepository.GetByIdAsync(clientId, organizationId, getArchived);
            if (client == null)
            {
                _logger.LogDebug("Client not found {ClientId} in Org {OrgId}", clientId, organizationId);
                return null;
            }
            return new ClientReadDto
            {
                Id = client.Id,
                Name = client.Name,
                PhoneNumber = client.PhoneNumber,
                BillingEmail = client.BillingEmail,
                Address = client.Address,
                Notes = client.Notes,
                Status = client.Status,
                Balance = client.Balance
            };
        }

        public async Task<ClientReadDto> GetClientByProject(Guid projectId, Guid organizationId, bool getArchived = false)
        {
            var client = await _clientRepository.GetClientByProject(projectId, organizationId, getArchived);
            if (client == null) throw new KeyNotFoundException("Client not found for the given project ID.");
            return new ClientReadDto
            {
                Id = client.Id,
                Name = client.Name,
                PhoneNumber = client.PhoneNumber,
                BillingEmail = client.BillingEmail,
                Address = client.Address,
                Notes = client.Notes,
                Status = client.Status,
                Balance = client.Balance
            };
        }

        public async Task<IEnumerable<ClientReadDto>> GetClientsByOrganizationAsync(Guid organizationId, bool getArchived = false)
        {
            var clients = await _clientRepository.GetByOrganizationAsync(organizationId, getArchived);
            return clients.Select(c => new ClientReadDto
            {
                Id = c.Id,
                Name = c.Name,
                PhoneNumber = c.PhoneNumber,
                BillingEmail = c.BillingEmail,
                Address = c.Address,
                Notes = c.Notes,
                Status = c.Status,
                Balance = c.Balance
            }).ToList();
        }

        public async Task ArchiveClientAsync(Guid clientId, Guid organizationId)
        {
            var client = await _clientRepository.GetByIdAsync(clientId, organizationId);
            if (client == null)
            {
                _logger.LogWarning("Archive failed. Client not found {ClientId} in Org {OrgId}", clientId, organizationId);
                return;
            }

            await _clientRepository.ArchiveAsync(client);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Client archived {ClientId} in Org {OrgId}", clientId, organizationId);

        }

        public async Task UpdateClientAsync(Guid organizationId, ClientUpdateDto dto, Guid modifiedByUserId)
        {
            var client = await _clientRepository.GetByIdAsync(dto.Id, organizationId);
            if (client == null)
                throw new KeyNotFoundException($"Client with id {dto.Id} not found in organization {organizationId}.");

            
            client.Name = dto.Name;
            client.PhoneNumber = dto.PhoneNumber;
            client.Address = dto.Address;
            client.BillingEmail = dto.BillingEmail;
            client.Notes = dto.Notes;
            client.Status = dto.Status;

            
            client.UpdatedAt = DateTime.UtcNow;
            client.UpdatedBy = modifiedByUserId;

            await _clientRepository.UpdateAsync(client);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Client updated {ClientId} in Org {OrgId} by {UserId}",
    dto.Id, organizationId, modifiedByUserId);
        }

        public async Task DeleteClientAsync(Guid organizationID, Guid clientId)
        {
            var client = await _clientRepository.GetByIdAsync(clientId, organizationID);
            if (client == null) throw new Exception("Client not found");

            bool hasProjects = client.Projects.Count != 0;

            if (hasProjects)
            {
                
                await _clientRepository.ArchiveAsync(client);
                await _clientRepository.ArchiveClientUserAsync(client.Id);

                await _unitOfWork.SaveChangesAsync();
            }
            else
            {
                await _clientRepository.DeleteAsync(client);
                await _clientRepository.DeleteClientUserAsync(client.Id);
                await _unitOfWork.SaveChangesAsync();
            }

            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("Client deleted/archived {ClientId} in Org {OrgId}",
    clientId, organizationID);
        }

        public async Task<int> GetClientsTotal(Guid organizationId, bool getArchived = false)
        {
            var num = await _clientRepository.GetByOrganizationAsync(organizationId, getArchived);

            if (num == null)
            {
                return 0;
            }

            return num.Count;
        }
    }

}
