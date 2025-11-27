using Microsoft.AspNetCore.SignalR;

namespace API.Hubs
{
    public class CallHub : Hub
    {
        // This method sends the peer ID to the other client
        public async Task SendPeerId(string peerId)
        {
            // Broadcast the peerId to other clients (peer-to-peer signaling)
            await Clients.Others.SendAsync("ReceivePeerId", peerId);
        }

        // This method handles signaling data (offer/answer/ICE candidates) from one client to another
        public async Task SendSignal(string peerId, string signalData)
        {
            // Send the signaling data to the specified peerId (other client)
            await Clients.Client(peerId).SendAsync("ReceiveSignal", signalData);
        }
    }
}
