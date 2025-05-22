
namespace CitySense.Shared.Dtos
{
    public class SensorInfoDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public SensorLocationDto Location { get; set; }
        public DateTime LastUpdateTime { get; set; }
    }
}