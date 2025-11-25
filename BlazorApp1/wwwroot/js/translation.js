window.fypTranslation = (function () {
    let currentLang = "en";
    let textNodes = [];
    let originalTexts = [];

    function collectTextNodes() {
        textNodes = [];
        originalTexts = [];

        const walker = document.createTreeWalker(
            document.body,
            NodeFilter.SHOW_TEXT,
            {
                acceptNode(node) {
                    const text = node.nodeValue;
                    if (!text) return NodeFilter.FILTER_REJECT;

                    if (!text.trim()) return NodeFilter.FILTER_REJECT;

                    const parent = node.parentElement;
                    if (!parent) return NodeFilter.FILTER_REJECT;

                    const tag = parent.tagName;
                    if (tag === "SCRIPT" || tag === "STYLE" || tag === "NOSCRIPT")
                        return NodeFilter.FILTER_REJECT;

                    return NodeFilter.FILTER_ACCEPT;
                }
            }
        );

        let n;
        while ((n = walker.nextNode())) {
            textNodes.push(n);
            originalTexts.push(n.nodeValue);
        }
    }

    async function setLanguage(lang) {
        if (lang === currentLang) return;

        if (textNodes.length === 0) {
            collectTextNodes();
        }

        // Back to English → just restore original text, no API call
        if (lang === "en") {
            for (let i = 0; i < textNodes.length; i++) {
                textNodes[i].nodeValue = originalTexts[i];
            }
            currentLang = "en";
            return;
        }

        const textsToTranslate = originalTexts;

        const response = await fetch("/api/translate", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                targetLanguage: lang,
                texts: textsToTranslate
            })
        });

        if (!response.ok) {
            console.error("Translation failed", await response.text());
            return;
        }

        const data = await response.json();
        const translated = data.texts || [];

        for (let i = 0; i < textNodes.length && i < translated.length; i++) {
            textNodes[i].nodeValue = translated[i];
        }

        currentLang = lang;
    }

    return {
        setLanguage
    };
})();
