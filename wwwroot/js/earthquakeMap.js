window.earthquakeMap = {
    map: null,
    markersLayer: null,
    onSelectCallback: null,

    init: function () {
        if (this.map) return;

        this.map = L.map('earthquakeMap').setView([23.6345, -102.5528], 5);

        L.tileLayer('https://{s}.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}{r}.png', {
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OSM</a> &copy; <a href="https://carto.com/">CARTO</a>',
            maxZoom: 19
        }).addTo(this.map);

        this.markersLayer = L.layerGroup().addTo(this.map);
    },

    updateMarkers: function (quakes, dotnetRef) {
        if (!this.map) this.init();
        this.markersLayer.clearLayers();
        if (!quakes || quakes.length === 0) return;

        for (var i = 0; i < quakes.length; i++) {
            var q = quakes[i];
            var size = q.markerSize || 12;
            var color = q.severityColor || '#88cc00';

            var icon = L.divIcon({
                html: '<div class="eq-marker" style="' +
                    'width:' + size + 'px;height:' + size + 'px;' +
                    'background:' + color + ';' +
                    'border-radius:50%;' +
                    'border:2px solid rgba(255,255,255,0.8);' +
                    'box-shadow:0 0 ' + (size / 2) + 'px ' + color + ';' +
                    'opacity:0.85;' +
                    (q.magnitude >= 4 ? 'animation:eqPulse 2s infinite;' : '') +
                    '"></div>',
                className: 'eq-icon',
                iconSize: [size, size],
                iconAnchor: [size / 2, size / 2],
                popupAnchor: [0, -size / 2]
            });

            var timeStr = new Date(q.time).toLocaleString();
            var marker = L.marker([q.lat, q.lon], { icon: icon })
                .bindPopup(
                    '<div style="min-width:240px;font-family:sans-serif">' +
                    '<h4 style="margin:0 0 8px 0;color:' + color + '">🔴 M ' + q.magnitude.toFixed(1) + '</h4>' +
                    '<p style="margin:4px 0"><strong>📍</strong> ' + q.place + '</p>' +
                    '<p style="margin:4px 0"><strong>⏰</strong> ' + timeStr + '</p>' +
                    '<p style="margin:4px 0"><strong>📏 Profundidad:</strong> ' + q.depth.toFixed(1) + ' km</p>' +
                    (q.felt ? '<p style="margin:4px 0"><strong>👥 Sentido por:</strong> ' + q.felt + ' personas</p>' : '') +
                    (q.tsunami ? '<p style="margin:4px 0;color:#ff0000;font-weight:bold">🌊 ¡Alerta de tsunami!</p>' : '') +
                    (q.url ? '<p style="margin:4px 0"><a href="' + q.url + '" target="_blank">🔗 Más detalles (USGS)</a></p>' : '') +
                    '</div>'
                );

            // Click to select in panel
            (function (quake, idx) {
                marker.on('click', function () {
                    if (dotnetRef) {
                        dotnetRef.invokeMethodAsync('SelectEarthquake', idx);
                    }
                });
            })(q, i);

            this.markersLayer.addLayer(marker);
        }
    },

    focusQuake: function (lat, lon, mag) {
        if (this.map) {
            var zoom = mag >= 5 ? 8 : mag >= 3 ? 10 : 12;
            this.map.setView([lat, lon], zoom);
        }
    },

    dispose: function () {
        if (this.map) {
            this.map.remove();
            this.map = null;
        }
    }
};
