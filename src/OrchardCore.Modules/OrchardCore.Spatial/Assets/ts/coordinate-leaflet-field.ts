import observeAndInit from "@orchardcore/bloom/helpers/observeAndInit";

// Minimal typings for the parts of the Leaflet global (loaded by the "leaflet" resource) used here.
interface LeafletLatLng {
    lat: number;
    lng: number;
}

interface LeafletMarker {
    addTo(map: LeafletMap): LeafletMarker;
    setLatLng(latLng: [number, number]): LeafletMarker;
    getLatLng(): LeafletLatLng;
    bindPopup(content: string): LeafletMarker;
    openPopup(): LeafletMarker;
}

interface LeafletMap {
    on(type: "click", fn: (e: { latlng: LeafletLatLng }) => void): LeafletMap;
    invalidateSize(): LeafletMap;
}

interface Leaflet {
    map(element: HTMLElement, options: { center: [number, number]; zoom: number }): LeafletMap;
    tileLayer(url: string, options: Record<string, string>): { addTo(map: LeafletMap): void };
    marker(latLng: [number, number], options: { draggable: boolean }): LeafletMarker;
}

declare const L: Leaflet;

// Initializes each editor on its own, including the ones added later, e.g. in a Flow or Bag part.
observeAndInit("[data-leaflet-editor]", (editor) => {
    const latInput = editor.querySelector<HTMLInputElement>("[data-latitude]");
    const longInput = editor.querySelector<HTMLInputElement>("[data-longitude]");
    const mapElement = editor.querySelector<HTMLElement>("[data-leaflet-map]");

    if (!latInput || !longInput || !mapElement) {
        return;
    }

    let mapCenter: [number, number] = [40.866667, 34.566667];
    let zoom = 0;

    if (latInput.value && longInput.value) {
        mapCenter = [Number(latInput.value), Number(longInput.value)];
        zoom = 14;
    }

    const map = L.map(mapElement, { center: mapCenter, zoom: zoom });
    L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
        referrerPolicy: "strict-origin-when-cross-origin",
    }).addTo(map);
    const marker = L.marker(mapCenter, { draggable: true }).addTo(map);

    const updateMarker = (lat: string, lng: string) => {
        marker
            .setLatLng([Number(lat), Number(lng)])
            .bindPopup("Location :  " + marker.getLatLng().toString())
            .openPopup();
    };

    map.on("click", (e) => {
        latInput.value = e.latlng.lat.toFixed(6);
        longInput.value = e.latlng.lng.toFixed(6);
        updateMarker(latInput.value, longInput.value);
    });

    const updateMarkerByInputs = () => updateMarker(latInput.value, longInput.value);
    latInput.addEventListener("input", updateMarkerByInputs);
    longInput.addEventListener("input", updateMarkerByInputs);

    // The map can be initialized while hidden (e.g. in a collapsed widget or an inactive tab), so its
    // size needs to be recomputed once it becomes visible.
    new ResizeObserver(() => map.invalidateSize()).observe(mapElement);
});
