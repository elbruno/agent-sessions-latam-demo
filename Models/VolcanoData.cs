namespace BalizasV16.Models;

public class VolcanoData
{
    // Semáforo de Alerta Volcánica
    public string AlertLevel { get; set; } = "Amarillo Fase 2";
    public string AlertColor { get; set; } = "#ffcc00";
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Actividad reciente
    public int ExhalationsToday { get; set; }
    public int TremorMinutes { get; set; }
    public int ExplosionsToday { get; set; }
    public int VolcanoTectonicEvents { get; set; }

    // Ubicación del Popocatépetl
    public double Lat { get; set; } = 19.0225;
    public double Lon { get; set; } = -98.6278;
    public double ElevationMeters { get; set; } = 5426;

    // Webcam URLs
    public string TlamacasCamUrl { get; set; } = "https://www1.cenapred.unam.mx/volcan/popocatepetl/imagen/tlamacas_hd.jpg";
    public string YouTubeLiveId { get; set; } = "dq5LX5lq-1Y";

    // Sismos cercanos (radio 50km)
    public List<Earthquake> NearbyEarthquakes { get; set; } = new();

    // Reportes
    public string CenapredReportUrl { get; set; } = "https://www.cenapred.gob.mx/es/reportesVolcanesMX/";
    public string SmithsonianUrl { get; set; } = "https://volcano.si.edu/volcano.cfm?vn=341090";
}
