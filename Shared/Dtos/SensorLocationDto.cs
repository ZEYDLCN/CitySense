// CitySense.Frontend/Services/SensorDataService.cs
// DTO'larınızın namespace'ini ekleyin. Eğer API projesindeki DTO'ları
// bir Shared Library'ye taşımadıysanız, DTO sınıflarını Frontend projesine de kopyalamanız
// veya API'ye referans vermeniz (önerilmez) ya da ayrı bir Shared proje oluşturmanız gerekir.
// Şimdilik DTO'ların Frontend projesinde de tanımlı olduğunu varsayalım:
// Eğer CitySense.Api.Dtos ise, Program.cs'de HttpClient'a BaseAddress set edilirken
// bu DTO'ların Frontend'de de olması gerekir.


namespace CitySense.Shared.Dtos
{
    
    public class SensorLocationDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}