// wwwroot/js/voiceRecorder.js

window.voiceRecorder = (function () {
    let mediaRecorder = null;
    let chunks = [];

    async function start() {
        if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
            alert("Audio recording is not supported in this browser.");
            return;
        }

        const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
        mediaRecorder = new MediaRecorder(stream);

        chunks = [];

        mediaRecorder.ondataavailable = function (e) {
            if (e.data && e.data.size > 0) {
                chunks.push(e.data);
            }
        };

        mediaRecorder.start();
    }

    function stop() {
        return new Promise((resolve, reject) => {
            if (!mediaRecorder) {
                resolve(null);
                return;
            }

            mediaRecorder.onstop = function () {
                const blob = new Blob(chunks, { type: "audio/webm" });

                const reader = new FileReader();
                reader.onloadend = function () {
                    // result looks like: data:audio/webm;codecs=opus;base64,AAAA...
                    resolve(reader.result);
                };
                reader.onerror = reject;

                reader.readAsDataURL(blob);
            };

            mediaRecorder.stop();
        });
    }

    return {
        start: start,
        stop: stop
    };
})();
