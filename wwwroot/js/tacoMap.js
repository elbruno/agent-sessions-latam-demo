window.tacoMap = {
    map: null,
    markersLayer: null,

    init: function () {
        if (this.map) return;

        this.map = L.map('tacoMap').setView([23.6345, -102.5528], 5);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>',
            maxZoom: 19
        }).addTo(this.map);

        this.markersLayer = L.layerGroup().addTo(this.map);
    },

    updateMarkers: function (stands) {
        if (!this.map) this.init();
        this.markersLayer.clearLayers();
        if (!stands || stands.length === 0) return;

        var tacoEmojis = {
            'pastor': '🌮', 'suadero': '🥩', 'birria': '🍖', 'carnitas': '🐷',
            'cabeza': '🐮', 'barbacoa': '🔥', 'canasta': '🧺', 'pescado': '🐟',
            'campechano': '🌯', 'variado': '🌮'
        };

        var bounds = [];

        for (var i = 0; i < stands.length; i++) {
            var s = stands[i];
            var emoji = tacoEmojis[s.tacoType] || '🌮';
            var lateClass = s.isOpenLateNight ? 'late-night' : 'day-time';

            var icon = L.divIcon({
                html: '<div class="taco-marker ' + lateClass + '">' + emoji + '</div>',
                className: 'taco-icon',
                iconSize: [36, 36],
                iconAnchor: [18, 18],
                popupAnchor: [0, -20]
            });

            var marker = L.marker([s.lat, s.lon], { icon: icon })
                .bindPopup(
                    '<div style="min-width:220px">' +
                    '<h4 style="margin:0 0 8px 0">' + emoji + ' ' + s.name + '</h4>' +
                    '<p style="margin:4px 0"><strong>Tipo:</strong> ' + s.tacoType + '</p>' +
                    '<p style="margin:4px 0"><strong>Horario:</strong> ' + (s.openingHours || 'No disponible') + '</p>' +
                    (s.isOpenLateNight ? '<p style="margin:4px 0;color:#ff6600;font-weight:bold">🌙 ¡Abierto después de las 11 PM!</p>' : '') +
                    '<p style="margin:4px 0"><strong>Votos:</strong> ' + s.communityVotes + ' 👍</p>' +
                    (s.address ? '<p style="margin:4px 0;font-size:0.85em;color:#666">' + s.address + '</p>' : '') +
                    '<p style="margin:4px 0;font-size:0.85em;color:#999">Coords: ' + s.lat.toFixed(5) + ', ' + s.lon.toFixed(5) + '</p>' +
                    '</div>'
                );
            this.markersLayer.addLayer(marker);
            bounds.push([s.lat, s.lon]);
        }

        if (bounds.length > 0) {
            this.map.fitBounds(bounds, { padding: [50, 50], maxZoom: 14 });
        }
    },

    focusCity: function (lat, lon) {
        if (this.map) {
            this.map.setView([lat, lon], 12);
        }
    },

    dispose: function () {
        if (this.map) {
            this.map.remove();
            this.map = null;
        }
    }
};
