function addMapPicker() {

    var latInput = document.querySelector('[data-latitude]');
    var longInput = document.querySelector('[data-longitude]');

    var lat = latInput.value;
    var long = longInput.value;

    var mapCenter = [40.866667, 34.566667];
    var zoom = 0;

    if (lat && long) {
        mapCenter = [lat, long];
        zoom = 14;
    }

    var map = L.map('map', { center: mapCenter, zoom: zoom });
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
    }).addTo(map);
    var marker = L.marker(mapCenter, { draggable: true }).addTo(map);

    function updateMarker(lat, lng) {
        marker
            .setLatLng([lat, lng])
            .bindPopup("Location :  " + marker.getLatLng().toString())
            .openPopup();
        return false;
    };

    map.on('click', function (e) {
        latInput.value = e.latlng.lat.toFixed(6);
        longInput.value = e.latlng.lng.toFixed(6);
        updateMarker(e.latlng.lat.toFixed(6), e.latlng.lng.toFixed(6));
    });


    var updateMarkerByInputs = function () {
        return updateMarker(latInput.value, longInput.value);
    }
    latInput.addEventListener('input', updateMarkerByInputs);
    longInput.addEventListener('input', updateMarkerByInputs);
}

document.addEventListener('DOMContentLoaded', function () {
    addMapPicker();
});
