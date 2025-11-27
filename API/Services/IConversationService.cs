using Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Services
{
    public interface IConversationService
    {
        Task<List<Conversation>> GetAllForParticipantAsync(string clientId);
        Task<Conversation?> GetByIdAsync(string id);
        Task<Conversation> CreateAsync(Conversation conversation);
        Task AddParticipantAsync(string conversationId, string clientId);
    }
}
