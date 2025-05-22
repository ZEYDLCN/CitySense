using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace CitySense.Data.Models
{
    public enum DataType
    {
        Traffic,
        AirPollution,
        Noise,
        Parking,
        Weather
    }

    public class SensorDataPoint
    {
        public long Id { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        public int SensorId { get; set; }

        [ForeignKey("SensorId")]
        public virtual Sensor Sensor { get; set; }

        public DataType ValueType { get; set; }

        [Required]
        public double Value { get; set; }

        [MaxLength(50)]
        public string Unit { get; set; }
    }
}
