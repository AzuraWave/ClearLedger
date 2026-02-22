using ApplicationLayer.DTOs.Client;
using InfrastructureLayer.Identity.User;
using InfrastructureLayer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace InfrastructureLayer.Test
{
    public class UserServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<ILogger<UserService>> _loggerMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            var storeMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
    storeMock.Object,
    Mock.Of<IOptions<IdentityOptions>>(),
    Mock.Of<IPasswordHasher<ApplicationUser>>(),
    Array.Empty<IUserValidator<ApplicationUser>>(),
    Array.Empty<IPasswordValidator<ApplicationUser>>(),
    Mock.Of<ILookupNormalizer>(),
    Mock.Of<IdentityErrorDescriber>(),
    Mock.Of<IServiceProvider>(),
    Mock.Of<ILogger<UserManager<ApplicationUser>>>()
);

            _loggerMock = new Mock<ILogger<UserService>>();
            _userService = new UserService(_userManagerMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task CreateClientUserAsync_Success()
        {
            var dto = new CreateClientUserDto
            {
                Email = "test@example.com",
                FirstName = "John",
                LastName = "Doe",
                TemporaryPassword = "Pass123!",
                OrganizationId = Guid.NewGuid(),
                ClientId = Guid.NewGuid(),
                Role = "Admin"
            };

            _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email))
                .ReturnsAsync((ApplicationUser?)null);
            _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), dto.TemporaryPassword))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), dto.Role))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(u => u.AddClaimsAsync(It.IsAny<ApplicationUser>(), It.IsAny<IEnumerable<Claim>>()))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), dto.TemporaryPassword))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<ApplicationUser, string>((user, pw) => user.Id = Guid.NewGuid());

            var result = await _userService.CreateClientUserAsync(dto);

            Assert.NotEqual(Guid.Empty, result);
            _userManagerMock.Verify(u => u.CreateAsync(It.IsAny<ApplicationUser>(), dto.TemporaryPassword), Times.Once);
            _userManagerMock.Verify(u => u.AddToRoleAsync(It.IsAny<ApplicationUser>(), dto.Role), Times.Once);
        }

        [Fact]
        public async Task CreateClientUserAsync_DuplicateEmail_Throws()
        {
            var dto = new CreateClientUserDto { Email = "test@example.com" };
            _userManagerMock.Setup(u => u.FindByEmailAsync(dto.Email))
                .ReturnsAsync(new ApplicationUser());

            await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.CreateClientUserAsync(dto));
        }
    }
}
