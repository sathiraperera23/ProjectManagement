using Moq;
using TaskManagementApi.Models;
using TaskManagementApi.Repositories;
using TaskManagementApi.Services;

namespace TaskManagementApi.Tests
{
    public class TicketServiceTests
    {
        [Fact]
        public async Task GetAllTicketsAsync_ReturnsAllTickets()
        {
            // Arrange
            var mockRepo = new Mock<IRepository<Ticket>>();
            var expectedTickets = new List<Ticket>
            {
                new Ticket { Id = 1 },
                new Ticket { Id = 2 }
            };
            mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(expectedTickets);
            var service = new TicketService(mockRepo.Object);

            // Act
            var result = await service.GetAllTicketsAsync();

            // Assert
            Assert.Equal(expectedTickets.Count, result.Count());
            mockRepo.Verify(repo => repo.GetAllAsync(), Times.Once);
        }
    }
}
