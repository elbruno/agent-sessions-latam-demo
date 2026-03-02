# 📖 Guía de APIs para Balizas V16

Este documento describe la API utilizada para obtener datos de balizas V16 activas en España.

---

## 🚨 DGT — Plataforma DGT 3.0 (DATEX II)

### Descripción
La Dirección General de Tráfico (DGT) de España publica un feed en formato DATEX II (XML) con todas las incidencias de tráfico activas, incluyendo las balizas V16 conectadas.

### Nivel de Acceso
- **100% gratuito** — No requiere API key ni registro
- Feed público accesible vía HTTP GET
- Formato: XML (DATEX II estándar europeo)
- Actualización: Cada 2-5 minutos

### Endpoint
```
GET https://nap.dgt.es/datex2/v3/dgt/SituationPublication/datex2_v36.xml
```

### Datos Disponibles
| Campo | Descripción |
|-------|-------------|
| `situationRecord.id` | Identificador único de la incidencia |
| `latitude` / `longitude` | Coordenadas GPS de la baliza |
| `overallStartTime` | Hora de activación |
| `validityStatus` | Estado (`active`, `suspended`, etc.) |
| `causeType` | Tipo de causa (`vehicleObstruction` = baliza V16) |
| `vehicleObstructionType` | Tipo de obstrucción |
| `roadName` | Nombre de la carretera |
| `municipality` | Municipio |
| `province` | Provincia |

### Cómo Filtrar Balizas V16
Las balizas V16 se identifican por:
- `causeType` = `vehicleObstruction`
- `validityStatus` = `active`

### Restricciones
- **CORS:** El endpoint tiene restricciones CORS, por lo que no se puede consumir directamente desde el navegador. Se debe consumir desde un backend.
- **Sin autenticación:** No requiere API key ni registro.

### Fuente Oficial
- DGT 3.0: https://www.dgt.es/muevete-con-seguridad/tecnologia-e-innovacion-en-carretera/forma-parte-de-la-dgt-3.0/
- GitHub DGT 3.0: https://github.com/dgt30-esp
- Caso de uso V16: https://github.com/dgt30-esp/Caso-de-uso-1

### API Formal DGT 3.0 (Para Integración Avanzada)
Si necesitas integración directa con la plataforma DGT 3.0:
1. Solicita acceso en: https://www.dgt.es/muevete-con-seguridad/tecnologia-e-innovacion-en-carretera/forma-parte-de-la-dgt-3.0/
2. Requiere: IP whitelisted + certificado X.509
3. Protocolos: REST API y/o MQTT
4. Documentación en GitHub: https://github.com/dgt30-esp/Caso-de-uso-1

---

## 📊 Proyectos de Referencia

| Proyecto | URL | Descripción |
|----------|-----|-------------|
| Mapa Balizas V16 | https://mapabalizasv16.es | Mapa interactivo independiente |
| Balizas en Vivo | https://www.balizasenvivo.es/mapa | Mapa tiempo real |
| V16 Tracker | https://v16tracker.com | Mapa tiempo real |
| balizas-v16 (GitHub) | https://github.com/JDamianCabello/balizas-v16 | Proyecto open source de referencia |
