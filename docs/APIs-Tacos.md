# 📖 Guía de APIs para Tacos Nocturnos

Este documento describe todas las APIs disponibles para localizar puestos de tacos en México, sus niveles gratuitos, limitaciones y cómo obtener las API keys necesarias.

---

## 1. 🗺️ OpenStreetMap / Overpass API (GRATUITA — Sin API Key)

### Descripción
OpenStreetMap (OSM) es una base de datos geográfica colaborativa y abierta. La Overpass API permite consultar datos de OSM con filtros avanzados.

### Nivel Gratuito
- **100% gratuita** — No requiere API key ni registro
- Límite razonable: ~10,000 peticiones/día, ~1 GB de descarga/día
- Uso comercial permitido (con atribución)

### Datos Disponibles
- Nombre del establecimiento
- Coordenadas (lat/lon)
- Horarios de apertura (`opening_hours`)
- Tipo de cocina (`cuisine`)
- Dirección

### Limitaciones
- Los datos dependen de contribuciones voluntarias — no todos los puestos callejeros están mapeados
- Cobertura variable por ciudad
- Sin fotos, ratings ni reviews

### Cómo Usar
No necesitas crear cuenta. La API es pública:
```
POST https://overpass-api.de/api/interpreter
Content-Type: application/x-www-form-urlencoded

data=[out:json][timeout:60];area["name"="México"][admin_level=2];(node["amenity"="fast_food"]["cuisine"~"taco|mexican",i](area););out body;
```

### Tags OSM Relevantes
| Tag | Valores | Descripción |
|-----|---------|-------------|
| `amenity` | `fast_food`, `restaurant` | Tipo de establecimiento |
| `cuisine` | `mexican`, `tacos` | Tipo de cocina |
| `opening_hours` | `Mo-Su 18:00-03:00` | Horario de operación |
| `name` | `Tacos El Pastor` | Nombre del puesto |

---

## 2. 🔍 Google Places API (FREEMIUM — Requiere API Key)

### Descripción
La API más completa para buscar establecimientos. Incluye horarios, fotos, ratings y reviews.

### Nivel Gratuito (desde marzo 2025)
- **5,000 peticiones gratis/mes** para Nearby Search (SKU Pro)
- **10,000 peticiones gratis/mes** para Basic (SKU Essentials)
- Después: ~$32 por 1,000 peticiones

### Datos Disponibles
- Nombre, dirección, coordenadas
- Horarios de apertura detallados
- Rating y número de reviews
- Fotos del establecimiento
- Si está abierto ahora (`openNow`)
- Categorías y tipos

### Cómo Obtener tu API Key

