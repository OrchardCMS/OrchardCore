function addMapPicker() {

    const latElement = document.querySelector('[data-latitude]');
    const longElement = document.querySelector('[data-longitude]');

    const mapCenter = [40.866667, 34.566667];
    let zoom = 0;

    if (latElement.value && longElement.value) {
        mapCenter[0] = parseFloat(latElement.value);
        mapCenter[1] = parseFloat(longElement.value);
        zoom = 14;
    }

    const map = L.map('map', { center: mapCenter, zoom: zoom });
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);
    const marker = L.marker(mapCenter, { draggable: true }).addTo(map);

    function updateMarker(lat, lng) {
        marker
            .setLatLng([lat, lng])
            .bindPopup(`Location :  ${marker.getLatLng().toString()}`)
            .openPopup();
        return false;
    }

    map.on('click', function (e) {
        latElement.value = e.latlng.lat.toFixed(6);
        longElement.value = e.latlng.lng.toFixed(6);
        updateMarker(e.latlng.lat.toFixed(6), e.latlng.lng.toFixed(6));
    });

    const updateMarkerByInputs = function () {
        return updateMarker(latElement.value, longElement.value);
    }

    latElement.addEventListener('input', updateMarkerByInputs);
    longElement.addEventListener('input', updateMarkerByInputs);
}

document.addEventListener('DOMContentLoaded', addMapPicker);
