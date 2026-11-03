import { describe, expect, it } from "vitest";
import { MAX_ZOOM, MIN_ZOOM, canvasToScreen, clampZoom, fitToContent, screenToCanvas, visibleCenter, zoomAt, zoomBy } from "../viewport";

describe("viewport", () => {
    it("clampZoom_OutOfRange_ClampsTo25And200Percent", () => {
        expect(clampZoom(0.1)).toBe(MIN_ZOOM);
        expect(clampZoom(5)).toBe(MAX_ZOOM);
        expect(clampZoom(Number.NaN)).toBe(1);
        expect(MIN_ZOOM).toBe(0.25);
        expect(MAX_ZOOM).toBe(2);
    });

    it("screenToCanvas_RoundTrip_ReturnsOriginalPoint", () => {
        const viewport = { panX: 30, panY: -20, zoom: 1.5 };
        const point = { x: 120, y: 80 };

        expect(canvasToScreen(viewport, screenToCanvas(viewport, point))).toEqual(point);
        expect(screenToCanvas(viewport, { x: 30, y: -20 })).toEqual({ x: 0, y: 0 });
    });

    it("zoomAt_Cursor_KeepsCanvasPointUnderCursor", () => {
        const viewport = { panX: 40, panY: 10, zoom: 1 };
        const cursor = { x: 300, y: 200 };
        const before = screenToCanvas(viewport, cursor);

        const zoomed = zoomAt(viewport, 1.6, cursor);

        expect(zoomed.zoom).toBe(1.6);
        expect(screenToCanvas(zoomed, cursor).x).toBeCloseTo(before.x);
        expect(screenToCanvas(zoomed, cursor).y).toBeCloseTo(before.y);
    });

    it("zoomBy_BeyondLimit_ClampsAndStillAnchors", () => {
        const viewport = { panX: 0, panY: 0, zoom: 1.8 };
        const cursor = { x: 100, y: 100 };

        const zoomed = zoomBy(viewport, 2, cursor);

        expect(zoomed.zoom).toBe(MAX_ZOOM);
        expect(screenToCanvas(zoomed, cursor).x).toBeCloseTo(screenToCanvas(viewport, cursor).x);
    });

    it("fitToContent_LargeGraph_ZoomsOutAndCenters", () => {
        const bounds = { x: 100, y: 50, width: 2000, height: 500 };
        const size = { width: 1080, height: 600 };

        const fitted = fitToContent(bounds, size, 40);

        expect(fitted.zoom).toBeCloseTo(0.5);
        // The content's center is at the viewport's center.
        const center = screenToCanvas(fitted, { x: size.width / 2, y: size.height / 2 });
        expect(center.x).toBeCloseTo(1100);
        expect(center.y).toBeCloseTo(300);
    });

    it("fitToContent_SmallGraph_DoesNotZoomInBeyond100Percent", () => {
        const fitted = fitToContent({ x: 0, y: 0, width: 200, height: 100 }, { width: 1000, height: 800 });

        expect(fitted.zoom).toBe(1);
        expect(fitted.panX).toBe(400);
        expect(fitted.panY).toBe(350);
    });

    it("fitToContent_NoContent_ReturnsDefaultViewport", () => {
        expect(fitToContent(null, { width: 800, height: 600 })).toEqual({ panX: 40, panY: 40, zoom: 1 });
    });

    it("fitToContent_HugeGraph_ClampsToMinimumZoom", () => {
        expect(fitToContent({ x: 0, y: 0, width: 100000, height: 100 }, { width: 800, height: 600 }).zoom).toBe(MIN_ZOOM);
    });

    it("visibleCenter_PannedAndZoomed_ReturnsCanvasPoint", () => {
        expect(visibleCenter({ panX: -100, panY: 50, zoom: 2 }, { width: 800, height: 600 })).toEqual({ x: 250, y: 125 });
    });
});