1. **Ve a** [Google Cloud Console](https://console.cloud.google.com/)
2. **Crea una cuenta** de Google si no tienes una
3. **Crea un proyecto nuevo** → Haz clic en "Select a project" → "New Project"
4. **Habilita la Places API:**
   - Ve a "APIs & Services" → "Library"
   - Busca "Places API (New)"
   - Haz clic en "Enable"
5. **Crea credenciales:**
   - Ve a "APIs & Services" → "Credentials"
   - Haz clic en "Create Credentials" → "API Key"
   - Copia tu API Key
6. **Configura facturación:**
   - Ve a "Billing" → Asocia una tarjeta (no se cobra dentro del tier gratuito)
   - Google da $200 USD de crédito gratis los primeros 90 días
7. **Restringe tu API Key** (recomendado):
   - En Credentials → selecciona tu key → "Restrict Key"
   - Restringe por IP o referrer HTTP

### Endpoint Principal
```
POST https://places.googleapis.com/v1/places:searchNearby
Headers:
  X-Goog-Api-Key: TU_API_KEY
  X-Goog-FieldMask: places.displayName,places.location,places.currentOpeningHours
Body:
{
  "includedTypes": ["restaurant"],
  "keyword": "tacos",
  "locationRestriction": {
    "circle": {
      "center": { "latitude": 19.4326, "longitude": -99.1332 },
      "radius": 5000
    }
  },
  "maxResultCount": 20
}
```

---

## 3. 🍽️ Yelp Fusion API (GRATUITA — Requiere API Key)

### Descripción
Excelente cobertura en México. Incluye ratings, reviews y fotos. Muy popular para restaurantes.

### Nivel Gratuito
- **5,000 peticiones gratis/día** (¡muy generoso!)
- Sin costo adicional para uso básico
- No requiere tarjeta de crédito

### Datos Disponibles
- Nombre, dirección, coordenadas
- Rating (1-5 estrellas) y número de reviews
- Categorías del negocio
- Fotos
- Teléfono, URL
- Rango de precios ($, $$, $$$, $$$$)

### Limitaciones
- No incluye horarios detallados en el endpoint de búsqueda (necesitas Business Details)
- Limitado a ciertos países en detalle (México tiene buena cobertura en ciudades grandes)

### Cómo Obtener tu API Key

1. **Ve a** [Yelp Developers](https://www.yelp.com/developers)
2. **Crea una cuenta Yelp** si no tienes una
3. **Ve a** [Manage API Access](https://www.yelp.com/developers/v3/manage_app)
4. **Crea una App:**
   - Nombre: "Tacos Nocturnos"
   - Descripción: "Mapa de puestos de tacos nocturnos"
   - Industria: "Food & Drink"
5. **Copia tu API Key** que aparece en la página
6. **Listo** — No necesitas configurar facturación

### Endpoint Principal
```
GET https://api.yelp.com/v3/businesses/search?term=tacos&location=Mexico+City&categories=mexican
Headers:
  Authorization: Bearer TU_API_KEY
```

---

## 4. 📍 Foursquare Places API (FREEMIUM — Requiere API Key)

### Descripción
Datos de establecimientos con categorías muy detalladas. Tiene una categoría específica para "Taco Place".

### Nivel Gratuito
- **10,000 peticiones gratis/mes** (endpoints Pro)
- Incluye: nombre, coordenadas, dirección, categorías
- **No incluye gratis:** horarios, fotos, ratings (estos son Premium y cuestan ~$18.75/1,000 peticiones)

### Datos Disponibles (Gratis - Pro)
- Nombre, dirección, coordenadas
- Categorías (con ID específico para "Taco Place")
- Teléfono, website

### Datos Premium (Pago)
- Horarios de apertura
- Fotos, tips, ratings
- Popularidad

### Cómo Obtener tu API Key

1. **Ve a** [Foursquare Developer Portal](https://foursquare.com/developers/)
2. **Crea una cuenta** — Haz clic en "Sign Up"
3. **Crea un proyecto** en el Developer Console
4. **Genera una API Key:**
   - Ve a tu proyecto → "API Keys"
   - Copia la key generada
5. **Listo** — El tier gratuito se activa automáticamente

### Endpoint Principal
```
GET https://api.foursquare.com/v3/places/search?near=Mexico+City&query=tacos&categories=13306
Headers:
  Authorization: TU_API_KEY
```
> Nota: `13306` es el ID de categoría para "Taco Place" en la taxonomía de Foursquare.

---

## 5. 🚗 TomTom Search API (FREEMIUM — Requiere API Key)

### Descripción
API de búsqueda de POIs con buena cobertura global. Ideal como complemento.

### Nivel Gratuito
- **2,500 peticiones gratis/día** (no por mes, ¡por día!)
- Sin tarjeta de crédito requerida
- Uso comercial permitido

### Cómo Obtener tu API Key

1. **Ve a** [TomTom Developer Portal](https://developer.tomtom.com/)
2. **Regístrate** — Crea una cuenta gratuita
3. **Crea una aplicación** en el dashboard
4. **Copia tu API Key** desde la sección de aplicaciones

### Endpoint Principal
```
GET https://api.tomtom.com/search/2/search/tacos.json?lat=19.4326&lon=-99.1332&radius=5000&categorySet=7315&key=TU_API_KEY
```
> `7315` = Restaurant category

---

## 📊 Tabla Comparativa

| API | Gratis | Límite | API Key | Horarios | Rating | Fotos | Cobertura MX |
|-----|--------|--------|---------|----------|--------|-------|-------------|
| **Overpass/OSM** | ✅ 100% | ~10K/día | ❌ No necesita | ⚠️ Variable | ❌ | ❌ | ⚠️ Variable |
| **Google Places** | ⚠️ 5K/mes | Luego pago | ✅ Sí | ✅ Excelente | ✅ | ✅ | ✅ Excelente |
| **Yelp Fusion** | ✅ 5K/día | Muy generoso | ✅ Sí | ⚠️ Limitado | ✅ | ✅ | ✅ Buena |
| **Foursquare** | ⚠️ 10K/mes | Luego pago | ✅ Sí | 💰 Premium | 💰 Premium | 💰 Premium | ✅ Buena |
| **TomTom** | ✅ 2.5K/día | Generoso | ✅ Sí | ⚠️ Básico | ⚠️ Básico | ❌ | ⚠️ Regular |

### Recomendación
1. **Empezar con Overpass (OSM)** — Gratis, sin registro, datos abiertos
2. **Agregar Yelp** — Gratis, excelente cobertura, ratings
3. **Complementar con Google Places** — Mejor calidad de horarios, pero limitado gratis
