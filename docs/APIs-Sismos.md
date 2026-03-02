# 📖 Guía de APIs para Sismos en Tiempo Real

## Investigación del equipo "Sísmica" — Agente Mercalli

---

## 1. 🇺🇸 USGS Earthquake Hazards Program (PRINCIPAL)

### Descripción
El Servicio Geológico de Estados Unidos (USGS) proporciona datos sísmicos globales en tiempo real a través de feeds GeoJSON públicos y gratuitos.

### Nivel de Acceso
- **100% gratuito** — No requiere API key ni registro
- Formato: **GeoJSON** (estándar para datos geoespaciales)
- Actualización: Cada minuto
- Cobertura: **Global** (incluye México completo)

### Endpoints Disponibles

| Feed | URL | Actualización |
|------|-----|---------------|
| Todos - última hora | `https://earthquake.usgs.gov/earthquakes/feed/v1.0/summary/all_hour.geojson` | 1 min |
| Todos - último día | `https://earthquake.usgs.gov/earthquakes/feed/v1.0/summary/all_day.geojson` | 1 min |
| Todos - última semana | `https://earthquake.usgs.gov/earthquakes/feed/v1.0/summary/all_week.geojson` | 1 min |
| Todos - último mes | `https://earthquake.usgs.gov/earthquakes/feed/v1.0/summary/all_month.geojson` | 15 min |
| M4.5+ - última semana | `https://earthquake.usgs.gov/earthquakes/feed/v1.0/summary/4.5_week.geojson` | 1 min |
| Significativos - semana | `https://earthquake.usgs.gov/earthquakes/feed/v1.0/summary/significant_week.geojson` | 1 min |

### Endpoint de Consulta Personalizada (Query API)
```
GET https://earthquake.usgs.gov/fdsnws/event/1/query?format=geojson&starttime=2026-01-01&endtime=2026-03-01&minmagnitude=4&minlatitude=14&maxlatitude=33&minlongitude=-118&maxlongitude=-86
```
Parámetros para filtrar solo México con magnitud ≥ 4.

### Estructura de Datos GeoJSON

```json
{
  "type": "FeatureCollection",
  "metadata": {
    "generated": 1772468222000,
    "url": "...",
    "title": "USGS All Earthquakes, Past Day",
    "count": 250
  },
  "features": [
    {
      "type": "Feature",
      "properties": {
        "mag": 5.2,
        "place": "10km SSW of Oaxaca, Mexico",
        "time": 1433621545500,
        "updated": 1433621823000,
        "felt": 150,
        "alert": "green",
        "tsunami": 0,
        "sig": 416,
        "magType": "mww",
        "type": "earthquake",
        "title": "M 5.2 - 10km SSW of Oaxaca, Mexico",
        "url": "https://earthquake.usgs.gov/earthquakes/eventpage/..."
      },
      "geometry": {
        "type": "Point",
        "coordinates": [-96.7266, 17.0732, 15.5]
      },
      "id": "us2026xxxx"
    }
  ]
}
```

### Campos Clave

| Campo | Tipo | Descripción |
|-------|------|-------------|
| `mag` | double | Magnitud del sismo |
| `place` | string | Ubicación descriptiva |
| `time` | long | Timestamp Unix en milisegundos |
| `coordinates[0]` | double | Longitud |
| `coordinates[1]` | double | Latitud |
| `coordinates[2]` | double | Profundidad en km |
| `felt` | int? | Número de personas que lo sintieron |
| `alert` | string? | Nivel de alerta (green/yellow/orange/red) |
| `tsunami` | int | 1 si hay alerta de tsunami |
| `sig` | int | Significancia (0-1000) |
| `magType` | string | Tipo de magnitud (ml, mb, mww, etc.) |

### Documentación Oficial
- Feeds: https://earthquake.usgs.gov/earthquakes/feed/
- GeoJSON Format: https://earthquake.usgs.gov/earthquakes/feed/v1.0/geojson.php
- Query API: https://earthquake.usgs.gov/fdsnws/event/1/

---

## 2. 🇲🇽 Servicio Sismológico Nacional (SSN - UNAM)

### Descripción
El SSN es la fuente oficial de datos sísmicos en México, operado por la UNAM.

### Nivel de Acceso
- **Gratuito** — No requiere API key
- Formato: **RSS/XML**
- No tiene API REST oficial con JSON
- Actualización: Cuasi tiempo real

### Feed RSS
```
GET http://www.ssn.unam.mx/rss/ultimos-sismos.xml
```

### Datos Disponibles
- Magnitud
- Ubicación (descripción textual)
- Coordenadas (latitud/longitud via GeoRSS)
- Fecha y hora
- Profundidad (en descripción)

### Catálogo Web (No Programático)
- Últimos sismos: http://www.ssn.unam.mx/sismicidad/ultimos/
- Catálogo histórico: http://www.ssn.unam.mx/sismicidad/catalogo/

### Limitaciones
- Sin endpoint JSON directo
- Requiere parsing de XML/RSS
- Menor frecuencia de actualización que USGS
- No incluye `felt` ni alertas de tsunami

---

## 3. 📊 Otras Fuentes

### EMSC (European-Mediterranean Seismological Centre)
- Feed: `https://www.emsc-csem.org/service/rss/rss.php?typ=emsc`
- Formato: RSS
- Cobertura: Global (incluye México)
- Gratuito, sin API key

### SASSLA (API Comunitaria México)
- GitHub: https://github.com/sassla/Historical-Records-API
- Estado: Experimental / prueba de concepto
- Requiere autenticación

---

## 📊 Tabla Comparativa

| Fuente | Formato | API Key | Cobertura MX | Tiempo Real | Datos Extra |
|--------|---------|---------|--------------|-------------|-------------|
| **USGS** | GeoJSON | ❌ No | ✅ Excelente | ✅ 1 min | felt, tsunami, alert |
| **SSN** | RSS/XML | ❌ No | ✅ Oficial MX | ⚠️ ~5 min | Básico |
| **EMSC** | RSS | ❌ No | ⚠️ Parcial | ⚠️ ~10 min | Básico |

### Recomendación de Mercalli
1. **Usar USGS como fuente principal** — GeoJSON nativo, datos ricos, global, sin límites
2. **Complementar con SSN** — Fuente oficial mexicana, puede tener sismos locales menores
3. **Filtrar por bounding box de México** — lat: 14-33, lon: -118 a -86
