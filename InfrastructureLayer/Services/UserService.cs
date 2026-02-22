using ApplicationLayer.DTOs.Client;
using ApplicationLayer.Interfaces.Services;
using InfrastructureLayer.Identity.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace InfrastructureLayer.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UserService> _logger;

        public UserService(UserManager<ApplicationUser> userManager, ILogger<UserService> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<Guid> CreateClientUserAsync(CreateClientUserDto dto)
        {
            _logger.LogInformation("Creating client user {Email} for Org {OrgId}, Client {ClientId}",
        dto.Email, dto.OrganizationId, dto.ClientId);
            // duplicate check
            var existing = await _userManager.FindByEmailAsync(dto.Email);
            if (existing != null)
            {
                _logger.LogWarning("User creation failed. Email already exists {Email}", dto.Email);
                throw new InvalidOperationException($"A user with email '{dto.Email}' already exists.");
            }
            var user = new ApplicationUser
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                UserName = dto.Email,
                Email = dto.Email,
                OrganizationId = dto.OrganizationId,
                ClientId = dto.ClientId,
                EmailConfirmed = true
            };

            // Create with password so account is usable immediately
            var createResult = await _userManager.CreateAsync(user, dto.TemporaryPassword);
            if (!createResult.Succeeded)
            {
                _logger.LogError("Failed to create user {Email}: {Errors}",
                    dto.Email, string.Join(",", createResult.Errors.Select(e => e.Description)));
                throw new InvalidOperationException(string.Join(", ", createResult.Errors.Select(e => e.Description)));
            }
            var role = string.IsNullOrWhiteSpace(dto.Role) ? "Customer" : dto.Role;
            var addRoleResult = await _userManager.AddToRoleAsync(user, role);
            if (!addRoleResult.Succeeded)
            {
                _logger.LogError("Failed to assign role {Role} to user {UserId}: {Errors}",
                    role, user.Id, string.Join(",", addRoleResult.Errors.Select(e => e.Description)));
                throw new InvalidOperationException($"Failed to assign role: {string.Join(", ", addRoleResult.Errors.Select(e => e.Description))}");
            }
            // Persist OrganizationId and ClientId as claims
            var claims = new[]
            {
                new Claim("OrganizationId", dto.OrganizationId.ToString()),
                new Claim("ClientId", dto.ClientId.ToString())
            };
            var addClaimsResult = await _userManager.AddClaimsAsync(user, claims);
            if (!addClaimsResult.Succeeded)
            {
                _logger.LogError("Failed to add claims for user {UserId}: {Errors}",
                    user.Id, string.Join(",", addClaimsResult.Errors.Select(e => e.Description)));
                throw new InvalidOperationException($"Failed to add claims: {string.Join(", ", addClaimsResult.Errors.Select(e => e.Description))}");
            }

            _logger.LogInformation("User created successfully {UserId} for Org {OrgId}",
                user.Id, dto.OrganizationId);
            return user.Id;
        }

        public async Task ArchiveUserAsync(Guid userId)
        {
            _logger.LogInformation("Archiving user {UserId}", userId);
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) {
                _logger.LogWarning("Archive failed. User not found {UserId}", userId);
                return;
                    }

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.MaxValue;
            user.IsArchived = true;

            var roles = await _userManager.GetRolesAsync(user);
            if (roles != null && roles.Count > 0)
            {
                var removeRolesResult = await _userManager.RemoveFromRolesAsync(user, roles);
                if (!removeRolesResult.Succeeded)
                    throw new InvalidOperationException($"Failed to remove roles for user {user.Id}: {string.Join(';', removeRolesResult.Errors.Select(e => e.Description))}");
            }

            // Update security stamp to invalidate existing cookies/tokens
            await _userManager.UpdateSecurityStampAsync(user);

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                throw new InvalidOperationException($"Failed to archive user {user.Id}: {string.Join(';', updateResult.Errors.Select(e => e.Description))}");
            _logger.LogInformation("User archived successfully {UserId}", user.Id);

        }

        public async Task DeleteUserAsync(Guid userId)
        {
            _logger.LogInformation("Deleting user {UserId}", userId);
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) {
                _logger.LogWarning("Delete skipped. User not found {UserId}", userId); 
                return; }

            var claims = await _userManager.GetClaimsAsync(user);
            if (claims.Any())
                await _userManager.RemoveClaimsAsync(user, claims);

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Any())
                await _userManager.RemoveFromRolesAsync(user, roles);

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to delete user {UserId}: {Errors}",
    user.Id, string.Join(";", result.Errors.Select(e => e.Description)));
                throw new InvalidOperationException($"Failed to delete user {user.Id}: {string.Join(';', result.Errors.Select(e => e.Description))}");
            }

            _logger.LogInformation("User deleted successfully {UserId}", user.Id);
        }
    }
}
