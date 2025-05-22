// CitySense.Domain/Services/FakeDataOrchestrator.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CitySense.Data;
using CitySense.Data.Models;
using CitySense.Domain.Interfaces;
using CitySense.Shared.Dtos; 
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;

namespace CitySense.Domain.Services
{
    public class FakeDataOrchestrator : IFakeDataOrchestrator
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<FakeDataOrchestrator> _logger;
        private readonly IHubContext<Hub> _hubContext;
        private readonly Random _random = new Random();
        private static List<Sensor> _activeSensors = new List<Sensor>();
        private static readonly object _sensorLock = new object();

        private const double MinLat = 40.8;
        private const double MaxLat = 41.2;
        private const double MinLon = 28.5;
        private const double MaxLon = 29.5;

        public FakeDataOrchestrator(AppDbContext dbContext,
                                    IHubContext<Hub> hubContext,
                                    ILogger<FakeDataOrchestrator> logger = null)
        {
            _dbContext = dbContext;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task InitializeSensorsIfNeededAsync()
        {
            lock (_sensorLock)
            {
                if (_activeSensors.Any())
                {
                    _logger?.LogInformation("Sensors already loaded in static memory cache.");
                    return;
                }
            }
            List<Sensor> sensorsFromDb;
            if (!await _dbContext.Sensors.AnyAsync())
            {
                _logger?.LogInformation("No sensors found in DB. Creating initial fake sensors...");
                var initialSensors = new List<Sensor>
                {
                    new Sensor { Name = "Taksim Gürültü Sensörü", Type = SensorType.NoiseLevel, Location = CreateRandomPoint(), LastUpdateTime = DateTime.UtcNow },
                    new Sensor { Name = "Kadıköy Trafik Sensörü", Type = SensorType.TrafficDensity, Location = CreateRandomPoint(), LastUpdateTime = DateTime.UtcNow },
                    new Sensor { Name = "Beşiktaş Hava Kalite Sensörü", Type = SensorType.AirQuality, Location = CreateRandomPoint(), LastUpdateTime = DateTime.UtcNow },
                    new Sensor { Name = "Üsküdar Park Sensörü", Type = SensorType.ParkingAvailability, Location = CreateRandomPoint(), LastUpdateTime = DateTime.UtcNow },
                    new Sensor { Name = "Sarıyer Sıcaklık Sensörü", Type = SensorType.Temperature, Location = CreateRandomPoint(), LastUpdateTime = DateTime.UtcNow },
                    new Sensor { Name = "Fatih Nem Sensörü", Type = SensorType.Humidity, Location = CreateRandomPoint(), LastUpdateTime = DateTime.UtcNow }
                };
                await _dbContext.Sensors.AddRangeAsync(initialSensors);
                await _dbContext.SaveChangesAsync();
                sensorsFromDb = initialSensors;
                _logger?.LogInformation($"{initialSensors.Count} initial fake sensors created.");
            }
            else
            {
                _logger?.LogInformation("Sensors already exist in DB. Loading them into static memory cache...");
                sensorsFromDb = await _dbContext.Sensors.ToListAsync();
            }

            lock (_sensorLock)
            {
                if (!_activeSensors.Any() && sensorsFromDb != null)
                {
                    _activeSensors.AddRange(sensorsFromDb);
                    _logger?.LogInformation($"{_activeSensors.Count} sensors loaded into static memory cache.");
                }
            }
        }

        public async Task GenerateAndSaveSensorDataPointAsync()
        {
            Sensor chosenSensor;
            lock (_sensorLock)
            {
                if (!_activeSensors.Any())
                {
                    _logger?.LogWarning("No active sensors in static memory cache to generate data for.");
                    return;
                }
                chosenSensor = _activeSensors[_random.Next(_activeSensors.Count)];
            }

            if (chosenSensor == null)
            {
                _logger?.LogError("Chosen sensor was null after random selection.");
                return;
            }

            var sensorInDb = await _dbContext.Sensors.FindAsync(chosenSensor.Id);
            if (sensorInDb == null)
            {
                _logger?.LogError($"Sensor with ID {chosenSensor.Id} not found in DB. Removing from active list.");
                lock (_sensorLock) { _activeSensors.RemoveAll(s => s.Id == chosenSensor.Id); }
                return;
            }

            var dataPointEntity = new SensorDataPoint
            {
                SensorId = sensorInDb.Id,
                Timestamp = DateTime.UtcNow,
                Value = GenerateRandomValue(sensorInDb.Type),
                ValueType = MapSensorTypeToDataType(sensorInDb.Type),
                Unit = GetUnitForSensorType(sensorInDb.Type)
            };

            _dbContext.SensorDataPoints.Add(dataPointEntity);
            sensorInDb.LastUpdateTime = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            _logger?.LogInformation($"Generated data for Sensor ID: {sensorInDb.Id} ({sensorInDb.Name}), Value: {dataPointEntity.Value} {dataPointEntity.Unit}");

            // DTO oluşturma ve SignalR ile gönderme
            var dataPointDto = new SensorDataPointDto
            {
                Id = dataPointEntity.Id,
                Timestamp = dataPointEntity.Timestamp,
                Value = dataPointEntity.Value,
                ValueType = dataPointEntity.ValueType.ToString(),
                Unit = dataPointEntity.Unit,
                Sensor = new SensorInfoDto
                {
                    Id = sensorInDb.Id,
                    Name = sensorInDb.Name,
                    Type = sensorInDb.Type.ToString(),
                    Location = new SensorLocationDto
                    {
                        Latitude = sensorInDb.Location.Y,
                        Longitude = sensorInDb.Location.X
                    },
                    LastUpdateTime = sensorInDb.LastUpdateTime
                }
            };

            await _hubContext.Clients.All.SendAsync("ReceiveSensorDataUpdate", dataPointDto);
            _logger?.LogInformation($"Sent data update via SignalR for Sensor ID: {sensorInDb.Id}");
        }

        private Point CreateRandomPoint()
        {
            var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
            double lat = MinLat + (_random.NextDouble() * (MaxLat - MinLat));
            double lon = MinLon + (_random.NextDouble() * (MaxLon - MinLon));
            return geometryFactory.CreatePoint(new Coordinate(lon, lat));
        }

        private double GenerateRandomValue(SensorType sensorType)
        {
            switch (sensorType)
            {
                case SensorType.NoiseLevel: return _random.Next(30, 120);
                case SensorType.TrafficDensity: return _random.Next(0, 100);
                case SensorType.AirQuality: return _random.Next(0, 300);
                case SensorType.ParkingAvailability: return _random.Next(0, 50);
                case SensorType.Temperature: return Math.Round(_random.NextDouble() * 30 + 5, 1);
                case SensorType.Humidity: return _random.Next(20, 90);
                default: return Math.Round(_random.NextDouble() * 100, 2);
            }
        }

        private DataType MapSensorTypeToDataType(SensorType sensorType)
        {
            switch (sensorType)
            {
                case SensorType.NoiseLevel: return DataType.Noise;
                case SensorType.TrafficDensity: return DataType.Traffic;
                case SensorType.AirQuality: return DataType.AirPollution;
                case SensorType.ParkingAvailability: return DataType.Parking;
                case SensorType.Temperature: return DataType.Weather;
                case SensorType.Humidity: return DataType.Weather;
                default:
                    _logger?.LogError($"Unsupported sensor type for DataType mapping: {sensorType}");
                    throw new ArgumentOutOfRangeException(nameof(sensorType), $"Unsupported sensor type: {sensorType} cannot be mapped to DataType.");
            }
        }

        private string GetUnitForSensorType(SensorType sensorType)
        {
            switch (sensorType)
            {
                case SensorType.NoiseLevel: return "dB";
                case SensorType.TrafficDensity: return "%";
                case SensorType.AirQuality: return "AQI";
                case SensorType.ParkingAvailability: return "slots";
                case SensorType.Temperature: return "°C";
                case SensorType.Humidity: return "%";
                default: return string.Empty;
            }
        }
    }
}