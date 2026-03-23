using Moq;
using TaskManagementApi.Domain.Entities;
using TaskManagementApi.Application.Interfaces;
using TaskManagementApi.Application.Services;

namespace TaskManagementApi.Tests
{
    public class TicketServiceTests
    {
        [Fact]
        public void Service_CanBeInitialized()
        {
            var service = new TicketService(
                new Mock<IRepository<Ticket>>().Object,
                new Mock<IRepository<TicketStatus>>().Object,
                new Mock<IRepository<Project>>().Object,
                new Mock<IRepository<TicketStatusHistory>>().Object,
                new Mock<IRepository<TicketLink>>().Object);

            Assert.NotNull(service);
        }
    }
}
