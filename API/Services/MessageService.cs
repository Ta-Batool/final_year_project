using System.Collections.Generic;
using System.Threading.Tasks;
using API.MongoModel;
using Microsoft.Extensions.Options;
using Model;
using MongoDB.Driver;

namespace API.Services
{
    public class MessageService : IMessageService
    {
        private readonly IMongoCollection<Message> _messages;

        public MessageService(IOptions<MongoDBSettings> mongoSettings)
        {
            var client = new MongoClient(mongoSettings.Value.ConnectionString);
            var database = client.GetDatabase(mongoSettings.Value.DatabaseName);
            _messages = database.GetCollection<Message>("Messages");
        }

        public async Task<List<Message>> GetAllAsync()
        {
            return await _messages.Find(FilterDefinition<Message>.Empty).ToListAsync();
        }

        public async Task<Message?> GetByIdAsync(string id)
        {
            return await _messages.Find(m => m.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Message message)
        {
            await _messages.InsertOneAsync(message);
        }

        public async Task UpdateAsync(string id, Message updatedMessage)
        {
            updatedMessage.Id = id;
            await _messages.ReplaceOneAsync(m => m.Id == id, updatedMessage);
        }

        public async Task DeleteAsync(string id)
        {
            await _messages.DeleteOneAsync(m => m.Id == id);
        }

        public async Task<List<Message>> GetConversationAsync(string userClientId, string doctorClientId)
        {
            var filter = Builders<Message>.Filter.And(
                Builders<Message>.Filter.Eq(m => m.UserClientId, userClientId),
                Builders<Message>.Filter.Eq(m => m.DoctorClientId, doctorClientId)
            );

            var list = await _messages.Find(filter).SortBy(m => m.SentAt).ToListAsync();
            return list;
        }

        public async Task<List<Message>> GetByConversationIdAsync(string conversationId)
        {
            var filter = Builders<Message>.Filter.Eq(m => m.ConversationId, conversationId);
            var list = await _messages.Find(filter).SortBy(m => m.SentAt).ToListAsync();
            return list;
        }

        public async Task<List<string>> GetDistinctUserIdsForDoctorAsync(string doctorClientId)
        {
            var filter = Builders<Message>.Filter.Eq(m => m.DoctorClientId, doctorClientId);
            return await _messages.Distinct<string>("UserClientId", filter).ToListAsync();
        }

        public async Task<List<string>> GetDistinctDoctorIdsForUserAsync(string userClientId)
        {
            var filter = Builders<Message>.Filter.Eq(m => m.UserClientId, userClientId);
            return await _messages.Distinct<string>("DoctorClientId", filter).ToListAsync();
        }
    }
}
