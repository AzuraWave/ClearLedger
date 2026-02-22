using ApplicationLayer.DTOs.Projects;
using ApplicationLayer.Interfaces.Patterns;
using ApplicationLayer.Interfaces.Repositories;
using ApplicationLayer.Services;
using DomainLayer.Entities;
using DomainLayer.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApplicationLayer.Test
{
    public class ProjectServiceTests
    {
        private readonly Mock<IProjectRepository> _repoMock = new();
        private readonly Mock<IUnitOfWork> _unitMock = new();
        private readonly Mock<ILogger<ProjectService>> _loggerMock = new();
        private readonly ProjectService _service;

        public ProjectServiceTests()
        {
            _service = new ProjectService(_repoMock.Object, _unitMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task CreateProjectAsync_Should_Create_And_Return_Id()
        {
            var dto = new ProjectCreateDto
            {
                Name = "Test",
                clientId = Guid.NewGuid(),
                organizationId = Guid.NewGuid()
            };

            _repoMock.Setup(r => r.AddAsync(It.IsAny<Project>()))
                     .Returns(Task.CompletedTask);
            _unitMock.Setup(u => u.SaveChangesAsync())
                     .Returns(Task.CompletedTask);

            var id = await _service.CreateProjectAsync(dto, Guid.NewGuid());

            Assert.NotEqual(Guid.Empty, id);
            _repoMock.Verify(r => r.AddAsync(It.Is<Project>(p =>
                p.Name == dto.Name &&
                p.ClientId == dto.clientId &&
                p.OrganizationId == dto.organizationId &&
                p.Status == ProjectStatus.Active)), Times.Once);
        }

        [Fact]
        public async Task CreateProjectAsync_Should_Throw_When_Name_Invalid()
        {
            var dto = new ProjectCreateDto { Name = "" };
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.CreateProjectAsync(dto, Guid.NewGuid()));
        }

        [Fact]
        public async Task ArchiveProjectAsync_Should_Set_Status_To_Archived()
        {
            var project = new Project { Id = Guid.NewGuid(), Status = ProjectStatus.Active };

            _repoMock.Setup(r => r.GetByIdAsync(project.Id))
                     .ReturnsAsync(project);
            _repoMock.Setup(r => r.UpdateAsync(project))
                     .Returns(Task.CompletedTask);
            _unitMock.Setup(u => u.SaveChangesAsync())
                     .Returns(Task.CompletedTask);

            await _service.ArchiveProjectAsync(project.Id, Guid.NewGuid());

            Assert.Equal(ProjectStatus.Archived, project.Status);
            _repoMock.Verify(r => r.UpdateAsync(project), Times.Once);
        }

        [Fact]
        public async Task GetProjectAsync_Returns_Null_When_NotFound()
        {
            _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                     .ReturnsAsync((Project?)null);

            var result = await _service.GetProjectAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        [Fact]
        public async Task GetProjectsByClientAsync_Maps_To_Dtos()
        {
            var projects = new List<Project>
        {
            new Project { Id = Guid.NewGuid(), Name = "GetClient", ClientId = Guid.NewGuid(), OrganizationId = Guid.NewGuid(), Status = ProjectStatus.Active }
        };

            _repoMock.Setup(r => r.GetByClientAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), false))
                     .ReturnsAsync(projects);

            var result = await _service.GetProjectsByClientAsync(Guid.NewGuid(), Guid.NewGuid());

            Assert.Single(result);
        }

        [Fact]
        public async Task UpdateProjectAsync_Should_Update_Only_Provided_Fields()
        {
            var project = new Project { Id = Guid.NewGuid(), Name = "Old", Status = ProjectStatus.Active };

            var dto = new ProjectUpdateDto
            {
                Id = project.Id,
                Name = "New"
            };

            _repoMock.Setup(r => r.GetByIdAsync(project.Id))
                     .ReturnsAsync(project);
            _repoMock.Setup(r => r.UpdateAsync(project))
                     .Returns(Task.CompletedTask);
            _unitMock.Setup(u => u.SaveChangesAsync())
                     .Returns(Task.CompletedTask);

            await _service.UpdateProjectAsync(dto, Guid.NewGuid());

            Assert.Equal("New", project.Name);
            Assert.Equal(ProjectStatus.Active, project.Status);
        }
    }
}
