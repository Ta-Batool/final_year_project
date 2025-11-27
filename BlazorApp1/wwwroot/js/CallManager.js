window.callManager = (function () {
    let connection = null;      // SignalR connection
    let peer = null;            // RTCPeerConnection
    let localStream = null;
    let remoteAudio = null;
    let conversationId = null;
    let isCaller = false;
    let myClientId = null;

    async function init(hubUrl, convId, isInitiator, clientId, remoteAudioElementId) {
        conversationId = convId;
        isCaller = isInitiator;
        myClientId = clientId;
        remoteAudio = document.getElementById(remoteAudioElementId);

        // build SignalR connection
        connection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl)
            .withAutomaticReconnect()
            .build();

        registerHandlers();

        await connection.start();
        await connection.invoke("JoinConversation", conversationId);

        await setupPeer();

        if (isCaller) {
            await startOffer();
        }
    }

    function registerHandlers() {
        connection.on("ReceiveOffer", async (fromClientId, sdp) => {
            await peer.setRemoteDescription(new RTCSessionDescription({ type: "offer", sdp }));
            const answer = await peer.createAnswer();
            await peer.setLocalDescription(answer);
            await connection.invoke("SendAnswer", conversationId, myClientId, answer.sdp);
        });

        connection.on("ReceiveAnswer", async (fromClientId, sdp) => {
            await peer.setRemoteDescription(new RTCSessionDescription({ type: "answer", sdp }));
        });

        connection.on("ReceiveIceCandidate", async (fromClientId, candidate) => {
            try {
                await peer.addIceCandidate(new RTCIceCandidate(JSON.parse(candidate)));
            } catch (e) {
                console.error("Error adding ICE candidate", e);
            }
        });
    }

    async function setupPeer() {
        // audio only
        localStream = await navigator.mediaDevices.getUserMedia({ audio: true, video: false });

        peer = new RTCPeerConnection({
            iceServers: [
                { urls: "stun:stun.l.google.com:19302" }
            ]
        });

        localStream.getTracks().forEach(t => peer.addTrack(t, localStream));

        peer.onicecandidate = e => {
            if (e.candidate) {
                connection.invoke(
                    "SendIceCandidate",
                    conversationId,
                    myClientId,
                    JSON.stringify(e.candidate)
                );
            }
        };

        peer.ontrack = e => {
            const [stream] = e.streams;
            if (remoteAudio) {
                remoteAudio.srcObject = stream;
                remoteAudio.play().catch(() => { });
            }
        };
    }

    async function startOffer() {
        const offer = await peer.createOffer();
        await peer.setLocalDescription(offer);
        await connection.invoke("SendOffer", conversationId, myClientId, offer.sdp);
    }

    async function endCall() {
        try {
            if (peer) {
                peer.close();
                peer = null;
            }
            if (localStream) {
                localStream.getTracks().forEach(t => t.stop());
                localStream = null;
            }
            if (connection) {
                await connection.stop();
                connection = null;
            }
        } catch (e) {
            console.error("Error ending call", e);
        }
    }

    return {
        init,
        endCall
    };
})();
