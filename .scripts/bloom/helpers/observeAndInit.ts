type InitFn = (element: HTMLElement) => void;

interface Registration {
    selector: string;
    initFn: InitFn;
    initialized: WeakSet<Element>;
}

interface ObserveAndInitState {
    registry: Registration[];
    observer: MutationObserver | null;
}

// Each view that calls `observeAndInit` is compiled into its own standalone Parcel bundle (every
// `Assets.json` entry is a separate, self-contained build with no shared runtime chunk between
// them), so this module's own top-level variables are NOT actually shared across views the way a
// single-file import would be - each bundle gets its own private copy. Anchoring the shared state
// on `window` instead makes the registry and the observer genuine page-wide singletons no matter
// how many separately-built views import this helper, which is the whole point of using one
// shared observer instead of one per widget type.
declare global {
    interface Window {
        __orchardCoreObserveAndInit?: ObserveAndInitState;
    }
}

const getState = (): ObserveAndInitState => {
    window.__orchardCoreObserveAndInit ??= { registry: [], observer: null };

    return window.__orchardCoreObserveAndInit;
};

const initExisting = (registration: Registration) => {
    document.querySelectorAll(registration.selector).forEach((element) => {
        if (!registration.initialized.has(element)) {
            registration.initialized.add(element);
            registration.initFn(element as HTMLElement);
        }
    });
};

const initMatchesWithin = (registration: Registration, node: Node) => {
    if (!(node instanceof Element)) {
        return;
    }

    if (node.matches(registration.selector) && !registration.initialized.has(node)) {
        registration.initialized.add(node);
        registration.initFn(node as HTMLElement);
    }

    node.querySelectorAll(registration.selector).forEach((element) => {
        if (!registration.initialized.has(element)) {
            registration.initialized.add(element);
            registration.initFn(element as HTMLElement);
        }
    });
};

// One shared observer for every registration, rather than one per widget type: MutationObserver
// delivery is already batched per task (not per mutation), so a single observer scanning the
// registry is far cheaper than N observers each independently re-scanning the same mutations.
const ensureObserverStarted = (state: ObserveAndInitState) => {
    if (state.observer) {
        return;
    }

    state.observer = new MutationObserver((mutations) => {
        for (const mutation of mutations) {
            mutation.addedNodes.forEach((node) => {
                for (const registration of state.registry) {
                    initMatchesWithin(registration, node);
                }
            });
        }
    });

    state.observer.observe(document.body, { childList: true, subtree: true });
};

// Initializes every element matching `selector` already in the document, then keeps calling
// `initFn` for any future element matching `selector` added later - including ones injected via
// AJAX (e.g. adding a Flow/Bag/Widgets-list widget) after this module's own top-level code, which
// only ever runs once per page, has already finished.
const observeAndInit = (selector: string, initFn: InitFn) => {
    const state = getState();
    const registration: Registration = { selector, initFn, initialized: new WeakSet() };
    state.registry.push(registration);

    initExisting(registration);
    ensureObserverStarted(state);
};

export default observeAndInit;
