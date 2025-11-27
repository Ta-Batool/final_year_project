let peer = null;
let currentCall = null;
let localStream = null;
let remoteStream = null;

// Initialize SignalR connection
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/callHub")
    .build();

connection.start().then(() => {
    console.log("SignalR connection established!");
}).catch(err => {
    console.error("SignalR connection failed:", err);
});

// Function to start a call
function startCall(remotePeerId) {
    localStream = getLocalStream();

    // Create a PeerJS connection
    peer = new Peer(undefined, {
        host: 'your-peer-server.com', 
        port: 9000, 
        path: '/peerjs'
    });

    peer.on('open', (id) => {
        console.log('Peer connected: ', id);

        // Send the peer ID via SignalR
        connection.invoke("SendPeerId", id);
    });

    // Make the call
    peer.on('call', (call) => {
        console.log('Receiving call from ' + call.peer);
        // Answer the call with the local stream
        call.answer(localStream);
        call.on('stream', (remoteStream) => {
            // Set the remote stream to the video element
            document.getElementById('remoteAudio').srcObject = remoteStream;
        });
    });

    // Call the peer
    peer.call(remotePeerId, localStream);
}

// Function to get local media stream (audio/video)
function getLocalStream() {
    navigator.mediaDevices.getUserMedia({ audio: true, video: true })
        .then((stream) => {
            document.getElementById('localAudio').srcObject = stream;
            return stream;
        }).catch((err) => {
            console.log("Failed to get local stream: ", err);
        });
}

// End the call
function endCall() {
    if (currentCall) {
        currentCall.close();
    }
}
