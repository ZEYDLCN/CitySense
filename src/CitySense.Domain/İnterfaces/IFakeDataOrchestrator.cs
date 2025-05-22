
using System.Threading.Tasks;

namespace CitySense.Domain.Interfaces
{
    public interface IFakeDataOrchestrator
    {
        Task InitializeSensorsIfNeededAsync();
        Task GenerateAndSaveSensorDataPointAsync();
    }
}