using System.ComponentModel.DataAnnotations;
using NetTopologySuite.Geometries;

namespace CitySense.Data.Models
{
    public enum SensorType
    {
        TrafficDensity,
        AirQuality,
        NoiseLevel,
        ParkingAvailability,
        Temperature,
        Humidity
    }

    public class Sensor
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public SensorType Type { get; set; }

        [Required]
        public Point Location { get; set; }

        public DateTime LastUpdateTime { get; set; }

        // public virtual ICollection<SensorDataPoint> SensorDataPoints { get; set; }
    }
}
