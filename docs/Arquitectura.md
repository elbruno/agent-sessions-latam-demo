# 🏗️ Arquitectura de la Aplicación

## Descripción General

Aplicación Blazor Server (.NET 10) con dos mapas interactivos en tiempo real:
1. **⚠️ Balizas V16** — Señales de emergencia activas en carreteras de España
2. **🌮 Tacos Nocturnos** — Puestos de tacos abiertos después de las 11 PM en México

## Stack Tecnológico

| Componente | Tecnología |
|------------|------------|
| Framework | .NET 10 — Blazor Server |
| Mapas | Leaflet.js 1.9.4 + OpenStreetMap tiles |
| Tiempo real | SignalR (integrado en Blazor Server) |
| APIs de datos | DGT DATEX II, Overpass API, Yelp, Google Places, Foursquare |
| Configuración | `appsettings.json` para API keys |

## Estructura del Proyecto

```
BalizasV16/
├── Components/
│   ├── Layout/          # MainLayout, NavMenu
│   ├── Pages/
│   │   ├── Home.razor   # Landing page con ambos proyectos
│   │   ├── Map.razor    # Mapa de Balizas V16 (España)
│   │   ├── Tacos.razor  # Mapa de Tacos Nocturnos (México)
│   │   └── Settings.razor # Configuración de API keys
│   ├── App.razor        # Layout principal + Leaflet CSS/JS
│   └── Routes.razor
├── Models/
│   ├── Baliza.cs        # Modelo de baliza V16
│   └── TacoStand.cs     # Modelo de puesto de tacos
├── Services/
│   ├── BalizaService.cs # BackgroundService polling DGT cada 2 min
│   ├── BalizaHub.cs     # SignalR hub para balizas
│   ├── TacoService.cs   # Servicio multi-proveedor (OSM, Yelp, Google, etc.)
│   └── TacoHub.cs       # SignalR hub para tacos
├── docs/
│   ├── APIs-Tacos.md    # Documentación de APIs de tacos
│   ├── APIs-Balizas.md  # Documentación de API de balizas
│   ├── Arquitectura.md  # Este archivo
│   └── Azure.md         # Propuesta de arquitectura cloud
├── wwwroot/
│   └── js/
│       ├── balizaMap.js # Mapa Leaflet para balizas
│       └── tacoMap.js   # Mapa Leaflet para tacos
├── Program.cs           # Registro de servicios y middleware
├── appsettings.json     # Configuración (incluyendo API keys)
└── BalizasV16.csproj
```

## Flujo de Datos

### Balizas V16
```
DGT DATEX II XML ──> BalizaService (polling 2 min) ──> BalizaHub (SignalR) ──> Map.razor ──> Leaflet
```

### Tacos Nocturnos
```
                  ┌─ Overpass API (OSM) ──────┐
User request ──> │  Yelp Fusion API ──────────├──> TacoService ──> TacoHub ──> Tacos.razor ──> Leaflet
                  │  Google Places API ────────│
                  └─ Foursquare Places API ───┘
```

## Proveedores de Datos (Multi-Provider)

La aplicación soporta múltiples proveedores de datos simultáneamente:
- **Overpass/OSM:** Siempre activo (gratuito, sin API key)
- **Yelp:** Activo si se configura API key
- **Google Places:** Activo si se configura API key
- **Foursquare:** Activo si se configura API key

Los resultados se fusionan y deduplicarán por proximidad geográfica.
