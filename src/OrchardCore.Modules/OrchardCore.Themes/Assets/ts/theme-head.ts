import { getPreferredTheme, setTheme } from "./constants";

// We need to apply the classes BEFORE the page is rendered. 
// That is why we use a MutationObserver instead of document.Ready().
const themeObserver = new MutationObserver(function (mutations) {

    for (let i = 0; i < mutations.length; i++) {
        for (let j = 0; j < mutations[i].addedNodes.length; j++) {

            const addedNode = mutations[i].addedNodes[j] as HTMLElement;
            if (addedNode.tagName === 'BODY') {
                setTheme(getPreferredTheme());

                // we're done: 
                themeObserver.disconnect();
            }
        }
    }
});

themeObserver.observe(document.documentElement, {
    childList: true,
    subtree: true
});
