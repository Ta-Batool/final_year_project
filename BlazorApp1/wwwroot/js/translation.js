window.fypTranslation = (function () {
    let currentLang = "en";

    // Keep the original English text per DOM node
    const originalsMap = new WeakMap();

    function collectTextNodes() {
        const nodes = [];
        const originals = [];

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
            nodes.push(n);

            let original = originalsMap.get(n);
            if (!original) {
                original = n.nodeValue;
                originalsMap.set(n, original);
            }

            originals.push(original);
        }

        return { nodes, originals };
    }

    async function setLanguage(lang) {
        // 🧠 Always rescan the DOM – pages may have changed
        const { nodes, originals } = collectTextNodes();

        // Back to English → restore originals only, no API call
        if (lang === "en") {
            for (let i = 0; i < nodes.length; i++) {
                nodes[i].nodeValue = originals[i];
            }
            currentLang = "en";
            return;
        }

        // Call our backend translate endpoint
        const response = await fetch("/api/translate", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                targetLanguage: lang,
                texts: originals
            })
        });

        if (!response.ok) {
            console.error("Translation failed", await response.text());
            return;
        }

        const data = await response.json();
        const translated = data.texts || [];

        for (let i = 0; i < nodes.length && i < translated.length; i++) {
            nodes[i].nodeValue = translated[i];
        }

        currentLang = lang;
    }

    return {
        setLanguage
    };
})();
