let peer = null;
let localStream = null;
let remoteStream = null;
let currentCall = null;
let inCall = false;

// Set up the SignalR connection for signaling
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/callHub")
    .build();

// Start the SignalR connection
connection.start().then(() => {
    console.log("SignalR connected to callHub");
}).catch(err => {
    console.error("SignalR connection failed:", err);
});

// Function to initialize the peer connection
function initPeerConnection() {
    peer = new Peer(undefined, {
        host: "your-peer-server.com",  // Replace with your actual server
        port: 9000,
        path: "/peerjs"
    });

    peer.on("open", (id) => {
        console.log("Peer connection opened with ID: ", id);
        connection.invoke("SendPeerId", id); // Send the Peer ID to SignalR server
    });

    peer.on("call", (call) => {
        console.log("Incoming call from:", call.peer);
        call.answer(localStream);  // Answer the incoming call with the local stream
        call.on("stream", (stream) => {
            remoteStream = stream;
            document.getElementById("remoteAudio").srcObject = stream;
        });
    });

    peer.on("error", (err) => {
        console.error("Peer error:", err);
        alert("Error during call: " + err);
    });
}

// Function to get the local media stream (audio + video)
function getLocalStream() {
    navigator.mediaDevices.getUserMedia({ audio: true, video: true })
        .then((stream) => {
            localStream = stream;
            document.getElementById("remoteAudio").srcObject = stream;
        }).catch((err) => {
            console.log("Error getting media stream: ", err);
            alert("Error accessing media devices: " + err);
        });
}

// Start the call (doctor initiates)
function startCall(remotePeerId) {
    getLocalStream();  // Get the local media stream

    // Wait for peer to be initialized before starting the call
    setTimeout(() => {
        currentCall = peer.call(remotePeerId, localStream); // Make the call
        currentCall.on("stream", (stream) => {
            remoteStream = stream;
            document.getElementById("remoteAudio").srcObject = stream;
        });
    }, 1000);  // Slight delay to allow peer to initialize
}

// End the call
function endCall() {
    if (currentCall) {
        currentCall.close();
        inCall = false;
        document.getElementById("remoteAudio").srcObject = null;
        console.log("Call ended");
    }
}

// Function to handle the "Start Call" event (Button click)
function OnVoiceCall(remotePeerId) {
    if (!inCall) {
        startCall(remotePeerId);
        inCall = true;
    } else {
        endCall();
    }
}

// Function to handle the video call (Placeholder for future video support)
function OnVideoCall() {
    alert("Video call functionality will be added soon!");
}

