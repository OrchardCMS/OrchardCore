// Browsers don't execute <script> tags inserted via innerHTML, so scripts are extracted
// server-side into a separate markup fragment and re-created here as real <script> elements,
// which do execute when appended to the document (mirrors jQuery's $.globalEval trick).
// The attribute-copy loop is intentionally generic so it also preserves attributes like
// `type="module"` or a future CSP `nonce` untouched.
const evalScripts = (html: string) => {
    const container = document.createElement("div");
    container.innerHTML = html;
    container.querySelectorAll("script").forEach((oldScript) => {
        const newScript = document.createElement("script");
        for (let i = 0; i < oldScript.attributes.length; i++) {
            newScript.setAttribute(oldScript.attributes[i].name, oldScript.attributes[i].value);
        }
        newScript.textContent = oldScript.textContent;
        document.body.appendChild(newScript);
    });
};

export default evalScripts;
