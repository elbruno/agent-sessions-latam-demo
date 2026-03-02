# 🗺️ Agent Sessions LATAM Demo — Mapas en Tiempo Real

> Demo project for [**VS Code Live: Agent Sessions Day LATAM**](https://www.youtube.com/watch?v=QVM4PrL44as) — showcasing how GitHub Copilot agent mode can build a full-stack real-time geospatial application from scratch.

![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)
![Blazor Server](https://img.shields.io/badge/Blazor-Server-512BD4?logo=blazor)
![SignalR](https://img.shields.io/badge/SignalR-Realtime-blue)
![Leaflet](https://img.shields.io/badge/Leaflet-Maps-199900?logo=leaflet)

## 🎬 About the Event

**VS Code Live: Agent Sessions Day LATAM** is a community event focused on demonstrating the power of AI-assisted development using **GitHub Copilot Agent Mode** in Visual Studio Code. This repository was built live during the session to showcase how agent sessions can accelerate the creation of complex, real-world applications.

📺 **Watch the session:** [https://www.youtube.com/watch?v=QVM4PrL44as](https://www.youtube.com/watch?v=QVM4PrL44as)

## ✨ Features

This application includes **four real-time interactive maps**, each powered by live data sources:

### ⚠️ Balizas V16 — España
- Real-time display of **V16 emergency beacons** on Spanish roads
- Data sourced from **DGT DATEX II** API (Spain's traffic authority)
- Auto-refreshes every 2 minutes via background polling
- Interactive Leaflet map with beacon details

### 🌮 Tacos Nocturnos — México
- Find **taco stands open after 11 PM** in Mexico
- **Multi-provider search** combining results from:
  - 🗺️ OpenStreetMap (Overpass API) — always active, no key required
  - 🔴 Yelp Fusion API
  - 🔵 Google Places API
  - 🟣 Foursquare Places API
- Results are merged and deduplicated by geographic proximity

### 🔴 Sismos — México
- **Real-time earthquake monitoring** for Mexico
- Data from **USGS** and **SSN** (Servicio Sismológico Nacional)
- Live updates via SignalR

### 🌋 Popocatépetl — Monitor Volcánico
- **Real-time volcanic activity** monitoring for Popocatépetl
- Data from **CENAPRED** (Centro Nacional de Prevención de Desastres)
- Live alerts and status updates

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────┐
│               Blazor Server (.NET 10)           │
├─────────────┬─────────────┬──────────┬──────────┤
│  Map.razor  │ Tacos.razor │ Sismos   │ Volcán   │
│  (Balizas)  │ (Tacos)     │          │          │
├─────────────┴─────────────┴──────────┴──────────┤
│              SignalR Hubs (Real-time)            │
│  BalizaHub · TacoHub · EarthquakeHub · VolcanoHub│
├─────────────────────────────────────────────────┤
│           Background Services (Polling)          │
│  BalizaService · EarthquakeService · VolcanoService│
├─────────────────────────────────────────────────┤
│              External APIs                       │
│  DGT · Overpass · Yelp · Google · Foursquare    │
│  USGS · SSN · CENAPRED                          │
└─────────────────────────────────────────────────┘
│              Leaflet.js + OpenStreetMap           │
└─────────────────────────────────────────────────┘
```

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Run the application

```bash
dotnet run
```

The app will be available at `https://localhost:5001` (or the port configured in your launch settings).

### API Keys (Optional)

Some data providers require API keys. Configure them in `appsettings.json`:

```json
{
  "ApiKeys": {
    "Yelp": "your-yelp-api-key",
    "GooglePlaces": "your-google-places-api-key",
    "Foursquare": "your-foursquare-api-key"
  }
}
```

> **Note:** The Overpass/OpenStreetMap provider works without any API key. Balizas V16 (DGT), earthquakes (USGS/SSN), and volcano data (CENAPRED) are also free and require no keys.

## 🛠️ Tech Stack

| Component | Technology |
|-----------|-----------|
| **Framework** | .NET 10 — Blazor Server |
| **Maps** | Leaflet.js 1.9.4 + OpenStreetMap tiles |
| **Real-time** | SignalR (integrated with Blazor Server) |
| **Data APIs** | DGT DATEX II, Overpass, Yelp, Google Places, Foursquare, USGS, SSN, CENAPRED |

## 📂 Project Structure

```
├── Components/
│   ├── Layout/              # MainLayout, NavMenu
│   └── Pages/
│       ├── Home.razor       # Landing page
│       ├── Map.razor        # Balizas V16 map (Spain)
│       ├── Tacos.razor      # Tacos Nocturnos map (Mexico)
│       ├── Sismos.razor     # Earthquake monitor (Mexico)
│       ├── Popocatepetl.razor # Volcano monitor
│       └── Settings.razor   # API key configuration
├── Models/                  # Data models (Baliza, TacoStand, Earthquake, VolcanoData)
├── Services/                # Background services + SignalR hubs
├── docs/                    # Detailed API & architecture documentation
├── wwwroot/                 # Static assets (JS, CSS)
└── Program.cs               # App configuration & service registration
```

## 📄 License

This project is provided as a demo for educational purposes.

## 🙏 Credits

Built with ❤️ using **GitHub Copilot Agent Mode** during [VS Code Live: Agent Sessions Day LATAM](https://www.youtube.com/watch?v=QVM4PrL44as).

---

> *"The best way to learn AI-assisted development is to build something real."*
