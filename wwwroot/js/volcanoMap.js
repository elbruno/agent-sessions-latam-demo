window.volcanoMap = {
    map: null,
    markersLayer: null,
    volcanoMarker: null,
    dangerZone: null,

    init: function () {
        if (this.map) return;

        this.map = L.map('volcanoMap').setView([19.0225, -98.6278], 10);

        L.tileLayer('https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}', {
            attribution: '&copy; Esri',
            maxZoom: 18
        }).addTo(this.map);

        this.markersLayer = L.layerGroup().addTo(this.map);

        // Popocatépetl volcano marker
        var volcanoIcon = L.divIcon({
            html: '<div class="volcano-icon">🌋</div>',
            className: 'volcano-marker',
            iconSize: [50, 50],
            iconAnchor: [25, 25]
        });

        this.volcanoMarker = L.marker([19.0225, -98.6278], { icon: volcanoIcon })
            .bindPopup(
                '<div style="text-align:center;min-width:200px">' +
                '<h3 style="margin:0">🌋 Popocatépetl</h3>' +
                '<p style="margin:4px 0">Elevación: 5,426 m</p>' +
                '<p style="margin:4px 0">Coords: 19.0225°N, 98.6278°W</p>' +
                '<p style="margin:4px 0;font-style:italic">"Montaña que humea"</p>' +
                '</div>'
            )
            .addTo(this.map);

        // Danger zones (approximate)
        L.circle([19.0225, -98.6278], {
            radius: 12000,
            color: '#ff0000',
            fillColor: '#ff0000',
            fillOpacity: 0.1,
            weight: 2,
            dashArray: '5,10'
        }).bindTooltip('Zona de exclusión (12 km)').addTo(this.map);

        L.circle([19.0225, -98.6278], {
            radius: 25000,
            color: '#ff9900',
            fillColor: '#ff9900',
            fillOpacity: 0.05,
            weight: 1,
            dashArray: '10,10'
        }).bindTooltip('Zona de alerta (25 km)').addTo(this.map);
    },

    updateQuakes: function (quakes) {
        if (!this.map) this.init();
        this.markersLayer.clearLayers();
        if (!quakes || quakes.length === 0) return;

        for (var i = 0; i < quakes.length; i++) {
            var q = quakes[i];
            var size = Math.max(8, q.magnitude * 6);
            var color = q.magnitude >= 4 ? '#ff0000' : q.magnitude >= 2.5 ? '#ff9900' : '#ffdd00';

            var icon = L.divIcon({
                html: '<div style="width:' + size + 'px;height:' + size + 'px;background:' + color +
                    ';border-radius:50%;border:1px solid white;opacity:0.8;box-shadow:0 0 6px ' + color + '"></div>',
                className: 'eq-nearby',
                iconSize: [size, size],
                iconAnchor: [size / 2, size / 2]
            });

            var timeStr = new Date(q.time).toLocaleString();
            L.marker([q.lat, q.lon], { icon: icon })
                .bindPopup(
                    '<div style="min-width:180px">' +
                    '<h4 style="margin:0;color:' + color + '">M ' + q.magnitude.toFixed(1) + '</h4>' +
                    '<p style="margin:2px 0">' + q.place + '</p>' +
                    '<p style="margin:2px 0">Profundidad: ' + q.depth.toFixed(1) + ' km</p>' +
                    '<p style="margin:2px 0;color:#888">' + timeStr + '</p>' +
                    '</div>'
                )
                .addTo(this.markersLayer);
        }
    },

    dispose: function () {
        if (this.map) { this.map.remove(); this.map = null; }
    }
};
