# 📖 Monitor del Popocatépetl — Fuentes de Datos

## Investigación del equipo

---

## 1. 🇺🇸 USGS Earthquake API (Sismicidad Cercana)

### Uso
Consulta de sismos en un radio de ~50 km alrededor del Popocatépetl para detectar actividad volcánico-tectónica.

### Endpoint
```
GET https://earthquake.usgs.gov/fdsnws/event/1/query?format=geojson
  &minlatitude=18.5&maxlatitude=19.6
  &minlongitude=-99.2&maxlongitude=-98.0
  &orderby=time&limit=100
```

### Datos
- Magnitud, profundidad, coordenadas, timestamp
- Formato: GeoJSON
- **Gratuito, sin API key**
- Actualización: ~1 minuto

---

## 2. 🇲🇽 CENAPRED — Centro Nacional de Prevención de Desastres

### Descripción
Fuente oficial de monitoreo volcánico en México. Publica reportes diarios del Popocatépetl.

### Recursos Disponibles

| Recurso | URL | Formato |
|---------|-----|---------|
| Reportes diarios | https://www.cenapred.gob.mx/es/reportesVolcanesMX/ | PDF |
| Semáforo volcánico | https://www.gob.mx/cenapred/articulos/semaforo-de-alerta-volcanica-220744 | HTML |
| Webcam Tlamacas HD | https://www1.cenapred.unam.mx/volcan/popocatepetl/imagen/tlamacas_hd.jpg | JPEG |
| Estación Colibri | https://www.cenapred.unam.mx/es/RegistrosVolcanPopo/colibri/index.php | HTML |
| Monitoreo general | https://www1.cenapred.unam.mx/es/RegistrosVolcanPopo/ | HTML |

### Limitaciones
- **No hay API REST oficial** — Datos en PDF y HTML
- Webcam actualiza cada ~1 minuto (imagen estática)
- Para automatización se requiere scraping o herramientas comunitarias

### Herramienta Comunitaria
- **PopoCLI:** https://github.com/KyleEdwardDonaldson/PopoCLI
- CLI para obtener datos de CENAPRED sobre el Popocatépetl

---

## 3. 📹 Webcams en Vivo

| Cámara | Fuente | URL | Tipo |
|--------|--------|-----|------|
| Tlamacas HD | CENAPRED | `tlamacas_hd.jpg` | Imagen estática (1 min) |
| Tlamacas Video | YouTube | `youtube.com/watch?v=dq5LX5lq-1Y` | Stream en vivo |
| Amecameca | Webcams de México | webcamsdemexico.com/webcam/popocatepetl-amecameca/ | Stream en vivo |
| SkylineWebcams | SkylineWebcams | skylinewebcams.com/.../volcano-popocatepetl.html | Stream HD |

---

## 4. 🌍 Smithsonian Global Volcanism Program

### Descripción
Perfil completo del Popocatépetl con historial eruptivo y reportes semanales.

### Recursos
- Perfil volcán: https://volcano.si.edu/volcano.cfm?vn=341090
- Reportes semanales: https://volcano.si.edu/reports_weekly.cfm
- GVP Number: **341090**

### USGS Volcano API
```
GET https://volcanoes.usgs.gov/vsc/api/volcanoApi/volcanoStationPlots/341090
```

---

## 5. 🚦 Semáforo de Alerta Volcánica

| Color | Fase | Significado |
|-------|------|-------------|
| 🟢 Verde | — | Actividad normal |
| 🟡 Amarillo | Fase 1 | Actividad sobre lo normal |
| 🟡 Amarillo | Fase 2 | Actividad intermedia, preparativos |
| 🟡 Amarillo | Fase 3 | Actividad alta, preparar evacuación |
| 🔴 Rojo | Fase 1 | Explosiones peligrosas |
| 🔴 Rojo | Fase 2 | Evacuación inmediata |

### Métricas de Actividad Monitoreadas
- **Exhalaciones:** Emisiones de vapor, gas y ceniza
- **Tremor volcánico:** Vibración continua (medida en minutos)
- **Explosiones:** Eventos explosivos visibles
- **Eventos vulcano-tectónicos (V-T):** Sismos asociados a fractura de roca por magma

---

## 📊 Arquitectura de Datos Implementada

```
USGS API (sismos cercanos) ──> VolcanoService (polling 10 min)
CENAPRED (reportes PDF)    ──>    ├── Métricas de actividad
                                   ├── Semáforo volcánico (calculado)
                                   └── SignalR push → Dashboard Blazor
                                         ├── Mapa Leaflet (satélite)
                                         ├── Webcam YouTube embed
                                         └── Panel de sismos recientes
```
