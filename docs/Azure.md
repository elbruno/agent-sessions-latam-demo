# ☁️ Propuesta de Arquitectura Cloud en Azure

## Diseño para Producción

### Componentes Principales

```
┌──────────────────────────────────────────────────────────────────────┐
│                        Azure Front Door / CDN                         │
│                    (Caché estático + SSL + WAF)                       │
└──────────────────────┬───────────────────────────────────────────────┘
                       │
         ┌─────────────┴─────────────┐
         │                           │
    ┌────▼────┐              ┌──────▼──────┐
    │ Azure   │              │ Azure       │
    │ App     │◄────────────►│ SignalR     │
    │ Service │              │ Service     │
    │ (Blazor)│              │ (real-time) │
    └────┬────┘              └─────────────┘
         │
    ┌────▼────────────────────────────────────────┐
    │              Azure Services                  │
    │  ┌──────────┐  ┌──────────┐  ┌──────────┐  │
    │  │ Cosmos   │  │ Redis    │  │ Azure    │  │
    │  │ DB       │  │ Cache    │  │ Functions│  │
    │  │ (datos)  │  │ (caché)  │  │ (polling)│  │
    │  └──────────┘  └──────────┘  └──────────┘  │
    └─────────────────────────────────────────────┘
```

### Detalle de Componentes

| Servicio | Uso | SKU Recomendado | Costo Estimado/mes |
|----------|-----|-----------------|-------------------|
| **App Service** | Blazor Server app | B1 (básico) / S1 (prod) | $13-$70 USD |
| **Azure SignalR** | Push real-time a clientes | Free (20 conexiones) / Standard | $0-$49 USD |
| **Cosmos DB** | Datos de tacos, votos, usuarios | Serverless (400 RU/s free) | $0-$25 USD |
| **Redis Cache** | Caché de respuestas API | C0 (250MB) | $16 USD |
| **Azure Functions** | Timer trigger para polling DGT | Consumption plan | $0 (tier gratuito) |
| **Front Door / CDN** | Caché estático, SSL | Standard | $35 USD |

### Escalabilidad Nocturna (Viernes y Sábado)

```
              Lun-Jue          Viernes           Sábado
              ┌────┐          ┌────────┐        ┌────────┐
Instancias:   │ 1  │          │  2-4   │        │  2-4   │
              └────┘          └────────┘        └────────┘
              6am-12am        6pm-4am           6pm-4am
```

**Configuración de Auto-Scale:**
- **Regla 1:** Scale out cuando CPU > 70% → +1 instancia (máx 4)
- **Regla 2:** Scale in cuando CPU < 30% → -1 instancia (mín 1)
- **Regla 3:** Schedule rule — viernes y sábado 18:00-04:00 → mín 2 instancias

### Estimación de Costos Mensuales

| Escenario | Costo Estimado |
|-----------|---------------|
| **Desarrollo/MVP** (tier gratuito donde posible) | ~$0-30 USD/mes |
| **Producción básica** (1 instancia, sin auto-scale) | ~$80-120 USD/mes |
| **Producción con picos** (auto-scale viernes/sábado) | ~$150-250 USD/mes |

### Seguridad
- API keys almacenadas en **Azure Key Vault** (no en appsettings.json)
- Managed Identity para acceso a Key Vault
- HTTPS obligatorio vía Front Door
- Rate limiting en API backend
