import { onBeforeUnmount, ref } from "vue";

/**
 * How long the pointer stays on a rail before its pane opens, and away from the pane before it closes.
 */
export const PEEK_OPEN_DELAY = 150;
export const PEEK_CLOSE_DELAY = 300;

/**
 * The hover behavior of a collapsed side pane, like the admin menu in its compact mode: hovering its rail opens
 * the pane over the canvas, and it closes once the pointer leaves it. It stays open while the focus is in it
 * or while something holds it open, such as a drag from the toolbox.
 */
export const usePeek = (contains: (element: Element | null) => boolean) => {
    const open = ref(false);
    let timer: ReturnType<typeof setTimeout> | undefined;
    let pointerInside = false;
    let holds = 0;

    const closeIfIdle = () => {
        if (!pointerInside && holds === 0 && !contains(document.activeElement)) {
            open.value = false;
        }
    };

    const enter = () => {
        pointerInside = true;
        clearTimeout(timer);

        if (!open.value) {
            timer = setTimeout(() => (open.value = true), PEEK_OPEN_DELAY);
        }
    };

    const leave = () => {
        pointerInside = false;
        clearTimeout(timer);
        timer = setTimeout(closeIfIdle, PEEK_CLOSE_DELAY);
    };

    onBeforeUnmount(() => clearTimeout(timer));

    return {
        open,
        enter,
        leave,

        close() {
            clearTimeout(timer);
            open.value = false;
        },

        /**
         * Keeps the pane open until `release`, for example during a drag that leaves it.
         */
        hold() {
            holds++;
        },

        release() {
            holds = Math.max(0, holds - 1);

            if (!pointerInside) {
                leave();
            }
        },

        // The focus left the pane: close it unless the pointer is still over it.
        focusOut(event: FocusEvent) {
            if (!contains(event.relatedTarget as Element | null) && !pointerInside) {
                leave();
            }
        },
    };
};
