using Microsoft.AspNetCore.SignalR;

namespace API.Hubs
{
    public class CallHub : Hub
    {
        // Clients can send their peer ID to this method
        public async Task SendPeerId(string peerId)
        {
            // Send peer ID to the other client
            await Clients.Others.SendAsync("ReceivePeerId", peerId);
        }

        // Broadcast the signaling data to connected clients (offer, answer, ICE candidates)
        public async Task SendSignal(string peerId, string signalData)
        {
            // Send the signaling data to the specific peer
            await Clients.Client(peerId).SendAsync("ReceiveSignal", signalData);
        }
    }
}
