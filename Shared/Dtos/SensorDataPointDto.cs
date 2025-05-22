// CitySense.Frontend/Services/SensorDataService.cs
// DTO'larınızın namespace'ini ekleyin. Eğer API projesindeki DTO'ları
// bir Shared Library'ye taşımadıysanız, DTO sınıflarını Frontend projesine de kopyalamanız
// veya API'ye referans vermeniz (önerilmez) ya da ayrı bir Shared proje oluşturmanız gerekir.
// Şimdilik DTO'ların Frontend projesinde de tanımlı olduğunu varsayalım:
// Eğer CitySense.Api.Dtos ise, Program.cs'de HttpClient'a BaseAddress set edilirken
// bu DTO'ların Frontend'de de olması gerekir.
// En iyi pratik, DTO'ları bir Shared Library'ye taşımaktır.
// Biz daha önce API projesinde Dtos klasörü oluşturmuştuk.
// O DTO'ları buraya da kopyalayabiliriz veya dediğim gibi Shared Library.
// Şimdilik Frontend altına Models veya Dtos diye bir klasör açıp
// SensorDataPointDto, SensorInfoDto, SensorLocationDto sınıflarını oraya kopyalayalım.

namespace CitySense.Shared.Dtos
{
    public class SensorDataPointDto
    {
        public long Id { get; set; }
        public DateTime Timestamp { get; set; }
        public double Value { get; set; }
        public string ValueType { get; set; }
        public string Unit { get; set; }
        public SensorInfoDto Sensor { get; set; }
    }
}