# 🗺️ Agent Sessions LATAM Demo — Mapas en Tiempo Real

> Proyecto demo para [**VS Code Live: Agent Sessions Day LATAM**](https://www.youtube.com/watch?v=QVM4PrL44as) — demostrando cómo GitHub Copilot en modo agente puede construir una aplicación geoespacial en tiempo real desde cero.

![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)
![Blazor Server](https://img.shields.io/badge/Blazor-Server-512BD4?logo=blazor)
![SignalR](https://img.shields.io/badge/SignalR-Realtime-blue)
![Leaflet](https://img.shields.io/badge/Leaflet-Maps-199900?logo=leaflet)

## 🎬 Sobre el Evento

**VS Code Live: Agent Sessions Day LATAM** es un evento comunitario enfocado en demostrar el poder del desarrollo asistido por IA usando **GitHub Copilot en Modo Agente** en Visual Studio Code. Este repositorio fue construido en vivo durante la sesión para mostrar cómo las sesiones de agente pueden acelerar la creación de aplicaciones complejas del mundo real.

📺 **Ver la sesión:** [https://www.youtube.com/watch?v=QVM4PrL44as](https://www.youtube.com/watch?v=QVM4PrL44as)

## ✨ Características

Esta aplicación incluye **cuatro mapas interactivos en tiempo real**, cada uno alimentado por fuentes de datos en vivo:

### ⚠️ Balizas V16 — España
- Visualización en tiempo real de **balizas de emergencia V16** en carreteras españolas
- Datos obtenidos de la API **DGT DATEX II** (Dirección General de Tráfico de España)
- Actualización automática cada 2 minutos mediante polling en segundo plano
- Mapa interactivo con Leaflet y detalles de cada baliza

### 🌮 Tacos Nocturnos — México
- Encuentra **puestos de tacos abiertos después de las 11 PM** en México
- **Búsqueda multi-proveedor** combinando resultados de:
  - 🗺️ OpenStreetMap (Overpass API) — siempre activo, no requiere API key
  - 🔴 Yelp Fusion API
  - 🔵 Google Places API
  - 🟣 Foursquare Places API
- Los resultados se fusionan y deduplicarán por proximidad geográfica

### 🔴 Sismos — México
- **Monitoreo sísmico en tiempo real** para México
- Datos de **USGS** y **SSN** (Servicio Sismológico Nacional)
- Actualizaciones en vivo vía SignalR

### 🌋 Popocatépetl — Monitor Volcánico
- **Monitoreo de actividad volcánica en tiempo real** del Popocatépetl
- Datos de **CENAPRED** (Centro Nacional de Prevención de Desastres)
- Alertas y actualizaciones de estado en vivo

## 🏗️ Arquitectura

```
┌─────────────────────────────────────────────────┐
│               Blazor Server (.NET 10)           │
├─────────────┬─────────────┬──────────┬──────────┤
│  Map.razor  │ Tacos.razor │ Sismos   │ Volcán   │
│  (Balizas)  │ (Tacos)     │          │          │
├─────────────┴─────────────┴──────────┴──────────┤
│            SignalR Hubs (Tiempo real)            │
│  BalizaHub · TacoHub · EarthquakeHub · VolcanoHub│
├─────────────────────────────────────────────────┤
│        Servicios en Segundo Plano (Polling)      │
│  BalizaService · EarthquakeService · VolcanoService│
├─────────────────────────────────────────────────┤
│                APIs Externas                     │
│  DGT · Overpass · Yelp · Google · Foursquare    │
│  USGS · SSN · CENAPRED                          │
└─────────────────────────────────────────────────┘
│              Leaflet.js + OpenStreetMap           │
└─────────────────────────────────────────────────┘
```

## 🚀 Cómo Empezar

### Prerrequisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Ejecutar la aplicación

```bash
dotnet run
```

La aplicación estará disponible en `https://localhost:5001` (o el puerto configurado en tus launch settings).

### API Keys (Opcional)

Algunos proveedores de datos requieren API keys. Configúralas en `appsettings.json`:

```json
{
  "ApiKeys": {
    "Yelp": "tu-api-key-de-yelp",
    "GooglePlaces": "tu-api-key-de-google-places",
    "Foursquare": "tu-api-key-de-foursquare"
  }
}
```

> **Nota:** El proveedor Overpass/OpenStreetMap funciona sin API key. Balizas V16 (DGT), sismos (USGS/SSN) y datos volcánicos (CENAPRED) también son gratuitos y no requieren claves.

## 🛠️ Stack Tecnológico

| Componente | Tecnología |
|------------|------------|
| **Framework** | .NET 10 — Blazor Server |
| **Mapas** | Leaflet.js 1.9.4 + OpenStreetMap tiles |
| **Tiempo real** | SignalR (integrado con Blazor Server) |
| **APIs de datos** | DGT DATEX II, Overpass, Yelp, Google Places, Foursquare, USGS, SSN, CENAPRED |

## 📂 Estructura del Proyecto

```
├── Components/
│   ├── Layout/              # MainLayout, NavMenu
│   └── Pages/
│       ├── Home.razor       # Página principal
│       ├── Map.razor        # Mapa de Balizas V16 (España)
│       ├── Tacos.razor      # Mapa de Tacos Nocturnos (México)
│       ├── Sismos.razor     # Monitor de sismos (México)
│       ├── Popocatepetl.razor # Monitor volcánico
│       └── Settings.razor   # Configuración de API keys
├── Models/                  # Modelos de datos (Baliza, TacoStand, Earthquake, VolcanoData)
├── Services/                # Servicios en segundo plano + hubs SignalR
├── docs/                    # Documentación detallada de APIs y arquitectura
├── wwwroot/                 # Archivos estáticos (JS, CSS)
└── Program.cs               # Configuración de la app y registro de servicios
```

## 🔗 Recursos

- 📺 [VS Code Live: Agent Sessions Day LATAM](https://www.youtube.com/watch?v=QVM4PrL44as)
- 🤖 [Awesome GitHub Copilot](https://github.com/github/awesome-copilot/) — Colección curada de recursos, herramientas y ejemplos de GitHub Copilot
- 🐿️ [Squad](https://github.com/bradygaster/squad) — Framework de agentes multi-rol para GitHub Copilot
- 📖 [Documentación de GitHub Copilot](https://docs.github.com/en/copilot)
- 🟣 [.NET 10](https://dotnet.microsoft.com/download/dotnet/10.0)

## 📄 Licencia

Este proyecto se proporciona como demo con fines educativos.

## 🙏 Créditos

Construido con ❤️ usando **GitHub Copilot en Modo Agente** durante [VS Code Live: Agent Sessions Day LATAM](https://www.youtube.com/watch?v=QVM4PrL44as).

---

> *"La mejor manera de aprender desarrollo asistido por IA es construir algo real."*
