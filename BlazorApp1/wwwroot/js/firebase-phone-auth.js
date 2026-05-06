let confirmationResult = null;
let recaptchaVerifier = null;
let recaptchaWidgetId = null;

window.sendOtp = async function (phoneNumber) {
    try {
        const firebaseConfig = {
            apiKey: "AIzaSyCbdKryMdUZv8H0XVHNZAX6NVe-x_O-P_A",
            authDomain: "nutrition-f3f0b.firebaseapp.com",
            projectId: "nutrition-f3f0b",
            storageBucket: "nutrition-f3f0b.firebasestorage.app",
            messagingSenderId: "1089956001537",
            appId: "1:1089956001537:web:4feafacc3d5ecdc7444d00"
        };

        // 🔥 FORCE INIT INSIDE FUNCTION (fixes your error)
        if (!firebase.apps.length) {
            firebase.initializeApp(firebaseConfig);
        }

        const auth = firebase.auth();

        if (!recaptchaVerifier) {
            recaptchaVerifier = new firebase.auth.RecaptchaVerifier(
                "recaptcha-container",
                { size: "invisible" }
            );

            recaptchaWidgetId = await recaptchaVerifier.render();
        } else {
            grecaptcha.reset(recaptchaWidgetId);
        }

        confirmationResult = await auth.signInWithPhoneNumber(
            phoneNumber,
            recaptchaVerifier
        );

        return true;

    } catch (error) {
        console.error("Send OTP error:", error);
        alert(error.message);
        throw error;
    }
};

window.verifyOtp = async function (code) {
    try {
        if (!confirmationResult) {
            throw new Error("OTP not requested.");
        }

        const result = await confirmationResult.confirm(code);
        return result.user.phoneNumber;

    } catch (error) {
        console.error("Verify OTP error:", error);
        throw error;
    }
};