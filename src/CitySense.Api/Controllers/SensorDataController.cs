using Microsoft.AspNetCore.Mvc;
using CitySense.Data;
using CitySense.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System;
using CitySense.Shared.Dtos;

[Route("api/[controller]")]
[ApiController]
public class SensorDataController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<SensorDataController> _logger;

    public SensorDataController(AppDbContext context, ILogger<SensorDataController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("latest")]
    public async Task<ActionResult<IEnumerable<SensorDataPointDto>>> GetLatestSensorData([FromQuery] int count = 100)
    {
        try
        {
            if (count <= 0) count = 100;
            if (count > 1000) count = 1000;

            _logger.LogInformation($"Fetching latest {count} sensor data points from DB for DTO mapping.");

            var latestDataFromDb = await _context.SensorDataPoints
                                        .OrderByDescending(sdp => sdp.Timestamp)
                                        .Include(sdp => sdp.Sensor)
                                        .Take(count)
                                        .ToListAsync();

            if (latestDataFromDb == null || !latestDataFromDb.Any())
            {
                _logger.LogInformation("No sensor data points found from DB.");
                return NotFound("No sensor data points found.");
            }

            _logger.LogInformation($"Mapping {latestDataFromDb.Count} sensor data points to DTOs.");

            var latestDataDto = latestDataFromDb.Select(sdp => new SensorDataPointDto
            {
                Id = sdp.Id,
                Timestamp = sdp.Timestamp,
                Value = HandleInvalidDouble(sdp.Value, $"SensorDataPoint.Value for SDP Id {sdp.Id}"),
                ValueType = sdp.ValueType.ToString(),
                Unit = sdp.Unit,
                Sensor = sdp.Sensor == null ? null : new SensorInfoDto
                {
                    Id = sdp.Sensor.Id,
                    Name = sdp.Sensor.Name,
                    Type = sdp.Sensor.Type.ToString(),
                    LastUpdateTime = sdp.Sensor.LastUpdateTime,
                    Location = sdp.Sensor.Location == null ? null : new SensorLocationDto
                    {
                        Longitude = HandleInvalidDouble(sdp.Sensor.Location?.X, $"Sensor.Location.X for SensorId {sdp.Sensor.Id}"),
                        Latitude = HandleInvalidDouble(sdp.Sensor.Location?.Y, $"Sensor.Location.Y for SensorId {sdp.Sensor.Id}")
                    }
                }
            }).ToList();

            _logger.LogInformation("Successfully mapped data to DTOs.");
            return Ok(latestDataDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching and mapping latest sensor data to DTO.");
            return StatusCode(500, "An internal server error occurred while processing your request. Details have been logged.");
        }
    }

    private double HandleInvalidDouble(double? value, string fieldNameForLog)
    {
        if (value.HasValue && (double.IsNaN(value.Value) || double.IsInfinity(value.Value)))
        {
            _logger.LogWarning($"Invalid double value ('{value.Value}') detected in field '{fieldNameForLog}'. Defaulting to 0.0 for JSON serialization.");
            return 0.0;
        }
        return value ?? 0.0;
    }
}
