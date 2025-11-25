using System.Collections.Generic;
using System.Threading.Tasks;
using Model;

namespace BlazorApp1.Service
{
    public interface IMService
    {
        Task<List<Message>> GetAllMessagesAsync();
        Task AddMessageAsync(Message message);
        Task<Message> GetMessageByIdAsync(string id);
        Task UpdateMessageAsync(string id, Message message);
        Task DeleteMessageAsync(string id);
    }
}
