using Microsoft.AspNetCore.SignalR;

namespace API.Hubs
{
    public class CallHub : Hub
    {
        // conversationId could be doctorId_userId string
        public async Task JoinConversation(string conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
        }

        public async Task SendOffer(string conversationId, string fromClientId, string sdp)
        {
            await Clients.OthersInGroup(conversationId)
                .SendAsync("ReceiveOffer", fromClientId, sdp);
        }

        public async Task SendAnswer(string conversationId, string fromClientId, string sdp)
        {
            await Clients.OthersInGroup(conversationId)
                .SendAsync("ReceiveAnswer", fromClientId, sdp);
        }

        public async Task SendIceCandidate(string conversationId, string fromClientId, string candidate)
        {
            await Clients.OthersInGroup(conversationId)
                .SendAsync("ReceiveIceCandidate", fromClientId, candidate);
        }
    }
}
