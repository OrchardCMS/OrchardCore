export interface DragHandlers {
    /**
     * Called once the pointer moved past the threshold.
     */
    onStart?(event: PointerEvent): void;
    /**
     * Called at most once per animation frame with the screen offset since the pointer went down.
     */
    onMove(dx: number, dy: number, event: PointerEvent): void;
    /**
     * Called when the pointer is released or the gesture is cancelled. `moved` is false for a click.
     */
    onEnd?(moved: boolean, event: PointerEvent | null): void;
    threshold?: number;
}

/**
 * Tracks a pointer gesture from `event` until it ends. Pointer moves are coalesced into one `onMove` per
 * animation frame, so a drag costs one state update (and one render) per frame. Returns a function that
 * cancels the gesture.
 */
export const startPointerDrag = (event: PointerEvent, handlers: DragHandlers): (() => void) => {
    const threshold = handlers.threshold ?? 3;
    const startX = event.clientX;
    const startY = event.clientY;
    let moved = false;
    let frame = 0;
    let latest: PointerEvent | null = null;

    const flush = () => {
        frame = 0;

        if (latest) {
            handlers.onMove(latest.clientX - startX, latest.clientY - startY, latest);
        }
    };

    const onMove = (moveEvent: PointerEvent) => {
        if (moveEvent.pointerId !== event.pointerId) {
            return;
        }

        if (!moved) {
            if (Math.hypot(moveEvent.clientX - startX, moveEvent.clientY - startY) < threshold) {
                return;
            }

            moved = true;
            handlers.onStart?.(moveEvent);
        }

        latest = moveEvent;

        if (!frame) {
            frame = requestAnimationFrame(flush);
        }
    };

    const cleanup = () => {
        window.removeEventListener("pointermove", onMove);
        window.removeEventListener("pointerup", onUp);
        window.removeEventListener("pointercancel", onUp);
    };

    function onUp(upEvent: PointerEvent) {
        if (upEvent.pointerId !== event.pointerId) {
            return;
        }

        cleanup();

        if (frame) {
            cancelAnimationFrame(frame);
            flush();
        }

        handlers.onEnd?.(moved, upEvent);
    }

    window.addEventListener("pointermove", onMove);
    window.addEventListener("pointerup", onUp);
    window.addEventListener("pointercancel", onUp);

    return () => {
        cleanup();

        if (frame) {
            cancelAnimationFrame(frame);
        }

        handlers.onEnd?.(moved, null);
    };
};
