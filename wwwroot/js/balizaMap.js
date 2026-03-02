window.balizaMap = {
    map: null,
    markersLayer: null,

    init: function () {
        if (this.map) return;

        this.map = L.map('balizaMap').setView([40.4168, -3.7038], 6);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
            maxZoom: 19
        }).addTo(this.map);

        this.markersLayer = L.layerGroup().addTo(this.map);
    },

    updateMarkers: function (balizas) {
        if (!this.map) this.init();

        this.markersLayer.clearLayers();

        if (!balizas || balizas.length === 0) return;

        var beaconIcon = L.divIcon({
            html: '<div style="background:#ff6600;width:24px;height:24px;border-radius:50%;border:3px solid #fff;box-shadow:0 0 10px rgba(255,102,0,0.8);animation:pulse 1.5s infinite"></div>',
            className: 'beacon-icon',
            iconSize: [24, 24],
            iconAnchor: [12, 12],
            popupAnchor: [0, -15]
        });

        var bounds = [];

        for (var i = 0; i < balizas.length; i++) {
            var b = balizas[i];
            var marker = L.marker([b.lat, b.lon], { icon: beaconIcon })
                .bindPopup(
                    '<div style="min-width:200px">' +
                    '<h4 style="margin:0 0 8px 0;color:#ff6600">⚠️ Baliza V16</h4>' +
                    '<p style="margin:4px 0"><strong>Tipo:</strong> ' + b.type + '</p>' +
                    '<p style="margin:4px 0"><strong>Ubicación:</strong> ' + b.location + '</p>' +
                    '<p style="margin:4px 0"><strong>Hora:</strong> ' + b.time + '</p>' +
                    '<p style="margin:4px 0;font-size:0.85em;color:#666">Coords: ' + b.lat.toFixed(5) + ', ' + b.lon.toFixed(5) + '</p>' +
                    '</div>'
                );
            this.markersLayer.addLayer(marker);
            bounds.push([b.lat, b.lon]);
        }

        if (bounds.length > 0) {
            this.map.fitBounds(bounds, { padding: [50, 50], maxZoom: 12 });
        }
    },

    dispose: function () {
        if (this.map) {
            this.map.remove();
            this.map = null;
        }
    }
};
