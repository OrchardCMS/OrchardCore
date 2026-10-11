import type { Point, Rect } from "./geometry";

// The viewport maps canvas coordinates to the screen: screen = canvas * zoom + pan. It is applied as a
// single CSS transform on the layer that holds both the nodes and the edges.

export interface ViewportState {
    panX: number;
    panY: number;
    zoom: number;
}

export interface Size {
    width: number;
    height: number;
}

export const MIN_ZOOM = 0.25;
export const MAX_ZOOM = 2;
export const ZOOM_STEP = 1.2;

export const clampZoom = (zoom: number) => Math.min(MAX_ZOOM, Math.max(MIN_ZOOM, Number.isFinite(zoom) ? zoom : 1));

export const screenToCanvas = (viewport: ViewportState, point: Point): Point => ({
    x: (point.x - viewport.panX) / viewport.zoom,
    y: (point.y - viewport.panY) / viewport.zoom,
});

export const canvasToScreen = (viewport: ViewportState, point: Point): Point => ({
    x: point.x * viewport.zoom + viewport.panX,
    y: point.y * viewport.zoom + viewport.panY,
});

/**
 * Zooms to `zoom` (clamped) while keeping the canvas point under `anchor` (a screen point relative to the
 * canvas element) where it is.
 */
export const zoomAt = (viewport: ViewportState, zoom: number, anchor: Point): ViewportState => {
    const next = clampZoom(zoom);
    const ratio = next / viewport.zoom;

    return {
        zoom: next,
        panX: anchor.x - (anchor.x - viewport.panX) * ratio,
        panY: anchor.y - (anchor.y - viewport.panY) * ratio,
    };
};

export const zoomBy = (viewport: ViewportState, factor: number, anchor: Point) => zoomAt(viewport, viewport.zoom * factor, anchor);

/**
 * The viewport that shows `bounds` centered in `size`, with a margin, never zooming in beyond `maxZoom`.
 */
export const fitToContent = (bounds: Rect | null, size: Size, padding = 40, maxZoom = 1): ViewportState => {
    if (!bounds || size.width <= 0 || size.height <= 0) {
        return { panX: padding, panY: padding, zoom: 1 };
    }

    const availableWidth = Math.max(1, size.width - padding * 2);
    const availableHeight = Math.max(1, size.height - padding * 2);
    const zoom = clampZoom(Math.min(maxZoom, availableWidth / Math.max(1, bounds.width), availableHeight / Math.max(1, bounds.height)));

    return {
        zoom,
        panX: (size.width - bounds.width * zoom) / 2 - bounds.x * zoom,
        panY: (size.height - bounds.height * zoom) / 2 - bounds.y * zoom,
    };
};

/**
 * The canvas point at the center of the visible area, where a clicked toolbox step is added.
 */
export const visibleCenter = (viewport: ViewportState, size: Size): Point =>
    screenToCanvas(viewport, { x: size.width / 2, y: size.height / 2 });
