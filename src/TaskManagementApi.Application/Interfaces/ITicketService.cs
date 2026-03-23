using TaskManagementApi.Domain.Entities;

namespace TaskManagementApi.Application.Interfaces
{
    public interface ITicketService
    {
        Task<IEnumerable<Ticket>> GetAllTicketsAsync();
        Task<Ticket?> GetTicketByIdAsync(int id);
    }
}
